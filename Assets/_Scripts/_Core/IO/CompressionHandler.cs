namespace BetaMax.Core.IO
{
    using System.IO;
    using System.IO.Compression;

    public static class CompressionHandler
    {
        ///<summary>
        /// Compresses the passed startPath to a .zip file and saves it at the passed savePath.
        /// <para>If the file already exists, it gets deleted.</para>
        /// </summary>
        public static void CompressToZip(string startPath, string savePath)
        {
            if (File.Exists(savePath))
            { File.Delete(savePath); }

            ZipFile.CreateFromDirectory(startPath, savePath, CompressionLevel.Optimal, true);
        }
    }
}