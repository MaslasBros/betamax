using BetaMax.Core;

namespace BetaMax.Posts
{
    public class PostHandler
    {
        ///<summary>The server URL</summary>
        string url;
        ///<summary>The local file location</summary>
        string fileLocation;

        ///<summary>The server username</summary>
        string username;
        ///<summary>The server password.</summary>
        string password;

        public PostHandler(string url, string fileLocation, string username, string password)
        {
            this.url = url;
            this.fileLocation = fileLocation;

            this.username = username;
            this.password = password;
        }

        ///<summary>Detects what protocol the URL is using and uploads it with the specified protocol handler.</summary>
        public bool BeginFileTransfer()
        {
            bool result = false;

            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                SubmissionHandler.Log("Is HTTP(S)");

                HTTP_Handler hTTP_Handler = SubmissionHandler.S.gameObject.AddComponent<HTTP_Handler>();
                hTTP_Handler.SetServerInfo(url, fileLocation, username, password);
                hTTP_Handler.UploadFile((res) => { result = res; });
            }
            else if (url.StartsWith("ftp://"))
            {
                SubmissionHandler.Log("Is FTP");

                FTP_Handler fTP_Handler = new FTP_Handler(url, fileLocation, username, password);
                fTP_Handler.UploadFile(out result);
            }
            else if (url.StartsWith("sftp://"))
            {
                SubmissionHandler.Log("Is SFTP");

                SFTP_Handler handler = new SFTP_Handler(url, fileLocation, username, password);
                handler.UploadFile(out result);
            }

            return result;
        }
    }
}