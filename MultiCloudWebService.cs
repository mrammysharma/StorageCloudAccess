using System.Web.Services;
using Dropbox.Api;
using System.IO;
using System.Threading.Tasks;
using Dropbox.Api.Files;
using System.Collections.Generic;
using Google.Apis.Drive.v2;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Drive.v2.Data;
using System;
using System.Threading;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Xml;
using System.Net;
using System.Security.Cryptography;
using System.Text;
/// <summary>
/// Summary description for MultiCloudWebService
/// </summary>
//[WebService(Namespace = "http://tempuri.org/")]
//[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
public class MultiCloudWebService : WebService
{

    //Mediafire API Info
    private static string url = "HTTPS://www.mediafire.com/api/1.2/user/get_session_token.php?";
    private static string mf_email = "", mf_password = "", mf_app_id = "", mf_api_key = "", mf_signature;
    private static string mf_secret_key, mf_ekey, mf_pkey, mf_session, mf_response, mf_time, mf_callsig;
    private static string mf_folder_key = "FOLDER_ID"; // Folder to upload files to.
    private string sessiontoken, keyforupload;
    // END

    //Insert Your drop Box Access Token
    string dropboxaccesstoken = "";

    //send file name
    // bytes of files
    // dropbox access Token
    public async Task<String> UploadDropBox(string filename,byte[] b,string token)
    {
        try
        {
            DropboxClient client = new DropboxClient(token);
            Stream strm = new System.IO.MemoryStream(b);
            await client.Files.UploadAsync("/"+filename, null, false, null, false, strm);
            strm.Close();
            return "done db";
        } catch(Exception ex)

        {
            return ex.Message + "dropbox";
        }
    }
 

    public async Task<List<Metadata>> GetDropBoxFilesforweb(string token)
    {
        DropboxClient client = new DropboxClient(token);


        ListFolderResult files = await client.Files.ListFolderAsync("");

        List<Metadata> myfiles = (List<Metadata>)files.Entries;
        for (int i = myfiles.Count - 1; i >= 0; i--)
        {
            if (myfiles[i].IsFolder)
            {
                myfiles.RemoveAt(i);

            }

        }
        
        return myfiles;
    }


    public async Task<string> DownloadDropBox(string filename ,string Token)
    {
        DropboxClient client = new DropboxClient(Token);
        var result = await client.Files.DownloadAsync("/" + filename);
        byte[] filedata = await result.GetContentAsByteArrayAsync();
        FileStream fos = System.IO.File.Create(Server.MapPath("downloads/" + filename));
    
        await fos.WriteAsync(filedata, 0, filedata.Length);
        return "";
            }
    public async Task<String> DownloadDropBox1(string filename,string token)
    {
       DropboxClient client = new DropboxClient(dropboxaccesstoken);

        //var result = await client.Files.DeleteAsync("/" + filename);
        var result = await client.Files.DownloadAsync("/" + filename);

        byte[] filedata = await result.GetContentAsByteArrayAsync();

        FileStream fos = System.IO.File.Create(Server.MapPath("downloads/" + filename));

        await fos.WriteAsync(filedata, 0, filedata.Length);
        fos.Dispose();
        return Server.MapPath("downloaads/" + filedata);
 
    }

    public async Task<bool> deletedropbox(string filename, string token)
    {
        DropboxClient client = new DropboxClient(token);

        var result = await client.Files.DeleteAsync("/" + filename);
        return result.IsDeleted;

    }

  
    //*****************Google Drive API ***********************************//

