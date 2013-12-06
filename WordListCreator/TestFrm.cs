using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WordListCreator
{
    public partial class TestFrm : Form
    {
        public TestFrm()
        {
            InitializeComponent();
            S16.Windows.Forms.UITraceListener.Create(this.listBoxLog, true);
        }

        private delegate void WriteLogCallback(string message);
        private void WriteLog(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new WriteLogCallback(this.WriteLog), message);
                return;
            }
            System.Diagnostics.Trace.WriteLine(message);
        }

        private void cmdBrowse_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                this.txtFilePath.Text = this.openFileDialog.FileName;
            }
        }

        private void cmdProcess_Click(object sender, EventArgs e)
        {
            //Unicode.MMConvert convert = new Unicode.MMConvert();
            //convert.Test();

            string srcPath = this.txtFilePath.Text;
            if (string.IsNullOrEmpty(srcPath)) return;
            string output = this.GetNewFileName(Path.GetDirectoryName(srcPath), Path.GetFileNameWithoutExtension(srcPath), ".xml");
            this.WriteLog(output);

            string[] args = new string[2];
            args[0] = this.txtFilePath.Text;
            args[1] = output;

            this.backgroundWorker.RunWorkerAsync(args);
        }

        private string GetNewFileName(string folderPath, string fileName, string extension)
        {
            string filePath = Path.Combine(folderPath, fileName + extension);

            int index = 0;
            while (File.Exists(filePath))
            {
                index++;
                filePath = Path.Combine(folderPath, string.Format("{0}_{1}{2}", fileName, index, extension));
            }

            return filePath;
        }

        private void PaseBinaryDictionary(string dictFile, string xmlFile)
        {
            if (string.IsNullOrEmpty(dictFile)) return;
            if (string.IsNullOrEmpty(xmlFile)) return;
            string output = xmlFile;

            ReadBinaryDictionary rbd = new ReadBinaryDictionary(dictFile, output);
            rbd.Dispose();

            this.WriteLog("Done");
        }

        #region SaiDict -- 20779 records 20778
        private const int DEF_FREQ = 99;
        private Unicode.MMConvert mMMConvert = null;
        private Unicode.ZawgyiInterpreter mZawgyiInterpreter = null;
        private StreamWriter mWriter = null;

        private bool InsertSaiDict(string[] dataArray)
        {
            string mmText = dataArray[0];

            if (this.mMMConvert == null)
            {
                this.mMMConvert = new Unicode.MMConvert();
            }

            if (this.mZawgyiInterpreter == null)
            {
                this.mZawgyiInterpreter = new Unicode.ZawgyiInterpreter();
            }

            if (string.IsNullOrEmpty(mmText)) return false;
            if (mmText.Contains("("))
            {
                mmText = mmText.Substring(0, mmText.IndexOf('('));
            }

            string cText = this.mMMConvert.NewZawGyiToZawGyi(mmText);
            this.mZawgyiInterpreter.Interpret(cText);
            string iText = this.mZawgyiInterpreter.Text;
            string text = string.Format("  <w f=\"{1}\">{0}</w>", iText, DEF_FREQ);

            this.mWriter.WriteLine(text);

            return true;
        }

        private void ImportSaiDict(string filePath)
        {
            try
            {
                int count = 0;
                int current = 0;
                StreamReader reader = new StreamReader(File.OpenRead(filePath));
                string line = reader.ReadLine(); // Skip first line

                if (!string.IsNullOrEmpty(line))
                {
                    line = reader.ReadLine();
                    while (!string.IsNullOrEmpty(line))
                    {
                        current++;
                        string[] dataArray = line.Split('\t');
                        if (dataArray.Length == 3)
                        {
                            if (this.InsertSaiDict(dataArray))
                            {
                                count++;
                                this.WriteLog("Imported " + count.ToString() + " record(s).");
                                line = reader.ReadLine();
                                continue;
                            }
                        }

                        this.WriteLog(string.Format("saidict >>> {0} >>> {1}", current, line));
                        line = reader.ReadLine();
                    }
                }

                reader.Close();
                reader.Dispose();

                this.WriteLog("Successful imported saidict data , records count :" + count.ToString());
            }
            catch (Exception ex)
            {
                this.WriteLog(string.Format("ERROR >>> {0}", ex.Message));
            }
        }

        private void ImportSaiDict(string input, string output)
        {
            using (this.mWriter = new StreamWriter(output, false, System.Text.Encoding.Unicode))
            {
                this.mWriter.WriteLine("<wordlist>");
                this.ImportSaiDict(input);
                this.mWriter.WriteLine("</wordlist>");
                this.mWriter.Flush();
            }
        }
        #endregion

        private void RemoveDistinctLine(string input, string output)
        {
            using (StreamReader reader = new StreamReader(input, System.Text.Encoding.Unicode))
            {
                using (StreamWriter writer = new StreamWriter(output, false, System.Text.Encoding.Unicode))
                {
                    writer.WriteLine("<wordlist>");

                    string prevLine = "";
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (prevLine != line)
                        {
                            writer.WriteLine(line);
                            prevLine = line;
                        }
                    }

                    writer.WriteLine("</wordlist>");
                    writer.Flush();
                }
            }
        }

        private void EncodeLine(string input, string output)
        {
            using (StreamReader reader = new StreamReader(input, System.Text.Encoding.Unicode))
            {
                using (StreamWriter writer = new StreamWriter(output, false, System.Text.Encoding.Unicode))
                {
                    writer.WriteLine("<wordlist>");

                    string prevLine = "";
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (prevLine != line)
                        {
                            writer.WriteLine(Unicode.MMConvert.ToXmlUnicode(line));
                            prevLine = line;
                        }
                    }

                    writer.WriteLine("</wordlist>");
                    writer.Flush();
                }
            }
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] args = (string[])e.Argument;
            string input = args[0];
            string output = args[1];

            this.PaseBinaryDictionary(input, output);
            //this.ImportSaiDict(input, output);
            //this.RemoveDistinctLine(input, output);
            //this.EncodeLine(input, output);
            //MakeBinaryDictionary.Make(input, output);
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.WriteLog("Done");
        }
    }
}
