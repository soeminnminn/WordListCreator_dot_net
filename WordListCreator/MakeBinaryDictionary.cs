using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using System.Xml;

namespace WordListCreator
{
    public class MakeBinaryDictionary
    {
        #region Variables
        private const char DEAD_KEY_PLACEHOLDER = (char)0x25cc;

        private const int ALPHA_SIZE = 256;
        private const string TAG_WORD = "w";
        private const string ATTR_FREQ = "f";

        private const int FLAG_ADDRESS_MASK = 0x400000;
        private const int FLAG_TERMINAL_MASK = 0x800000;
        private const int ADDRESS_MASK = 0x3fffff;

        private const int CHAR_WIDTH = 8;
        private const int FLAGS_WIDTH = 1;
        private const int ADDR_WIDTH = 23;
        private const int FREQ_WIDTH_BYTES = 1;
        private const int COUNT_WIDTH_BYTES = 1;

        private static CharNode EMPTY_NODE = new CharNode();

        private static int sNodes = 0;
        private int m_wordCount = 0;

        private int m_nullChildrenCount = 0;
        private int m_notTerminalCount = 0;

        private ArrayList m_roots = null;
        private byte[] m_dict = null;
        private int m_dictSize = 0;
        #endregion

        #region Constructor/Destructor
        public MakeBinaryDictionary()
        {
        }

        public MakeBinaryDictionary(string srcFilename, string destFilename)
        {
            this.m_nullChildrenCount = 0;
            this.m_notTerminalCount = 0;
            this.PopulateDictionary(srcFilename);
            this.WriteToDict(destFilename);
        }
        #endregion

        #region Private Methods
        private void PopulateDictionary(string filename)
        {
            this.m_roots = new ArrayList();
            this.m_wordCount = 0;
            try
            {
                FileInfo fileInfo = new FileInfo(filename);
                if (!fileInfo.Exists) return;

                XmlReader reader = XmlReader.Create(fileInfo.OpenRead());
                string word = string.Empty;
                int freq = 99;
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == TAG_WORD)
                        {
                            string freqStr = reader.GetAttribute(ATTR_FREQ);
                            if (!string.IsNullOrEmpty(freqStr))
                            {
                                freq = int.Parse(freqStr);
                            }
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.Text)
                    {
                        word = reader.Value;
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (!string.IsNullOrEmpty(word))
                        {
                            this.m_wordCount++;
                            this.AddWordTop(word, freq);
                        }
                        word = string.Empty;
                        freq = 99;
                    }
                }
            }
            catch (Exception ioe)
            {
                Console.WriteLine(string.Format("Exception in parsing : {0}", ioe));
            }

