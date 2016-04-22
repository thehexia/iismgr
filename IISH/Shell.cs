using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace IISH
{
    // Handles the shell for the user.
    public class Shell
    {
        private IISHandler iis;
        
        public Shell()
        {
            iis = new IISHandler();
        }

        public Shell(string remoteHost)
        {
            iis = new IISHandler(remoteHost);
        }

        // Get the directory of the IIS server.
        public void ListIISRootDir()
        {
            string[] files = Directory.GetDirectories(iis.IISRoot());
            foreach (var file in files)
            {
                Console.WriteLine(file);
            }
        }
    }
}