    public void ConnectToGoogleDriveforweb(string filename,string serviceemail,string projectname)
    {
        string[] scopes = new string[] { DriveService.Scope.Drive }; // Full access
 
                var keyFilePath = Server.MapPath(filename);
                var serviceAccountEmail = serviceemail;
                var certificate = new X509Certificate2(keyFilePath, "Your Password", X509KeyStorageFlags.Exportable);
                var credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(serviceAccountEmail)
                {
                    Scopes = scopes
                }.FromCertificate(certificate));
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = projectname,
                });
                 Session["service"] = service;
       
            }
   


    public List<Google.Apis.Drive.v2.Data.File> GetFileGoogleDrive()
    {
        MyClass obj = new MyClass();
        return obj.GetFiles();
    }

    public async Task< string> uploadFileGoogledrive(DriveService _service, string _uploadFile, string _parent = "")
    {
        if (System.IO.File.Exists(_uploadFile)) 
        {
            Google.Apis.Drive.v2.Data.File body = new Google.Apis.Drive.v2.Data.File();
          
            body.Title = System.IO.Path.GetFileName(_uploadFile);
            body.Description = "File Name " + DateTime.Now.ToString();
            body.MimeType = GetMimeType(_uploadFile);
           
            body.Parents = new List<ParentReference>() { new ParentReference() {  Id=_parent} };

            // File's content.

            byte[] byteArray = System.IO.File.ReadAllBytes(_uploadFile);
            System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
            try
            {
                
                FilesResource.InsertMediaUpload request = _service.Files.Insert(body, stream, GetMimeType(_uploadFile));
                request.Upload();
                await Task.Delay(10000);
                return "File Uploaded";
               
            }
            catch (Exception e)
            {
                return "An error occurred: " + e.Message;
            }
        }
        else {
            return "File does not exist: " + _uploadFile;
        }
    }
    public async Task<string> uploadFileGoogledrivetoweb(DriveService _service,byte[] byteArray, string _uploadFile, string _parent = "")
    {
          if (true ||System.IO.File.Exists(_uploadFile))
        { 
           
            Google.Apis.Drive.v2.Data.File body = new Google.Apis.Drive.v2.Data.File();
            body.Title = System.IO.Path.GetFileName(_uploadFile);
            body.Description = "File Name " + DateTime.Now.ToString();
            body.MimeType = GetMimeType(_uploadFile);
            
            //            body.Parents = new List<ParentReference>() { new ParentReference() { Id = _parent } };
//            uploadFileGoogledrive(_service, _uploadFile);
           
            // File's content.
            byte[] buffer=            System.IO.File.ReadAllBytes(_uploadFile);
            System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
            try
            {
                FilesResource.InsertMediaUpload request = _service.Files.Insert(body, stream, GetMimeType(_uploadFile));
                request.Upload();
await        Task.Delay(1000);
                return "pass File Uploaded";

            }
            catch (Exception e)
            {
                return "fail" + e.Message;
            }
        }
        else
        {
            return "File does not exist: " + _uploadFile;
        }
    }
 

    public String DownloadFileGoogleDrive(string downloadurl, string filename,string email)
{
    
    var service = Session["service"] as DriveService;
      //  service.Files.Delete("abc");
        
        var x = service.HttpClient.GetByteArrayAsync(downloadurl);
        byte[] arrBytes = x.Result;
        string _saveTo = Server.MapPath("downloads/" + filename);
        System.IO.File.WriteAllBytes(_saveTo, arrBytes);

        return "downloads/"+ filename;

}
     public void DeleteGdrivefile(String id,String email)
    {
      
        var service = Session["service"] as DriveService;
        service.Files.Delete("abc");
    }
    private static string GetMimeType(string fileName)
    {
        string mimeType = "application/unknown";
        string ext = System.IO.Path.GetExtension(fileName).ToLower();
        Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
        if (regKey != null && regKey.GetValue("Content Type") != null)
            mimeType = regKey.GetValue("Content Type").ToString();
        return mimeType;
    }

    public static string Mediafire_GetSignature()
    {
        // combine email of user and password with our app id and api key to get signature
        string data = mf_email + mf_password + mf_app_id + mf_api_key;
        // convert them into bytes then encrypt those bytes with SHA1 and remmove '-' from bytes
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        byte[] hash;
        using (SHA1 sha1 = new SHA1Managed())
            hash = sha1.ComputeHash(bytes);
        string hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();
        return hashString;
    }
    public string[] Mediafire_GetInfo()
    {

        //get session token
        mf_response = Mediafire_GetSessionToken();

        //if success is not there
        if (!mf_response.ToLower().Contains("<result>success</result>"))
        {
            // nothing will happen
        }

        else
        {
            // if success is found then we will get session , p_key,e_key with substrings
            string[] sess = { "<session_token>", "</session_token>" };
            string a = "";
            mf_session = mf_response.Substring(mf_response.IndexOf(sess[0]) + sess[0].Length, mf_response.IndexOf(sess[1]) - mf_response.IndexOf(sess[0]) - sess[1].Length + 1);

            /*string[] secret = { "<secret_key>", "</secret_key>" }; // token_version=2 only
            mf_secret_key = mf_response.Substring(mf_response.IndexOf(secret[0]) + secret[0].Length, mf_response.IndexOf(secret[1]) - mf_response.IndexOf(secret[0]) - secret[1].Length + 1);*/
            string[] pkey = { "<pkey>", "</pkey>" };
            mf_pkey = mf_response.Substring(mf_response.IndexOf(pkey[0]) + pkey[0].Length, mf_response.IndexOf(pkey[1]) - mf_response.IndexOf(pkey[0]) - pkey[1].Length + 1);
            string[] ekey = { "<ekey>", "</ekey>" };
            mf_ekey = mf_response.Substring(mf_response.IndexOf(ekey[0]) + ekey[0].Length, mf_response.IndexOf(ekey[1]) - mf_response.IndexOf(ekey[0]) - ekey[1].Length + 1);
            /*string[] time = { "<time>", "</time>" }; // token_version=2 only
            mf_time = mf_response.Substring(mf_response.IndexOf(time[0]) + time[0].Length, mf_response.IndexOf(time[1]) - mf_response.IndexOf(time[0]) - time[1].Length + 1);*/

            //store signature and session key into sessions
            Session["token"] = mf_session;
            Session["signature"] = mf_signature;
            return sess;

        }
        // if nothing then reurn null
        if (mf_session == null || /*mf_secret_key == null || */mf_pkey == null || mf_ekey == null/* || mf_time == null*/)
        {
        }
        return null;

    }



    public string Mediafire_GetSessionToken()
    {

        // make url with user email and password with your app id and app keyy and create http connection 
        string posturl = url + "signature=" + mf_signature + "&email=" + mf_email + "&password=" + mf_password + "&application_key=" + mf_api_key + "&application_id=" + mf_app_id + "&token_version=1&response_format=xml";
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(posturl);
        byte[] bytes;
        bytes = System.Text.Encoding.ASCII.GetBytes(posturl);
        request.ContentType = "text/xml; encoding='utf-8'";
        request.ContentLength = bytes.Length;
        request.Method = "POST";
        Stream requestStream = request.GetRequestStream();
        requestStream.Write(bytes, 0, bytes.Length);
        requestStream.Close();
        HttpWebResponse response;

        response = (HttpWebResponse)request.GetResponse();
        if (response.StatusCode == HttpStatusCode.OK)
        {
            // check wheter response is ok if yes then read all daa from stream
            Stream responseStream = response.GetResponseStream();
            string responseStr = new StreamReader(responseStream).ReadToEnd();
            return responseStr;
        }


        return null;
    }



    public List<filesformobile> getfiles()
    {
        //  now get fiiles
        List<filesformobile> ls = new List<filesformobile>();
        string sign = mf_signature;
        String ss = "";
        // connection on this url to get details of user with the seesion token
        string url = "http://www.mediafire.com/api/1.1/folder/get_content.php?folder_key=&session_token=" + Session["token"].ToString() + "&content_type=files";

        WebClient wc = new WebClient();

        XmlDocument xml = new XmlDocument();
        // response will be in xml
        xml.LoadXml(wc.DownloadString(url));
        // parse the ml here
        XmlNodeList xnList = xml.SelectNodes("/response/folder_content/files/file");

        foreach (XmlNode xn in xnList)
        {
            // get name and file key from xml and add into list
            string firstName = xn["filename"].InnerText;
            string key = xn["quickkey"].InnerText;
            ls.Add(new filesformobile() { url = key, name = firstName });
        }
        // data table is for design view
        return ls;
       
        // adding data into design view
       

    }


}