            Console.WriteLine(string.Format("Words = {0}, Nodes = {1}", this.m_wordCount, sNodes));
        }

        private int IndexOf(ArrayList children, char c)
        {
            if (children == null)
                return -1;

            for (int i = 0; i < children.Count; i++)
                if (((CharNode)children[i]).data == c)
                    return i;

            return -1;
        }

        private void AddWordTop(string word, int occur)
        {
            if (occur > 255)
                occur = 255;

            char firstChar = word[0];
            int index = this.IndexOf(this.m_roots, firstChar);
            if (index == -1)
            {
                CharNode newNode = new CharNode();
                newNode.data = firstChar;
                newNode.freq = occur;
                index = this.m_roots.Count;
                this.m_roots.Add(newNode);
            }
            else
            {
                ((CharNode)this.m_roots[index]).freq += occur;
            }

            if (word.Length > 1)
                this.AddWordRec((CharNode)this.m_roots[index], word, 1, occur);
            else
                ((CharNode)this.m_roots[index]).terminal = true;
        }

        private void AddWordRec(CharNode parent, string word, int charAt, int occur)
        {
            CharNode child = null;
            char data = word[charAt];

            if (parent.children == null)
            {
                parent.children = new ArrayList();
            }
            else
            {
                int i = 0;
                do
                {
                    if (i >= parent.children.Count)
                        break;
                    CharNode node = (CharNode)parent.children[i];
                    if (node.data == data)
                    {
                        child = node;
                        break;
                    }
                    i++;
                } while (true);
            }

            if (child == null)
            {
                child = new CharNode();
                parent.children.Add(child);
            }

            child.data = data;
            if (child.freq == 0)
                child.freq = occur;

            if (word.Length > charAt + 1)
            {
                this.AddWordRec(child, word, charAt + 1, occur);
            }
            else
            {
                child.terminal = true;
                child.freq = occur;
            }
        }

        private void AddCount(int count)
        {
            this.m_dict[this.m_dictSize++] = (byte)(0xff & count);
        }

        private void AddNode(CharNode node)
        {
            int charData = 0xffff & node.data;

            if (charData > 254)
            {
                this.m_dict[this.m_dictSize++] = 0xff; //-1;
                this.m_dict[this.m_dictSize++] = (byte)(node.data >> 8 & 0xff);
                this.m_dict[this.m_dictSize++] = (byte)(node.data & 0xff);
            }
            else
            {
                this.m_dict[this.m_dictSize++] = (byte)(0xff & node.data);
            }

            if (node.children != null)
                this.m_dictSize += 3;
            else
                this.m_dictSize++;

            if ((0xffffff & node.freq) > 255)
                node.freq = 255;

            if (node.terminal)
            {
                byte freq = (byte)(0xff & node.freq);
                this.m_dict[this.m_dictSize++] = freq;
            }
        }

        private void UpdateNodeAddress(int nodeAddress, CharNode node, int childrenAddress)
        {
            if ((this.m_dict[nodeAddress] & 0xff) == 255)
                nodeAddress += 2;

            childrenAddress = ADDRESS_MASK & childrenAddress;

            if (childrenAddress == 0)
                this.m_nullChildrenCount++;
            else
                childrenAddress |= FLAG_ADDRESS_MASK;

            if (node.terminal)
                childrenAddress |= FLAG_TERMINAL_MASK;
            else
                this.m_notTerminalCount++;

            this.m_dict[nodeAddress + 1] = (byte)(childrenAddress >> 16);

            if ((childrenAddress & FLAG_ADDRESS_MASK) != 0)
            {
                this.m_dict[nodeAddress + 2] = (byte)((childrenAddress & 0xff00) >> 8);
                this.m_dict[nodeAddress + 3] = (byte)(childrenAddress & 0xff);
            }
        }

        private void WriteWordsRec(ArrayList children)
        {
            if (children == null || children.Count == 0)
                return;

            int childCount = children.Count;
            this.AddCount(childCount);

            int[] childrenAddresses = new int[childCount];
            for (int j = 0; j < childCount; j++)
            {
                CharNode node = (CharNode)children[j];
                childrenAddresses[j] = this.m_dictSize;
                this.AddNode(node);
            }

            for (int j = 0; j < childCount; j++)
            {
                CharNode node = (CharNode)children[j];
                int nodeAddress = childrenAddresses[j];
                int cacheDictSize = this.m_dictSize;

                this.WriteWordsRec(node.children);
                this.UpdateNodeAddress(nodeAddress, node, node.children == null ? 0 : cacheDictSize);
            }
        }

        private void WriteToDict(string dictFilename)
        {
            this.m_dict = new byte[0x400000];
            this.m_dictSize = 0;
            this.WriteWordsRec(this.m_roots);
            Console.WriteLine(string.Format("Dict Size = {0}", this.m_dictSize));

            try
            {
                FileStream fos = new FileStream(dictFilename, FileMode.CreateNew);
                fos.Write(this.m_dict, 0, this.m_dictSize);
                fos.Close();
            }
            catch (IOException ioe)
            {
                Console.WriteLine(string.Format("Error writing dict file: {0}", ioe));
            }
        }

        private void TraverseDict(int pos, char[] word, int depth)
        {
            int count = this.m_dict[pos++] & 0xff;

            for (int i = 0; i < count; i++)
            {
                char c = (char)(this.m_dict[pos++] & 0xff);
                if (c == (char)0x377) //'\377')
                {
                    c = (char)((this.m_dict[pos] & 0xff) << 8 | this.m_dict[pos + 1] & 0xff);
                    pos += 2;
                }

                word[depth] = c;
                bool terminal = (this.m_dict[pos] & 0x80) > 0;
                int address = 0;
                if ((this.m_dict[pos] & 0x40) > 0)
                {
                    address = (this.m_dict[pos + 0] & 0x40) << 16 | (this.m_dict[pos + 1] & 0xff) << 8 | this.m_dict[pos + 2] & 0xff;
                    pos += 2;
                }

                pos++;
                if (terminal)
                {
                    this.ShowWord(word, depth + 1, this.m_dict[pos] & 0xff);
                    pos++;
                }

                if (address != 0)
                    this.TraverseDict(address, word, depth + 1);
            }
        }

        private void ShowWord(char[] word, int size, int freq)
        {
            Console.WriteLine(string.Format("{0} \n", new string(word, 0, size), ""));
        }
        #endregion

        #region Static Methods
        public static void Usage()
        {
            Console.WriteLine("Usage: makedict <src.xml> <dest.dict>");
        }

        public static void Make(string srcFilename, string destFilename)
        {
            new MakeBinaryDictionary(srcFilename, destFilename);
        }
        #endregion

        #region Nested Types
        private class CharNode
        {
            #region Variables
            public char data = '\0';
            public int freq = 0;
            public bool terminal = false;
            public ArrayList children = null;
            #endregion

            #region Constructor
            public CharNode()
            {
                sNodes++;
            }
            #endregion
        }
        #endregion
    }
}
