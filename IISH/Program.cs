using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Administration;
using System.Threading.Tasks;

namespace IISH
{
    class Program
    {
        private IISHandler handler = new IISHandler();

        private void TestGetSite()
        {
            // Test fetching site by name.
            Site s = handler.GetSiteByName("Default Web Site");
            if (s != null)
                Console.WriteLine("Site: " + s.ToString());
            Console.WriteLine();
        }

        private void TestListInfo()
        {
            // Test printing site and app info to screen.
            // handler.ListSites();
            handler.ListAllApplications();
            Console.WriteLine();
        }

        private void TestAppPools()
        {
            var appPool = handler.AddApplicationPool("addPool1");
            handler.ListApplicationPools();
            Console.WriteLine();
            handler.RemoveApplicationPool("addPool1");
            handler.ListApplicationPools();
            Console.WriteLine();
        }

        private void TestAddSite()
        {
            Console.WriteLine("------------------- Add Site -----------------");

            // Add a site named website3 on port 82.
            ApplicationPool appPool = handler.AddApplicationPool("Website3");
            handler.AddSite(appPool, "Website3", @"C:\Users\Hoang Nguyen\Documents\TestSites3", 82);
            handler.ListAllApplications();
        }

        private void TestRemoveSite()
        {
            Console.WriteLine("------------------- Remove Site -----------------");
            Site site = handler.GetSiteByName("Website3");
            handler.RemoveSite(site.Name);
            handler.RemoveApplicationPool("Website3");
            handler.ListAllApplications();
        }

        private void TestAddSiteSamePort()
        {
            Console.WriteLine("------------------- Add Two Sites Same Port -----------------");
            // Add a site named website3 on port 82.
            ApplicationPool appPool1 = handler.AddApplicationPool("Website4");
            //ApplicationPool appPool2 = handler.AddApplicationPool("Website4");
            //handler.RemoveApplicationPool("Website4"));
                
            try
            {
                handler.RemoveSite("Website3");
                handler.RemoveSite("Website4");
            } catch(Exception) { }

            handler.AddSite(appPool1, "Website3", @"C:\Users\Hoang Nguyen\Documents\TestSites2", 82);
            handler.AddSite(appPool1, "Website4", @"C:\Users\Hoang Nguyen\Documents\TestSites3", 82);
            handler.ListAllApplications();
        }

        private void TestStartStopSite()
        {
            handler.StopSite("Website3");
            handler.StartSite("Website4");
            // handler.StopSite("Website2");
        }

        private void Test()
        {
            try
            {
                // Test getting site by name.
                TestGetSite();

                // Test printing site and app info to screen.
                TestListInfo();
                
                // Test adding and removing app pools.
                TestAppPools();

                // Test adding site to server.
                TestAddSite();

                // Test removing site from server.
                TestRemoveSite();

                // Test adding two sites on the same port.
                TestAddSiteSamePort();
                // handler.RemoveSite("Website3");
                // handler.RemoveSite("Website4");

                // test start and stopping site.
                TestStartStopSite();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
        }

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Test();
        }
    }
}
