using System.Collections;

namespace BetaMax.Posts
{
    public interface ITransferHandler
    {
        ///<summary>Begins the file transfer sequence in a new thread.</summary>
        public void HandleFileTransfer();
    }
}