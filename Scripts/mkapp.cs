using System;
using System.IO;
using System.Collections.Generic;
using NDesk.Options;
using IISH;
using Microsoft.Web.Administration;

namespace IISUtil
{
  public class MakeApplication
  {
    private static void Usage()
    {
      Console.WriteLine("Usage: ");
      Console.WriteLine("mkapp.exe <virtual-path> <physical-path> <site-name>");
      Console.WriteLine("mkapp.exe <physical-path> <site-name>");
      Console.WriteLine();
      Console.WriteLine(@"<virtual-path>: The virtual path of the application
                          on the web server. If not specified, will be auto
                          generated using the name of the physical path folder.");

      Console.WriteLine("<physical-path>: The location of the file on the disk.");
      Console.WriteLine("<site-name>: Name of the site in IIS. Use double quotes for sites with spaces.");
    }

    // Adds an application found in the given physical path.
    // Virtual directory is user defined.
    private static void
    AddApplication(IISHandler handler, string virPath, string physPath, string siteName)
    {
      string fullPath = Path.GetFullPath(physPath);
      Console.WriteLine("Adding app from dir: " + fullPath);
      Console.WriteLine("With virtual dir: " + virPath);
      Site site = handler.GetSiteByName(siteName);

      if (site != null)
        site.Applications.Add(virPath, fullPath);

      handler.CommitChanges();
    }

    // Adds an application found in the given physical path.
    // Creates the virtual directory name automatically.
    private static void
    AddApplication(IISHandler handler, string physPath, string siteName)
    {
      String[] paths = null;
      if (PathHelper.ContainsWildcard(physPath))
      {
        paths = PathHelper.ExpandWildcardDirectories(physPath);
        foreach (var p in paths)
        {
          Console.WriteLine(p);
        }
      }
      else
      {
        string virPath = "";
        Site site = handler.GetSiteByName(siteName);
        if (site != null)
          virPath = PathHelper.SuggestVirtualDirectory(site, physPath);

        AddApplication(handler, virPath, physPath, siteName);
      }
    }


    public static void Main(string[] args)
    {
      var opts = new OptionSet () {
        { "h|help",  "show this message and exit", v => Usage() },
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

      try
      {
        IISHandler handler = new IISHandler();
        // Handle two possible arguments.
        if (extra.Count == 2)
        {
          AddApplication(handler, extra[0], extra[1]);
        }
        else if (extra.Count == 3)
        {
          AddApplication(handler, extra[0], extra[1], extra[2]);
        }
        else
          Usage();
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }
    }
  }
}
