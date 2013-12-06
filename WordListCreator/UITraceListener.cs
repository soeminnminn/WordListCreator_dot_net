using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

// Usage:
//public partial class MyForm : Form
//{
//    private TextboxTraceListener textboxTraceListener;
//    public MyForm()
//    {
//        InitializeComponent();
//        textboxTraceListener = new TextboxTraceListener(traceTextBox);
//    }
//    // ...
//    public MyMethod()
//    {
//            System.Diagnostics.Trace.WriteLine("trace value", "trace category");
//    }
//    // ...
//}

namespace S16.Windows.Forms
{
    public class UITraceListener : TraceListener
    {
        #region Variables
        private Control m_ctrl = null;
        private ConsoleListener m_consoleListener = null;
        protected delegate void Delegate();
        #endregion

        #region Constructor/Destructor
        public UITraceListener(Control ctrl)
            : this(ctrl, false)
        {
        }

        public UITraceListener(Control ctrl, bool withConsole)
        {
            this.m_ctrl = ctrl;
            TraceListenerCollection traceListeners = Trace.Listeners;
            if (!traceListeners.Contains(this))
            {
                traceListeners.Add(this);
            }

            if (withConsole)
            {
                this.m_consoleListener = new ConsoleListener(this);
                Console.SetOut(this.m_consoleListener);
            }
        }
        #endregion

        #region Override Methods
        public override void Write(string message)
        {
            this.RunOnUiThread(delegate
            {
                this.WriteInUiThread(message);
            }
            );
        }

        public override void WriteLine(string message)
        {
            this.Write(message);
        }
        #endregion

        #region Private Methods
        private void WriteInUiThread(string message)
        {
            try
            {
                // accessing m_ctrl.Text is not fast but works for small trace logs
                if (this.m_ctrl is ListBox)
                {
                    ListBox listBox = (ListBox)this.m_ctrl;

                    int index = listBox.Items.Add(message);
                    listBox.SelectedIndex = index;
                }
                else if (this.m_ctrl is TextBox)
                {
                    TextBox textBox = (TextBox)this.m_ctrl;

                    textBox.Text = textBox.Text + message;
                    textBox.SelectionLength = 0;
                    textBox.SelectionStart = textBox.Text.Length;
                }
            }
            catch
            {
            }
            finally
            {
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        #endregion

        #region Protected Methods
        protected virtual void RunOnUiThread(Delegate method)
        {
            try
            {
                this.m_ctrl.Invoke(method);
            }
            catch (System.Exception)
            {
                try
                {
                    this.m_ctrl.BeginInvoke(method);
                }
                catch (System.Exception)
                {
                }
            }
        }
        #endregion

        #region Static Methods
        public static void Create(Control ctrl)
        {
            new UITraceListener(ctrl);
        }

        public static void Create(Control ctrl, bool withConsole)
        {
            new UITraceListener(ctrl, withConsole);
        }
        #endregion

        #region Nested Type
        private class ConsoleListener : System.IO.StringWriter
        {
            #region Variables
            private UITraceListener m_listener = null;
            #endregion

            #region Constructor
            public ConsoleListener(UITraceListener listener)
            {
                this.m_listener = listener;
            }
            #endregion

            #region Override Methods
            public override System.Text.StringBuilder GetStringBuilder()
            {
                if (this.m_listener != null)
                {
                    return new System.Text.StringBuilder(this.m_listener.m_ctrl.Text);
                }

                return new System.Text.StringBuilder();
            }

            public override void Write(char value)
            {
                if (this.m_listener != null)
                {
                    this.m_listener.Write(value.ToString());
                }
            }

            public override void Write(string value)
            {
                this.m_listener.Write(value);
            }

            public override void Write(char[] buffer, int index, int count)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException("buffer");
                }
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                if (count < 0)
                {
                    throw new ArgumentOutOfRangeException("count");
                }
                if ((buffer.Length - index) < count)
                {
                    throw new ArgumentException();
                }

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append(buffer, index, count);
                this.m_listener.Write(sb.ToString());
            }
            #endregion
        }
        #endregion
    }
}
