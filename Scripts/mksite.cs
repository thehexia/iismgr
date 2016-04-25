using System;
using System.IO;
using System.Collections.Generic;
using NDesk.Options;
using IISH;
using Microsoft.Web.Administration;

namespace IISUtil
{
  public class HandleSite
  {
    private static void Usage(OptionSet opts)
    {
      Console.WriteLine("Usage: ");
      Console.WriteLine("mksite add <site-name> <file-path>");
      Console.WriteLine("mksite remove <site-name>");
      Console.WriteLine("mksite start <site-name>");
      Console.WriteLine("mksite stop <site-name>");
      Console.WriteLine();
      Console.WriteLine("Options: ");
      opts.WriteOptionDescriptions(Console.Out);
    }

    private static void AddSite(IISHandler handler, ApplicationPool appPool, string siteName, string filePath, int port = 80)
    {
      filePath = Path.GetFullPath(filePath);
      handler.AddSite(appPool, siteName, filePath, port);
    }

    private static void AddSite(IISHandler handler, string siteName, string filePath, int port = 80)
    {
      ApplicationPool appPool = handler.AddApplicationPool(siteName);
      AddSite(handler, appPool, siteName, filePath, port);
    }

    private static void RemoveSite(IISHandler handler, string siteName)
    {
      handler.RemoveSite(siteName);
    }

    private static void StartSite(IISHandler handler, string siteName)
    {
      handler.StartSite(siteName);
    }

    private static void StopSite(IISHandler handler, string siteName)
    {
      handler.StopSite(siteName);
    }


    public static void Main(string[] args)
    {
      bool help = false;
      string appPoolName = "";
      int port = 80; // Default to 80.

      var opts = new OptionSet () {
        { "h|help",  "show this message and exit", v => help = true },
        { "pool=", "name of an existing app pool", v => appPoolName = v },
        { "port=", "specific port number", v => int.TryParse(v, out port) },
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
        string kind = "";
        if (extra.Count > 0)
          kind = extra[0];

        IISHandler handler = new IISHandler();

        if (kind.Equals("add"))
        {
          if (!(extra.Count > 2))
            throw new ArgumentException("Invalid number of arguments.");

          // handle adding sites.
          if (!String.IsNullOrEmpty(appPoolName))
          {
            ApplicationPool pool = handler.AddApplicationPool(appPoolName);
            AddSite(handler, pool, extra[1], extra[2], port);
          }
          else
            AddSite(handler, extra[1], extra[2], port);
        }
        else if (kind.Equals("remove"))
        {
          // Remove site.
          if (!(extra.Count > 1))
            throw new ArgumentException("Invalid number of arguments.");

          RemoveSite(handler, extra[1]);
        }
        else if (kind.Equals("start"))
        {
          if (!(extra.Count > 1))
            throw new ArgumentException("Invalid number of arguments.");

          StartSite(handler, extra[1]);
        }
        else if (kind.Equals("stop"))
        {
          if (!(extra.Count > 1))
            throw new ArgumentException("Invalid number of arguments.");

          StopSite(handler, extra[1]);
        }
        else
          throw new ArgumentException("Second argument " + kind + " not valid.");
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }
    }
  }
}
