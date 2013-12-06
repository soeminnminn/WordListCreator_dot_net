using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WordListCreator
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
            //string test = "hello world";
            ////System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(test, @"\b([\w]?)([\w]?)");
            ////string output = System.Text.RegularExpressions.Regex.Replace(test, @"\b([\w]?)([\w]?)", "$2 ");
            //string output = System.Text.RegularExpressions.Regex.Replace(test, @"(\s)", "~");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TestFrm());
		}
	}
}
