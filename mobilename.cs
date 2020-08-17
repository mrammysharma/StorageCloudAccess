using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for mobilename
/// </summary>
public class mobilename
{
    public mobilename()
    {

    }
    public List<filesformobile> getitems()
    {   
        var service = HttpContext.Current.Session["service"] as DriveService;
        FilesResource.ListRequest request = service.Files.List();
       FileList files =request.Execute();
        IList<File> fitems= files.Items;
        List<filesformobile> ls = new List<filesformobile>();
        foreach (var item in fitems)
        {
            if (item.OriginalFilename == null || item.OriginalFilename.Equals(""))
                continue;
            ls.Add(new filesformobile() { name = item.OriginalFilename , url = item.DownloadUrl ,id=item.Id});
        }

            return ls;
    }
}