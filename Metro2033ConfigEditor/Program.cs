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
        static void Main(params string[] args)
        {
            // Enable logging
            if (args.Length != 0 && args[0] == "-log")
                Logger.enabled = true;

            // Make sure another instance is not running
            if (!Helper.IsSingleInstance())
            {
                MessageBox.Show("An instance of this program is already running!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Metro2033ConfigEditorForm());
        }
    }
}
