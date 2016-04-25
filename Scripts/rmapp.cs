using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NDesk.Options;
using IISH;
using Microsoft.Web.Administration;

namespace IISUtil
{
  public class MakeApplication
  {
    private static void Usage(OptionSet opts)
    {
      Console.WriteLine("Usage: ");
      Console.WriteLine("rmapp.exe <app-name> <site-name>");
      Console.WriteLine("rmapp.exe <app-name> <site-name>");
      Console.WriteLine();
      Console.WriteLine(@"<app-name>: Name of the application. Equivalent to virtual path. Ex. /App1");
      Console.WriteLine("<site-name>: Name of the site in IIS. Use double quotes for sites with spaces.");
      Console.WriteLine();
      Console.WriteLine("Options: ");
      opts.WriteOptionDescriptions(Console.Out);
    }

    public static void
    RemoveApplication(IISHandler handler, string appName, string siteName)
    {
      if (appName.Equals("/"))
      {
        Console.WriteLine(appName + " cannot be removed safely. It is the site default. Removing would corrupt website.");
        return;
      }

      String[] paths = null;
      if (PathHelper.ContainsWildcard(appName))
      {
        Site site = handler.GetSiteByName(siteName);
        if (site == null)
        {
          Console.WriteLine("Site " + siteName + " does not exist.");
          return;
        }

        paths = PathHelper.ExpandWildcardDirectories(appName);
        List<Application> apps =
          new List<Application>(site.Applications.Where(p => paths.Contains(p.VirtualDirectories["/"].PhysicalPath)));

        foreach (var app in apps)
        {
          Console.WriteLine("Removing app " + app);
          try
          {
            handler.RemoveApplicationNonCommit(site, app);
          }
          catch (Exception e)
          {
            Console.WriteLine(e.Message);
          }
        }

        handler.CommitChanges();
        return;
      }

      try
      {
        handler.RemoveApplication(siteName, appName);
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }
    }

    public static void
    RemoveApplicationRecursively(IISHandler handler, string appPrefix, string siteName)
    {
      if (appPrefix.Equals("/"))
      {
        Console.WriteLine(appPrefix + " cannot be removed safely. It is the site default. Removing would corrupt website.");
        return;
      }

      try
      {
        Site site = handler.GetSiteByName(siteName);
        if (site == null)
        {
          Console.WriteLine("Site " + siteName + " does not exist.");
          return;
        }

        if (!appPrefix.StartsWith(site.Name))
        {
          appPrefix = site.Name + appPrefix;
        }

        List<Application> apps =
          new List<Application>(site.Applications
                                    .Where(p => p.ToString().StartsWith(appPrefix)));

        foreach (var app in apps)
        {
          Console.WriteLine("Removing app: " + app);
          try
          {
            handler.RemoveApplicationNonCommit(site, app);
          }
          catch (Exception e)
          {
            Console.WriteLine(e.Message);
          }
        }
        handler.CommitChanges();
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }
    }


    public static void Main(string[] args)
    {
      bool recursiveRmv = false;
      bool help = false;

      var opts = new OptionSet () {
        { "h|help",  "show this message and exit", v => help = true },
        { "r|recursive", "recursively remove all sub applications", v => recursiveRmv = true }
      };

      // All arguments that are not options or optional arguments.
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

      try
      {
        IISHandler handler = new IISHandler();
        // Handle two possible arguments.
        if (extra.Count == 2)
        {
          if (recursiveRmv)
            RemoveApplicationRecursively(handler, extra[0], extra[1]);
          else
            RemoveApplication(handler, extra[0], extra[1]);
        }
        else
          Console.WriteLine("Invalid usage. Use --help.");
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }
    }
  }
}
