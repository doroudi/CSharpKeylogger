using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Explorer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 frm = new Form1();
            frm.Visible = false;
            Application.Run(frm);
        }
    }
}
