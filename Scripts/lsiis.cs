using System;
using System.IO;
using System.Collections.Generic;
using NDesk.Options;
using IISH;

namespace IISUtil
{
  public class ListIIS
  {
    private static void Usage()
    {
      Console.WriteLine("Usage: ");
      Console.WriteLine("-a: List sites and applications under them.");
      Console.WriteLine("-site=<site-name>: List applications under a specific web site.");
      Console.WriteLine("-h: help");
    }

    public static void Main(string[] args)
    {
      IISHandler handler = new IISHandler();
      bool optAllApps = false;
      string site = "";

      var opts = new OptionSet () {
        { "h|help",  "show this message and exit", v => Usage() },
        { "a|apps", "List sites and applications under them.", v => optAllApps = true },
        { "site=", " List applications under a specific web site.", v => site = v }
      };

      try
      {
        opts.Parse (args);
      }
      catch (OptionException e) {
        Console.WriteLine (e.Message);
        Console.WriteLine ("Try `--help' for more information.");
        return;
      }

      if (optAllApps && String.IsNullOrEmpty(site))
      {
        handler.ListAllApplications();
      }
      else if (!optAllApps && String.IsNullOrEmpty(site))
      {
        handler.ListSites();
      }
      else if(!String.IsNullOrEmpty(site))
      {
        handler.ListSiteApplications(site);
      }
    }
  }
}
