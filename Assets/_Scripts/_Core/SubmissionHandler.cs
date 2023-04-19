namespace BetaMax.Core
{
    using System;
    using System.IO;
    using System.Globalization;
    using System.Threading.Tasks;

    using UnityEngine;

    using BetaMax.UI;
    using BetaMax.Core.IO;
    using BetaMax.Posts;
    using TMPro;
    using System.Collections;

    ///<summary>
    /// This class is the main handler of the serialization and packaging proccess of the tool.
    /// </summary>
    public class SubmissionHandler : MonoBehaviour
    {
        ///<summary>SubmissionHandler singleton</summary>
        static SubmissionHandler _s;
        ///<summary>SubmissionHandler Instance</summary>
        public static SubmissionHandler S;

        [Header("File Naming Settings")]
        ///<summary>The application AppData main folder.</summary>
        [SerializeField, Tooltip("The application config main folder.")]
        string sourceFolder = "YOUR_APPLICATION_NAME";
        ///<summary>The name of the JSON file with the user's config.</summary>
        [Tooltip("The name of the JSON file with the user's config.")]
        public string JSON_FILE_NAME = "testerInfo.json";
        ///<summary>The path of the temp folder</summary>
        [Tooltip("The path of the temp folder, leave empty to save at Application.dataPath")]
        [HideInInspector] public string TEMP_FOLDER_PATH = string.Empty;
        ///<summary>The name of the temp folder</summary>
        [Tooltip("The name of the temp folder")]
        public string TEMP_FOLDER_NAME = "SubTemp";
        ///<summary>The name of the folder where the user's opt files will be copied to.</summary>
        [Tooltip("The name of the folder where the user's opt files will be copied to.")]
        public string USER_OPT_FOLDER = "UserOptFiles";
        ///<summary>The name of the final zip to transfer</summary>
        [Tooltip("The name of the final zip to transfer")]
        public string MAIN_ZIP_NAME = "mainZip.zip";
        ///<summary>The format of the uploaded zip.</summary>
        [Tooltip("The format of the uploaded zip.")]
        [SerializeField] string mainZipFormat = "{date}_{name}";
        ///<summary>The date format</summary>
        [Tooltip("The date format")]
        [SerializeField] string dateFormat = "";
        ///<summary>The name of the zip that will hold the optional user files during transfer.</summary>
        [Tooltip("The name of the zip that will hold the optional user files during transfer.")]
        public string DOWNLOADED_ZIP_NAME = "sentZip.zip";

        ///<summary>The button that will open the submit panel</summary>
        [Header("Submitter settings")]
        [SerializeField, Tooltip("The button that will open the submit panel")]
        KeyCode submitPanelKey;
        ///<summary>The button that will open the configuration panel</summary>
        [SerializeField, Tooltip("The button that will open the configuration panel")]
        KeyCode configPanelKey;
        ///<summary>The categories of issues.</summary>
        [SerializeField, Tooltip("The categories of issues.")]
        string[] issueCategories;

        ///<summary>The transfer server url</summary>
        [Header("Server Info")]
        [SerializeField, Tooltip("The transfer server url")]
        string hostname;
        ///<summary>The server login username</summary>
        [SerializeField, Tooltip("The server login username")]
        string serverUsername;
        ///<summary>The server login password</summary>
        [SerializeField, Tooltip("The server login password")]
        string serverPassword;

        ///<summary>The camera that will be used for screenshoting</summary>
        [Header("Screenshot Camera")]
        [SerializeField, Tooltip("The camera that will be used for screenshoting")]
        Camera screenshotCamera;

        ///<summary>Reference to the SubmissionPanelHandler on the submission panel gameObject</summary>
        [Header("UI Panels")]
        [SerializeField, Tooltip("Reference to the SubmissionPanelHandler on the submission panel gameObject")]
        SubmissionPanelHandler submissionPanel;
        ///<summary>Reference to the ConfigPanelHandler on the configuration panel gameObject</summary>
        [SerializeField, Tooltip("Reference to the ConfigPanelHandler on the configuration panel gameObject")]
        ConfigPanelHandler configPanel;
        ///<summary>The UI element used to show tool messages to the interface</summary>
        [SerializeField, Tooltip("The UI element used to show tool messages to the interface")]
        TextMeshProUGUI messageText;
        ///<summary>The time which the message text will get displayed for.</summary>
        [SerializeField, Tooltip("The time which the message text will get displayed for.")]
        float closeAfterSeconds = 5f;

        ///<summary>Whether to show debug log or not.</summary>
        [Header("Debugging Log")]
        [SerializeField, Tooltip("Whether to show debug log or not.")]
        bool showDebug = true;

        #region PRIVATE_VARS
        ///<summary>Whether to show debug messages in the console, used for the static Log method.</summary>
        static bool _showDebug;
        ///<summary>The path of the log dump file, used for the static Log method.</summary>
        static string _logDumpPath;

        ///<summary>The initial path to create the application folder to.</summary>
        string sourcePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        ///<summary>ScreenshotHandler reference</summary>
        ScreenshotHandler screenshotHandler;

        ///<summary>The initial location for the user json file</summary>
        string infoSaveFilePath = string.Empty;

        ///<summary>The path of the log dump file.</summary>
        string logDumpPath = string.Empty;
        ///<summary>The name of the log dump file.</summary>
        string logDumpFileName = "log.btmdump";
        #endregion

        #region PUBLIC_VARS
        ///<summary>The initial location for the temp folder</summary>
        public string InfoSavePath => TEMP_FOLDER_PATH;
        ///<summary>The initial location for the user json file</summary>
        public string InfoSavePathFinal => infoSaveFilePath;
        ///<summary>The categories of issues.</summary>
        public string[] IssueCategories => (string[])issueCategories.Clone();
        ///<summary>Pause on Issue serialized value.</summary>
        public bool PauseOnIssue { get; set; }
        #endregion

        #region EVENTS
        ///<summary>Subscribe to this event to get notified when the user opens the submission form.</summary>
        public event Action onIssueCommited;
        ///<summary>Raises the onIssueCommited event</summary>
        void OnIssueCommited()
        { onIssueCommited?.Invoke(); }

        ///<summary>Subscribe to this event to get notified when the user opens the submission form.</summary>
        public event Action onIssuePause;
        ///<summary>Raises the onIssuePause event</summary>
        public void OnIssuePause()
        { onIssuePause?.Invoke(); }

        ///<summary>Subscribe to this event to receive the config info from the configuration panel when raised.</summary>
        public event Action<ConfigInfo> onSerializeConfigInfo;
        ///<summary>Raises the onSerializeConfigInfo event</summary>
        public void OnSerializeConfigInfo(ConfigInfo info)
        { onSerializeConfigInfo?.Invoke(info); }

        ///<summary>An extra proccess which will get executed upon submission.</summary>
        public event Action onAuxProcessCalled;
        ///<summary>Raises the onAuxProcessCalled event</summary>
        void OnAuxProcessCalled()
        { onAuxProcessCalled?.Invoke(); }
        #endregion

        private void Awake()
        {
            InitialSetup();

            //Create source
            HandleAppDataDirectory(sourcePath, sourceFolder);

            //Create temp directory
            HandleTempDirectory();

            //Create logger file.
            HandleDumpLogCreation(TEMP_FOLDER_PATH, TEMP_FOLDER_NAME, logDumpFileName);

            infoSaveFilePath = Path.Combine(TEMP_FOLDER_PATH, TEMP_FOLDER_NAME, JSON_FILE_NAME);
        }

        ///<summary>Set the class statics and subscribes to needed events.</summary>
        void InitialSetup()
        {
            S = this;
            _showDebug = showDebug;
            screenshotHandler = gameObject.AddComponent<ScreenshotHandler>();

            submissionPanel.onSubmitPressed += CaptureScreenshot;
            submissionPanel.onSubmitPressed += PackagingSequence;
            onSerializeConfigInfo += SerializeFieldsToJSON;
        }

        ///<summary>Call to create the application folder inside the sourcePath path.</summary>
        void HandleAppDataDirectory(string sourcePath, string sourceFolder)
        {
            string path = Path.Combine(sourcePath, sourceFolder);

            if (!Directory.Exists(path))
            { Directory.CreateDirectory(path); }
        }

        ///<summary>
        /// Creates the temp user directory inside the sourceFolder.
        /// <para>Sets TEMP_FOLDER_PATH to the sourceFolder path.</para>
        /// </summary>
        void HandleTempDirectory()
        {
            TEMP_FOLDER_PATH = Path.Combine(sourcePath, sourceFolder);

            if (!Directory.Exists(Path.Combine(TEMP_FOLDER_PATH, TEMP_FOLDER_NAME)))
            { Directory.CreateDirectory(Path.Combine(TEMP_FOLDER_PATH, TEMP_FOLDER_NAME)); }
        }

        ///<summary>Sets the initial logDumpPath and creates the log dump file used for logging.</summary>
        void HandleDumpLogCreation(string rootFolder, string subFolderName, string logDumpFileName)
        {
            logDumpPath = Path.Combine(rootFolder, subFolderName, logDumpFileName);
            _logDumpPath = logDumpPath;

            if (!File.Exists(logDumpPath))
            { File.Create(logDumpPath); }
        }

        private void Start()
        {
            if (ValidateDateFormat(dateFormat))
            { Log($"Date format validated as {dateFormat}"); }

            //For initial tester info file creation
            if (!File.Exists(infoSaveFilePath))
            {
                ConfigInfo temp = configPanel.GetConfigInfo();
                SerializeFieldsToJSON(temp);
            }
        }

        ///<summary>
        /// Returns true if the inspector passed date format does not contain eny path invalid symbols
        /// and can be parsed as a DateTime formatter. False or error otherwise.
        ///</summary>
        bool ValidateDateFormat(string dateFormat)
        {
            bool result = false;

            if (!String.IsNullOrEmpty(dateFormat) && !String.IsNullOrWhiteSpace(dateFormat))
            {
                //Verify that dateFormat is indeed a DateTime formatter.
                if (DateTime.TryParseExact(DateTime.Now.ToString(dateFormat), dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateTime))
                {
                    result = true;
                }
                else
                { throw new FormatException("Passed date format is not a valid format"); }

                if (ContainsInvalidSymbols(dateFormat))
                { throw new ArgumentException("The input string contains file system symbols."); }
            }

            return result;
        }

        ///<summary>Returns true if the passed strings contains any file name invalid symbols, false otherwise.</summary>
        bool ContainsInvalidSymbols(string str)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char invalidCh in invalidChars)
            {
                if (str.Contains(invalidCh))
                { return true; }
            }

            return false;
        }

        ///<summary>Returns zipName correctly formatted based on the zipNameFormat and dateFormat.</summary>
        string FormatZipName(string zipName, string zipNameFormat, string dateFormat)
        {
            return zipNameFormat.Replace("{date}", DateTime.Now.ToString(dateFormat)).Replace("{name}", zipName);
        }

        ///<summary>Serializes the UI elements info and saves them to the passed string representing a path.</summary>
        void SerializeFieldsToJSON(ConfigInfo configInfo)
        {
            TesterInfo tInfo = new TesterInfo();

            //Togglers
            tInfo.onIssueToggleValue = configInfo.onIssuePauseValue;
            tInfo.onsSubmitDownloadValue = configInfo.onSubmitDownloadValue;

            //Paths
            tInfo.downloadPath = configInfo.downloadsPathValue;
            tInfo.optionalsPath = configInfo.optionalsPathValue;

            //Fields
            tInfo.testerName = configInfo.betaTesterValue;
            tInfo.osField = configInfo.osFieldValue;
            tInfo.cpuField = configInfo.cpuFieldValue;
            tInfo.ramField = configInfo.ramFieldValue;
            tInfo.svgaField = configInfo.svgaFieldValue;

            PauseOnIssue = tInfo.onIssueToggleValue;

            Log("Serialization result:" + InfoSerialization.SerializeTesterInfo(tInfo, infoSaveFilePath));
        }

        ///<summary>Captures a screenshot through the screenshot handler</summary>
        void CaptureScreenshot()
        {
            screenshotHandler.CaptureScreenshot(screenshotCamera);
        }

        #region SUBMISSION_SEQUENCE
        async void PackagingSequence()
        {
            //Format the zip name
            string zipNameCache = MAIN_ZIP_NAME;
            MAIN_ZIP_NAME = FormatZipName(MAIN_ZIP_NAME, mainZipFormat, dateFormat);

            string userJsonPath = Path.Combine(TEMP_FOLDER_PATH, TEMP_FOLDER_NAME, JSON_FILE_NAME);
            string finalZipPath = Path.Combine(Application.dataPath, MAIN_ZIP_NAME);

            string optFilesPath;
            string downFilesPath;
            bool canDownload;

            bool uploadResult = false;

            TesterInfo tempInfo = InfoSerialization.DeserializeJsonToObj(userJsonPath);
            if (tempInfo != null)
            {
                optFilesPath = tempInfo.optionalsPath;
                downFilesPath = tempInfo.downloadPath;
                canDownload = tempInfo.onsSubmitDownloadValue;
            }
            else { throw new NullReferenceException("No JSON file deserialized!"); }

            ShowUploadMessage("Uploading...");

            await Task.Run(async () =>
            {
                //Raise the aux event.
                OnAuxProcessCalled();

                if (!String.IsNullOrEmpty(optFilesPath) && !String.IsNullOrWhiteSpace(optFilesPath))
                {
                    string userTempFolder = Path.Combine(TEMP_FOLDER_PATH, TEMP_FOLDER_NAME, USER_OPT_FOLDER);

                    if (!Directory.Exists(userTempFolder))
                    { Directory.CreateDirectory(userTempFolder); }

                    Log("Started copying");

                    await Task.Run(() =>
                    {
                        try
                        {
                            CopyFilesAndFolders(optFilesPath, userTempFolder);
                        }
                        catch (Exception e)
                        {
                            Log(e.ToString());
                        }
                    });

                    Log("Finished file copying");
                }

                string tempFolder = Path.Combine(TEMP_FOLDER_PATH, TEMP_FOLDER_NAME);
                string txtFilePath = Path.Combine(tempFolder, "issue.txt");

                if (File.Exists(txtFilePath))
                { File.Delete(txtFilePath); }

                //Write the issue to the txt file
                using (FileStream fs = File.Create(txtFilePath))
                {
                    using (TextWriter tw = new StreamWriter(fs))
                    {
                        await tw.WriteLineAsync("Issue Category:\n" + issueCategories[submissionPanel.SelectedIssueCategory]);
                        await tw.WriteLineAsync("\nIssue Description:\n" + submissionPanel.IssueDescription);
                        await tw.WriteLineAsync("\nIssue Repsoduction:\n" + submissionPanel.IssueReproduction);
                    }
                }

                Log("Starting final zip generation");

                //Create the final zip containing all files.
                await Task.Run(() => CompressionHandler.CompressToZip(Path.Combine(TEMP_FOLDER_PATH, TEMP_FOLDER_NAME), finalZipPath));

                Log($"Final zip generated as {MAIN_ZIP_NAME}");

            }).ContinueWith((t) => //File uploading
            {
                Log("Starting zip uploading");
                PostHandler postHandler = new PostHandler(hostname, finalZipPath, serverUsername, serverPassword);
                uploadResult = postHandler.BeginFileTransfer();

            }).ContinueWith((t) => //File copying to local
            {
                if (canDownload && !String.IsNullOrEmpty(downFilesPath) && !String.IsNullOrWhiteSpace(downFilesPath))
                {
                    string copyToPath = Path.Combine(downFilesPath, DateTime.Now.ToString("dddd_dd_MMMM_yyyy_HH_mm_ss") + "_" + DOWNLOADED_ZIP_NAME);

                    if (Directory.Exists(downFilesPath))
                    {
                        Log("downloads folder exists");

                        File.Copy(finalZipPath, copyToPath);
                    }
                    else
                    {
                        Directory.CreateDirectory(downFilesPath);

                        Log("downloads did not exist./nCreating folder.");

                        File.Copy(finalZipPath, copyToPath);
                    }
                }

                //Delete the main zip at the end too
                if (File.Exists(finalZipPath))
                { File.Delete(finalZipPath); }

                //Reset the name to its original value so it can get formatted again.
                MAIN_ZIP_NAME = zipNameCache;
            });

            HandleUploadResult(uploadResult);
        }

        void CopyFilesAndFolders(string sourcePath, string destinationPath)
        {
            Directory.CreateDirectory(destinationPath);

            string[] sourceFiles = Directory.GetFileSystemEntries(sourcePath);

            foreach (string file in sourceFiles)
            {
                string sourceFileName = Path.GetFileName(file);

                string destFileName = Path.Combine(destinationPath, sourceFileName);

                if (Directory.Exists(file))
                {
                    CopyFilesAndFolders(file, destFileName);
                }
                else
                {
                    File.Copy(file, destFileName, true);
                }
            }
        }

        ///<summary>Show the appropriate error or success message on tools UI based on the result.</summary>
        void HandleUploadResult(bool result)
        {
            if (result)
            { ShowUploadMessage("File uploaded successfully.", true); }
            else
            { ShowUploadMessage("Error uploading file.", true); }
        }
        #endregion

        ///<summary>Renders the passed message to the tools UI text element.</summary>
        public void ShowUploadMessage(string message, bool clearAfter = false)
        {
            messageText.text = message;

            if (clearAfter)
            {
                StopCoroutine(CloseAfterSeconds());
                StartCoroutine(CloseAfterSeconds());
            }
        }

        IEnumerator CloseAfterSeconds()
        {
            yield return new WaitForSeconds(closeAfterSeconds);
            messageText.text = string.Empty;
        }

        private void Update()
        {
            if (Input.GetKeyDown(submitPanelKey))
            {
                submissionPanel.TogglePanel();
                if (configPanel.IsActive) configPanel.TogglePanel();

                if (submissionPanel.IsActive) { OnIssueCommited(); }
            }

            if (Input.GetKeyDown(configPanelKey))
            {
                configPanel.TogglePanel(true);
                if (submissionPanel.IsActive) submissionPanel.TogglePanel();
            }
        }

        private void OnDestroy()
        {
            submissionPanel.onSubmitPressed -= CaptureScreenshot;
            submissionPanel.onSubmitPressed -= PackagingSequence;
            onSerializeConfigInfo -= SerializeFieldsToJSON;
            S = null;
        }

        ///<summary>
        /// Shows the passed argument to the debug console if _showDebug is true.
        /// <para>Use this to write the msg to the log dump of the tool</para>
        /// </summary>
        public static void Log(string msg)
        {
            using (StreamWriter writer = new StreamWriter(_logDumpPath, true))
            {
                writer.Write(DateTime.Now.ToString("dddd_dd_MMMM_yyyy_HH_mm_ss") + ": " + msg + "\n");
            }

            if (!_showDebug) return;

            Debug.Log(msg);
        }
    }
}