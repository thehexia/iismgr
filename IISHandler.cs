using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Microsoft.Web.Administration;


namespace IISH
{
    // Expand wildcard paths into a list of files.
    // Taken from: http://stackoverflow.com/questions/381366/is-there-a-wildcard-expansion-option-for-net-apps
    public class PathHelper
    {
      public static string[] ExpandWildcardDirectories(string path)
      {
        // Get directory and file parts of complete relative pattern
        int lastBackslashPos = path.LastIndexOf('\\') + 1;
        string pattern = path.Substring(lastBackslashPos, path.Length - lastBackslashPos);
        string relDir = path.Substring (0, path.Length - pattern.Length);
        if (relDir.Length == 0)
          relDir = ".";
        string absPath = Path.GetFullPath(relDir);
        // Search files mathing the pattern
        string[] dirs = Directory.GetDirectories(absPath, pattern);
        return dirs;
      }

      // Recursively gets all directories in a given path,
      public static List<string> GetDirectoriesRecursively(string path)
      {
        string absPath = Path.GetFullPath(path);
        // Search files mathing the pattern
        List<string> dirs = new List<string>(Directory.GetDirectories(absPath));
        // Needed so foreach iteration actually works.
        List<string> tempDirs = new List<string>(Directory.GetDirectories(absPath));
        foreach (var dirPath in tempDirs)
        {
          dirs.AddRange(GetDirectoriesRecursively(dirPath));
        }

        tempDirs = new List<string>(dirs);
        foreach (var dirPath in tempDirs)
        {
          if (IsExcludedFolder(dirPath))
          {
            dirs.Remove(dirPath);
          }
        }

        return dirs;
      }

      // Checks if a filename is one of the IIS web application required folders.
      // Eg. Bin, App_Browser, App_Code, App_data, App_globalresources,
      // App_localresources, App_themes, App_webreferences
      // For scalability, this will return true if the folder name is ever prefixed
      // with "App_". Function ignores casing.
      public static bool IsExcludedFolder(string path)
      {
        string foldername = Path.GetFileName(path);
        if (foldername.ToLower().StartsWith("app_") || foldername.ToLower().Equals("bin"))
          return true;

        return false;
      }

      public static bool ContainsWildcard(string path)
      {
        return path.Contains("*");
      }

      // Returns a potential virtual directory for an application.
      public static string SuggestVirtualDirectory(Site site, string path)
      {
        if (site.Applications != null)
          if (site.Applications.Count > 0)
          {
            // Get the top level application associated with every site.
            Application top = site.Applications[0];
            var topPath = top.VirtualDirectories["/"].PhysicalPath;
            var appPath = Path.GetFullPath(path);
            var virPath = "";
            if (appPath.StartsWith(topPath))
            {
              virPath = appPath.Replace(topPath, "");
              virPath = virPath.Replace("\\", "/");
              // Remove the trailing / because its not valid.
              if (virPath.EndsWith("/"))
                virPath = virPath.Substring(0, virPath.Length - 1);
            }
            else
            {
              virPath = Path.GetDirectoryName(appPath);
              virPath = new DirectoryInfo(virPath).Name;
              virPath = "/" + virPath;
            }
            return virPath;
          }
        return "";
      }
    }


    public class IISHandler
    {
        // Current server being handled.
        private ServerManager sm;

        // Default ctor.
        public IISHandler()
        {
            // Constructs server manager with default path to applicationHost.config
            sm = new ServerManager();
        }

        // Ctor for remote servers.
        public IISHandler(string remoteHost)
        {
            sm = new ServerManager(remoteHost);
        }

        // Get all sites.
        SiteCollection Sites() { return sm.Sites; }

        public string IISRoot() { return "C:\\Inetpub\\wwwroot"; }

        // Commit changes.
        public void CommitChanges() { sm.CommitChanges(); }

        // Print all the sites on the server.
        public void ListSites()
        {
            foreach (var site in Sites())
            {
                ListSiteInformation(site);
            }
        }

        // Print only site information.
        public void ListSiteInformation(Site site)
        {
            // Get all the ports a site is bound to.
            StringBuilder portBindings = new StringBuilder();
            foreach (var binding in site.Bindings)
            {
                int port = binding.EndPoint.Port;
                portBindings.Append(port);
            }

            Console.WriteLine("Site (" + portBindings + ") ("+ site.State + "): " + site.ToString());
        }

