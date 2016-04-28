using System;
using System.IO;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Security.Principal;
using NDesk.Options;

namespace WPSetup
{
    public class FolderSetup
    {
        public static string[] ReadFile(string path)
        {
            string fullPath = Path.GetFullPath(path);
            try
            {
                string[] lines = System.IO.File.ReadAllLines(fullPath);
                return lines;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        // Adds an ACL entry on the specified file for the specified account.
        public static void AddDirectorySecurity(string fileName, string account,
            FileSystemRights rights, AccessControlType controlType)
        {
            // Get a FileSecurity object that represents the
            // current security settings.
            DirectorySecurity fSecurity = Directory.GetAccessControl(fileName);

            // Add the FileSystemAccessRule to the security settings.
            fSecurity.AddAccessRule(new FileSystemAccessRule(account,
                rights, controlType));

            // Set the new access settings.
            Directory.SetAccessControl(fileName, fSecurity);

        }

        public static void SetupFolders(string[] users, string[] permissions, string[] projects)
        {
            string currDir = System.IO.Directory.GetCurrentDirectory();

            // The current machine domain.
            string domain = Environment.UserDomainName;

            // Create a folder for each user.
            foreach (var usr in users)
            {
                // Create the folder name
                string fName = System.IO.Path.Combine(currDir, usr);
                System.IO.Directory.CreateDirectory(fName);

                // Create subfolders for each project.               
                foreach (var proj in projects)
                {
                    string pfName = System.IO.Path.Combine(fName, proj);
                    System.IO.Directory.CreateDirectory(pfName);
                    // Add individual groups from file to permissions groups.
                    foreach (var acl in permissions)
                    {
                        string accessGroup = domain + "\\" + acl;

                        try
                        {
                            // Add access to the folder.
                            AddDirectorySecurity(pfName, acl,
                                        FileSystemRights.ReadAndExecute, AccessControlType.Allow);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Failed to add " + acl + " to permissions.");
                            Console.WriteLine(e.Message);
                        }
                    }
                }

                // Set up permissions for each folder.
                // Ensure that every user has access to their own folder.

                // Add the user to access control.
                string userAccess = domain + "\\" + usr;
                try
                {
                    AddDirectorySecurity(fName, usr,
                                    FileSystemRights.ReadAndExecute, AccessControlType.Allow);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to add " + usr + " to permissions.");
                    Console.WriteLine(e.Message);
                }

                // Add individual groups from file to permissions groups.
                foreach (var acl in permissions)
                {
                    string accessGroup = domain + "\\" + acl;

                    try
                    {
                        // Add access to the folder.
                        AddDirectorySecurity(fName, acl,
                                    FileSystemRights.ReadAndExecute, AccessControlType.Allow);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to add " + acl + " to permissions.");
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }

        public static void Usage(OptionSet opts)
        {
            Console.WriteLine("All options must be given (except --help).");
            Console.WriteLine("Usage: ");
            opts.WriteOptionDescriptions(Console.Out);
        }

        public static void Main(string[] args)
        {
            string usersFilePath = "";
            string permFilePath = "";
            string projFilePath = "";
            bool help = false;

            var opts = new OptionSet() {
                { "h|help",  "show this message and exit", v => help = true },
                { "u=|users=", "File containing list of user names.", v => usersFilePath = v },
                { "a=|permissions=", "File containing extra users who must be given permissions to folders.", v => permFilePath = v },
                { "p=|projects=", "File containg names of each project. Subfolders will be created for each project name.", v=> projFilePath = v }
            };

            try
            {
                opts.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `--help' for more information.");
                return;
            }

            if (help)
            {
                Usage(opts);
                return;
            }

            if (String.IsNullOrEmpty(usersFilePath) || String.IsNullOrEmpty(permFilePath) || String.IsNullOrEmpty(projFilePath))
            {
                Usage(opts);
                return;
            }

            try
            {
                string[] users = ReadFile(usersFilePath);
                string[] perm = ReadFile(permFilePath);
                string[] projs = ReadFile(projFilePath);

                SetupFolders(users, perm, projs);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
