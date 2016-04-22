using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Microsoft.Web.Administration;


namespace IISH
{
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

        // Print all applications on all sites.
        public void ListAllApplications()
        {
            foreach (var site in Sites())
            {
                ListSiteApplications(site);
            }
        }

        // Get site with specific name.
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

        // Add an app pool for a site.
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
    }
}
