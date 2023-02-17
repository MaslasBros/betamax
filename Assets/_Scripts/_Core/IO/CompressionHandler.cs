namespace BetaMax.Core.IO
{
    using System.IO.Compression;

    public static class CompressionHandler
    {
        public static void CompressToZip(string startPath, string savePath)
        {
            ZipFile.CreateFromDirectory(startPath, savePath);
        }
    }
}