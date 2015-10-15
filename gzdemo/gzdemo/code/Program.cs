using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace gzdemo
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
            System.Windows.Forms.Application.EnableVisualStyles();   //这两行实现   XP   可视风格   
            System.Windows.Forms.Application.DoEvents();
            Application.Run(new FormMain());
        }
    }
}
