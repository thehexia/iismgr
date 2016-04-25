using System;
using System.IO;
using System.Collections.Generic;
using NDesk.Options;
using IISH;

namespace IISUtil
{
  public class ListIIS
  {
    private static void Usage(OptionSet opts)
    {
      Console.WriteLine("Lists information about IIS sites and applications.");
      Console.WriteLine("Usage: ");
      Console.WriteLine("lsiis  -  Lists all sites.");
      Console.WriteLine("lsiis <site-name>  -  Lists all applications under a site.");
      Console.WriteLine();
      Console.WriteLine("Options: ");
      opts.WriteOptionDescriptions(Console.Out);
    }

    public static void Main(string[] args)
    {
      IISHandler handler = new IISHandler();
      bool optAllApps = false;
      bool help = false;
      string site = "";

      var opts = new OptionSet () {
        { "h|help",  "show this message and exit", v => help = true },
        { "a|apps", "List sites and applications under them.", v => optAllApps = true },
      };

      List<string> extra;
      try
      {
        extra = opts.Parse (args);
      }
      catch (OptionException e) {
        Console.WriteLine (e.Message);
        Console.WriteLine ("Try `--help' for more information.");
        return;
      }

      if (help)
      {
        Usage(opts);
        return;
      }

      if (extra != null && extra.Count == 1)
        site = extra[0];

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
