using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Metro2033ConfigEditor
{
    class Instance
    {
        private static Mutex _mutex;

        public static bool IsSingleInstance()
        {
            string guid = Marshal.GetTypeLibGuidForAssembly(Assembly.GetExecutingAssembly()).ToString();

            try
            {
                // Try to open an existing mutex
                Mutex.OpenExisting(guid);
            }
            catch
            {
                // If an exception occurred, there is no such mutex
                _mutex = new Mutex(true, guid);

                // Only one instance
                return true;
            }

            // More than one instance
            return false;
        }
    }
}
