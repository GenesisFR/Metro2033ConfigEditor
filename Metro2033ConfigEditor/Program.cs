using System;
using System.Windows.Forms;

namespace Metro2033ConfigEditor
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
            Application.Run(new Metro2033ConfigEditorForm());
        }
    }
}