        // Print only site information. If site with name does not exist, prints error.
        public void ListSiteInformation(string name)
        {
            Site site = GetSiteByName(name);
            if (site == null)
            {
                Console.WriteLine("Site " + name + " is invalid or does not exist.");
                return;
            }

            // Get all the ports a site is bound to.
            StringBuilder portBindings = new StringBuilder();
            foreach (var binding in site.Bindings)
            {
                int port = binding.EndPoint.Port;
                portBindings.Append(port);
            }

            Console.WriteLine("Site (" + portBindings + ") ("+ site.State + "): " + site.ToString());
        }

        // Print applications on a given site.
        public void ListSiteApplications(Site site)
        {
            ListSiteInformation(site);
            string indent = "    ";
            foreach (var app in site.Applications)
            {
                string appPoolName = app.ApplicationPoolName;
                Console.WriteLine(indent + "App Name: " + app.ToString());
                // Print metadata about app
                Console.WriteLine(indent + indent + "app pool: " + appPoolName);
                Console.WriteLine(indent + indent + "physical path: " + app.VirtualDirectories["/"].PhysicalPath);
                VirtualDirectoryCollection directories = app.VirtualDirectories;
                foreach (VirtualDirectory directory in directories)
                {
                    Console.WriteLine(indent + indent + "virtual path: " + directory);
                }
                Console.WriteLine();
            }
        }

        // Prints site info from site name. If no such site exists, prints error.
        public void ListSiteApplications(string name)
        {
          Site site = GetSiteByName(name);
          if (site != null)
            ListSiteApplications(site);
          else
            Console.WriteLine("Site " + name + " does not exist.");
        }

        // Print all site information.
        public void ListAllSites()
        {
          foreach (var site in Sites())
          {
            ListSiteInformation(site);
          }
        }

        // Print all applications on all sites.
        public void ListAllApplications()
        {
            foreach (var site in Sites())
            {
                ListSiteApplications(site);
            }
        }

        // Get site with specific name.
        // Returns null if site does not exist.
        public Site GetSiteByName(string name)
        {
            SiteCollection sites = Sites();
            Site target = sites[name];
            return target;
        }

        // List all application pools.
        public void ListApplicationPools()
        {
            foreach (var appPool in sm.ApplicationPools)
            {
                Console.WriteLine("App Pool: " + appPool.Name);
            }
        }

        // Removes an application pool with a given name.
        // WARNING: May invoke dangerous behavior if an application pool services multiple
        // applications/site instances. Avoid removing pools before removing sites.
        public void RemoveApplicationPool(string name)
        {
            // Check to see that there are app pools.
            if (sm.ApplicationPools != null && sm.ApplicationPools.Count > 0)
            {
                var appPool = sm.ApplicationPools.FirstOrDefault(p => p.Name == name);
                if (appPool != null)
                {
                    sm.ApplicationPools.Remove(appPool);
                    sm.CommitChanges();
                }
                else
                {
                    throw new ArgumentException("App pool " + name + " does not exist.");
                }
            }
        }

        // Add an app pool for a site. If it already exists, return the existing one.
        public ApplicationPool AddApplicationPool(string name)
        {
            ApplicationPool appPool = null;
            // Create an application pool.
            if (sm.ApplicationPools != null)
            {
                // If one already exists make sure not to overlap names
                if (sm.ApplicationPools.Count > 0)
                {
                    appPool = sm.ApplicationPools.FirstOrDefault(p => p.Name == name);
                    if (appPool != null)
                    {
                        return appPool;
                    }
                    // If it does not exist, then add it.
                    else
                    {
                        appPool = sm.ApplicationPools.Add(name);
                    }
                }
                // Otherwise just make it.
                else
                {
                    appPool = sm.ApplicationPools.Add(name);
                }
            }

            // Now verify configuration with the app pool.
            if (appPool != null)
            {
                sm.CommitChanges();
            }

            return appPool;
        }

