using BetaMax.Core;
using UnityEngine;

namespace BetaMax.Posts
{
    public class PostHandler
    {
        string url;
        string fileLocation;

        string username;
        string password;

        ITransferHandler activeHandler;

        public PostHandler(string url, string fileLocation, string username, string password)
        {
            this.url = url;
            this.fileLocation = fileLocation;

            this.username = username;
            this.password = password;
        }

        public void BeginFileTransfer()
        {
            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                Debug.Log("Is HTTP(S)");

                HTTP_Handler hTTP_Handler = SubmissionHandler.S.gameObject.AddComponent<HTTP_Handler>();
                hTTP_Handler.SetServerInfo(url, fileLocation, username, password);
                hTTP_Handler.UploadFile();
            }
            else if (url.StartsWith("ftp://"))
            {
                Debug.Log("Is FTP");
                //Handle it with FTP
            }
            else if (url.StartsWith("sftp://"))
            {
                Debug.Log("Is FTPS");
                //Handle it with SFTP
            }

            activeHandler?.HandleFileTransfer();
        }
    }
}