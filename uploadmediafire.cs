using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MediaFireSDK;
using MediaFireSDK.Model;

/// <summary>
/// Summary description for uploadmediafire
/// </summary>
public class uploadmediafire
{


    private static string AppId = "";
    private static string AppKey = "";
    public static string Email;
    public static string Password;
    public static string FilePath;

    //
    //  The file to be uploaded
    //
    public uploadmediafire(string email, string password, string path,String appid,String appkey)

    {
        AppId = appid;
        AppKey = appkey;
        Email = email;
        Password = password;
        FilePath = path;
        ServicePointManager.Expect100Continue = false;
        var config = new MediaFireApiConfiguration
           (
               appId: AppId,
               apiKey: AppKey,
               apiVersion: "1.5",
               automaticallyRenewToken: true,
               chunkTransferBufferSize: 500000,
               useHttpV1: true //On some platforms, the client will throw the error "The server committed a protocol violation. Section=ResponseStatusLine". In that cases set this property to true. 

           );

        _agent = new MediaFireAgent(config);
    }


    private static IMediaFireAgent _agent;
    public async Task<string> Main()
    {
        Console.WriteLine("Signing in {0}...", Email);
        await _agent.User.GetSessionToken(Email, Password);

        var fileInfo = GetFile();

        var resumableController = new MfResumableFileUploadController(_agent, fileInfo.Length, fileInfo.OpenRead(), fileInfo.Name);
        resumableController.CalculateFileHash();


        var checkDetails = await _agent.Upload.Check(fileInfo.Name, resumableController.FileLength, hash: resumableController.FileHash, resumable: true);

        if (checkDetails.FileExists)
        {
            Console.WriteLine("File already exists in the server, please choose another one");
            return "fail File already exists in the server";
        }

        Console.WriteLine("Unit Size:\t{0}", checkDetails.ResumableUpload.UnitSize);
        Console.WriteLine("Nr Of Units:\t{0}", checkDetails.ResumableUpload.NumberOfUnits);



string get=        await resumableController.Upload(checkDetails.ResumableUpload);
  
        return get;
    }


    private static FileInfo GetFile()
    {
        var finfo = new FileInfo(FilePath);

        Console.WriteLine("File: {0}\t\n", finfo.Name);
        Console.WriteLine("\t\t(bytes)");

        Console.WriteLine("TotalSize:\t{0}\n", finfo.Length);

        return finfo;
    }

}