        /// <summary>
        /// Add a site to the IIS server. If a site with that name already exists, it must be removed first.
        /// </summary>
        /// <param name="appPool">The application pool to run the site on.</param>
        /// <param name="name">The name of the website.</param>
        /// <param name="path">The physical path where the site will reside.</param>
        /// <param name="kind">Kind of site. By default http.</param>
        /// <param name="ip">The ip address.</param>
        /// <param name="port">The port to run on. By default *.</param>
        /// <param name="hostName">The host name. By default *</param>
        /// <returns>Returns created site, or an existing site with that name but no changes made.
        /// Null if server manager in invalid state.</returns>
        public Site AddSite(ApplicationPool appPool, string name, string path, int port, string kind ="http", string ip = "*", string hostName="*")
        {
            Site site = null;
            if (sm.Sites != null && sm.Sites.Count > 0)
            {
                site = sm.Sites.FirstOrDefault(s => s.Name == name);
                // Confirm that the site with that name does not yet exist.
                if (site == null)
                {
                    string bindingInfo = string.Format(@"{0}:{1}:{2}", ip, port, hostName);

                    // Add the site to the server manager.
                    site = sm.Sites.Add(name, kind, bindingInfo, path);

                    // Set the app pool for this new site.
                    site.ApplicationDefaults.ApplicationPoolName = appPool.Name;

                    // Save the new web site.
                    sm.CommitChanges();

                    return site;
                }
            }
            return null;
        }

        /// <summary>
        /// Removes a site with the given name. Throws argument exception if site does not exist.
        /// </summary>
        /// <param name="name">The name of the site being removed.</param>
        public void RemoveSite(string name)
        {
            if (sm.Sites != null && sm.Sites.Count > 0)
            {
                Site site = sm.Sites.FirstOrDefault(p => p.Name == name);
                if (site != null)
                {
                    sm.Sites.Remove(site);
                    sm.CommitChanges();
                }
            }
        }

        public void StartSite(Site site)
        {
            if (site.State == ObjectState.Stopped)
                site.Start();
            sm.CommitChanges();
        }

        public void StopSite(Site site)
        {
            if (site.State == ObjectState.Started)
                site.Stop();
            sm.CommitChanges();
        }

        /// <summary>
        /// Start a site with a given name if it exists
        ///
        /// </summary>
        /// <param name="name"></param>
        public void StartSite(string name)
        {
            if (sm.Sites != null && sm.Sites.Count > 0)
            {
                Site site = sm.Sites.FirstOrDefault(p => p.Name == name);
                if (site != null)
                    StartSite(site);
                sm.CommitChanges();
            }
        }

        /// <summary>
        /// Stop a site with a given name if it exists.
        /// </summary>
        /// <param name="name"></param>
        public void StopSite(string name)
        {
            if (sm.Sites != null && sm.Sites.Count > 0)
            {
                Site site = sm.Sites.FirstOrDefault(p => p.Name == name);
                if (site != null)
                    StopSite(site);
                sm.CommitChanges();
            }
        }

        // Gets an application from a given site by name.
        // Returns null if app does not exist.
        public Application GetApplicationByName(Site site, string name)
        {
            Application app = site.Applications[name];
            return app;
        }

        /// <summary>
        /// Adds an application with a given name to a site.
        /// </summary>
        /// <param name="site">Site to add app to.</param>
        /// <param name="physpath">Physical path on disk.</param>
        /// <param name="virpath">Virtual path on server.</param>
        /// <returns></returns>
        public Application AddApplication(string physpath, string virpath, Site site)
        {
            Application app = site.Applications.Add(virpath, physpath);
            sm.CommitChanges();
            return app;
        }

        // Removes an application from a given site.
        public void RemoveApplication(Site site, Application app)
        {
            site.Applications.Remove(app);
            sm.CommitChanges();
        }

        // Removes an application from a given site.
        public void RemoveApplicationNonCommit(Site site, Application app)
        {
            site.Applications.Remove(app);
        }

        // Searches for a site with siteName and an app with appName.
        // Removes app from site.
        public void RemoveApplication(string siteName, string appName)
        {
          Site site = GetSiteByName(siteName);
          if (site == null)
            throw new ArgumentException("Site " + siteName + " does not exist.");

          Console.WriteLine(appName);
          Application app = GetApplicationByName(site, appName);
          if (app == null)
            throw new ArgumentException("Application " + appName + " does not exist inside " + siteName);

          RemoveApplication(site, app);
        }


    }
}
