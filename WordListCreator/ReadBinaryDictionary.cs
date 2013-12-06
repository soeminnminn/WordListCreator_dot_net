using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace WordListCreator
{
    public class ReadBinaryDictionary : IDisposable
    {
        #region Variables
        // 22-bit address = ~4MB dictionary size limit, which on average would be about 200k-300k words
        private const int ADDRESS_MASK = 0x3FFFFF;

        // The bit that decides if an address follows in the next 22 bits
        private const int FLAG_ADDRESS_MASK = 0x40;
        // The bit that decides if this is a terminal node for a word. The node could still have children,
        // if the word has other endings.
        private const int FLAG_TERMINAL_MASK = 0x80;

        private const int FLAG_BIGRAM_READ = 0x80;
        private const int FLAG_BIGRAM_CHILDEXIST = 0x40;
        private const int FLAG_BIGRAM_CONTINUED = 0x80;
        private const int FLAG_BIGRAM_FREQ = 0x7F;

        private const int DICTIONARY_VERSION_MIN = 200;
        private const int DICTIONARY_HEADER_SIZE = 2;
        private const int NOT_VALID_WORD = -99;

        private const char QUOTE = '\'';

        private byte[] mDict = null;
        private int mVersion = 0;
        private int mBigram = 0;
        private ushort[] mWord = null;

        private StreamWriter mWriter = null;
        #endregion

        #region Constructor
        public ReadBinaryDictionary(string dictPath, string outputPath)
        {
            mWord = new ushort[128];

            FileInfo fileInfo = new FileInfo(dictPath);
            if (!fileInfo.Exists) return;

            int length = (int)fileInfo.Length;
            mDict = new byte[length];

            using (FileStream fileStream = fileInfo.OpenRead())
            {
                fileStream.Read(mDict, 0, length);
            }

            mWriter = new StreamWriter(outputPath, false, System.Text.Encoding.Unicode);
            mWriter.WriteLine(string.Format("<wordlist version=\"{0}\" bigram=\"{1}\">", mVersion, mBigram));

            getVersionNumber();

            if (checkIfDictVersionIsLatest())
            {
                getWordsRec(DICTIONARY_HEADER_SIZE, 0);
            }
            else
            {
                getWordsRec(0, 0);
            }

            mWriter.WriteLine("</wordlist>");
            mWriter.Flush();
        }
        #endregion

        #region Public Methods
        public void Dispose()
        {
            if (this.mWriter != null)
            {
                this.mWriter.Close();
                this.mWriter.Dispose();
                this.mWriter = null;
            }
        }
        #endregion

        #region Private Methods
        private static void LOGI(string format, params object[] values)
        {
        }

        private void getVersionNumber()
        {
            mVersion = (mDict[0] & 0xFF);
            mBigram = (mDict[1] & 0xFF);
            LOGI("IN NATIVE SUGGEST Version: %d Bigram : %d \n", mVersion, mBigram);
        }

        private bool checkIfDictVersionIsLatest()
        {
            return (mVersion >= DICTIONARY_VERSION_MIN) && (mBigram == 1 || mBigram == 0);
        }

        private ushort getChar(ref int pos)
        {
            ushort ch = (ushort)(mDict[pos++] & 0xFF);
            if (ch == 0xFF)
            {
                ch = (ushort)(((mDict[pos] & 0xFF) << 8) | (mDict[pos + 1] & 0xFF));
                pos += 2;
            }
            return ch;
        }

        private bool getTerminal(ref int pos) 
        { 
            return (mDict[pos] & FLAG_TERMINAL_MASK) > 0; 
        }

        private int getCount(ref int pos) 
        {
            if (mDict.Length > pos)
            {
                return mDict[pos++] & 0xFF;
            }

            return 0;
        }

        private int getAddress(ref int pos)
        {
            int address = 0;
            if ((mDict[pos] & FLAG_ADDRESS_MASK) == 0)
            {
                pos += 1;
            }
            else
            {
                address += (mDict[pos] & (ADDRESS_MASK >> 16)) << 16;
                address += (mDict[pos + 1] & 0xFF) << 8;
                address += (mDict[pos + 2] & 0xFF);
                pos += 3;
            }
            return address;
        }

        private int getFreq(ref int pos)
        {
            int freq = mDict[pos++] & 0xFF;

            if (checkIfDictVersionIsLatest())
            {
                // skipping bigram
                int bigramExist = (mDict[pos] & FLAG_BIGRAM_READ);
                if (bigramExist > 0)
                {
                    int nextBigramExist = 1;
                    while (nextBigramExist > 0)
                    {
                        pos += 3;
                        nextBigramExist = (mDict[pos++] & FLAG_BIGRAM_CONTINUED);
                    }
                }
                else
                {
                    pos++;
                }
            }

            return freq;
        }

        private string toUnicode(ushort[] word, int length)
        {
            StringBuilder builder = new StringBuilder();

            for(int i = 0; i < length; i++)
            {
                ushort ch = word[i];
                //if ((ch >= 0x1000) && (ch <= 0x1200))
                if ((ch < 0x0020) || (ch > 0x007F))
                {
                    builder.AppendFormat(@"\u{0:X4}", ch);
                }
                else
                    builder.Append((char)ch);
            }

            return builder.ToString();
        }

        private bool addWord(ushort[] word, int length, int frequency)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                builder.Append((char)word[i]);
            }
            string text = string.Format("  <w f=\"{1}\">{0}</w>", builder, frequency);
            mWriter.WriteLine(text);
            return true;
        }

        private void getWordsRec(int pos, int depth)
        {
            int count = getCount(ref pos);
            for (int i = 0; i < count; i++)
            {
                // -- at char
                ushort c = getChar(ref pos);
                // -- at flag/add
                bool terminal = getTerminal(ref pos);
                int childrenAddress = getAddress(ref pos);
                // -- after address or flag
                int freq = 1;
                if (terminal) freq = getFreq(ref pos);
                // -- after add or freq

                // If we are only doing completions, no need to look at the typed characters.
                mWord[depth] = c;
                if (terminal)
                {
                    addWord(mWord, depth + 1, freq);
                }
                if (childrenAddress != 0)
                {
                    getWordsRec(childrenAddress, depth + 1);
                }
            }
        }
        #endregion

        #region Nested Types
        #endregion
    }
}
