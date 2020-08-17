using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dropbox.Api;
using Dropbox.Api.Files;
using System.Threading.Tasks;


public class MyClass
{

    public List<File> GetFiles()
    {


            var service = HttpContext.Current.Session["service"] as DriveService;
            FilesResource.ListRequest request = service.Files.List();
            FileList files = request.Execute();
            IList<File> fitems = files.Items;

            var items = fitems.Where(f => f.MimeType != "application/vnd.google-apps.folder").OrderBy(f => f.Title).ToList();
            return items;
       
           
    }

}