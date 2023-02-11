namespace BetaMax.Core.IO
{
    using System.IO;
    using Newtonsoft.Json;

    public static class InfoSerialization
    {
        ///<summary>Serializes the passed packet as a .json file at the infoSavePat.
        /// <para>Returns true if the file was serialized correctly, false otherwise.
        /// </summary>
        public static bool SerializeTesterInfo(TesterInfo packet, string infoSavePath)
        {
            string jsonString = JsonConvert.SerializeObject(packet, Formatting.Indented);

            //Create the file storing the json string.
            using (FileStream fs = File.Create(infoSavePath))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(jsonString);
                }
            }

            if (File.Exists(infoSavePath))
            {
                return File.ReadAllText(infoSavePath).Equals(jsonString);
            }

            return false;
        }

        ///<summary>Returns the deserialized TesterInfo from the passed path, if there is one.
        /// <para>Will return null in case there is not file at the specified path</para>
        /// </summary>
        public static TesterInfo DeserializeJsonToObj(string infoSavePath)
        {
            TesterInfo temp = new TesterInfo();

            if (!File.Exists(infoSavePath)) { return null; }

            string jsonString = string.Empty;
            using (StreamReader sr = new StreamReader(infoSavePath))
            {
                jsonString = sr.ReadToEnd();
            }

            temp = JsonConvert.DeserializeObject<TesterInfo>(jsonString);

            return temp;
        }
    }
}