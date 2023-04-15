namespace BetaMax.Core
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using UnityEngine;

    using BetaMax.UI;
    using BetaMax.Core.IO;
    using BetaMax.Posts;

    ///<summary>
    /// This class is the main handler of the serialization and packaging proccess of the tool.
    /// </summary>
    public class SubmissionHandler : MonoBehaviour
    {
        ///<summary>SubmissionHandler singleton</summary>
        static SubmissionHandler _s;
        ///<summary>SubmissionHandler Instance</summary>
        public static SubmissionHandler S;

        ///<summary>The file name of the tester info json</summary>
        [Header("File Naming Settings")]
        [Tooltip("The file name of the tester info json")]
        public string JSON_FILE_NAME = "testerInfo.json";
        ///<summary>The path of the temp folder</summary>
        [Tooltip("The path of the temp folder, leave empty to save at Application.dataPath")]
        public string TEMP_FOLDER_PATH = string.Empty;
        ///<summary>The name of the temp folder</summary>
        [Tooltip("The name of the temp folder")]
        public string TEMP_FOLDER_NAME = "SubTemp";
        ///<summary>The name of the folder where the user's opt files will be copied to.</summary>
        [Tooltip("The name of the folder where the user's opt files will be copied to.")]
        public string USER_OPT_FOLDER = "UserOptFiles";
        ///<summary>The name of the final zip to transfer</summary>
        [Tooltip("The name of the final zip to transfer")]
        public string MAIN_ZIP_NAME = "mainZip.zip";
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

        ///<summary>Whether to show debug log or not.</summary>
        [Header("Debugging Log")]
        [SerializeField, Tooltip("Whether to show debug log or not.")]
        bool showDebug = true;

        #region PRIVATE_VARS
        static bool _showDebug;
        static string _logDumpPath;

        ///<summary>ScreenshotHandler reference</summary>
        ScreenshotHandler screenshotHandler;

        ///<summary>The initial location for the user json file</summary>
        string infoSaveFilePath = string.Empty;

        string logDumpPath = string.Empty;
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
        {
            onIssueCommited?.Invoke();
        }

        ///<summary>Subscribe to this event to get notified when the user opens the submission form.</summary>
        public event Action onIssuePause;
        ///<summary>Raises the onIssuePause event</summary>
        public void OnIssuePause()
        {
            onIssuePause?.Invoke();
        }

        ///<summary>Subscribe to this event to receive the config info from the configuration panel when raised.</summary>
        public event Action<ConfigInfo> onSerializeConfigInfo;
        ///<summary>Raises the onSerializeConfigInfo event</summary>
        public void OnSerializeConfigInfo(ConfigInfo info)
        {
            onSerializeConfigInfo?.Invoke(info);
        }
        #endregion

        ///<summary>An extra proccess which will get executed upon submission.</summary>
        public Func<bool> AuxProccess = null;

        private void Awake()
        {
            S = this;
            _showDebug = showDebug;

            //Default to the Application.dataPath if TEMP_FOLDER_PATH is left empty.
            if (String.IsNullOrEmpty(TEMP_FOLDER_PATH) || String.IsNullOrWhiteSpace(TEMP_FOLDER_PATH))
            { TEMP_FOLDER_PATH = Application.dataPath; }

            if (!Directory.Exists(Path.Combine(TEMP_FOLDER_PATH, TEMP_FOLDER_NAME)))
            { Directory.CreateDirectory(Path.Combine(TEMP_FOLDER_PATH, TEMP_FOLDER_NAME)); }

            HandleDumpLogCreation();

            infoSaveFilePath = Path.Combine(TEMP_FOLDER_PATH, TEMP_FOLDER_NAME, JSON_FILE_NAME);

            screenshotHandler = gameObject.AddComponent<ScreenshotHandler>();
        }

        ///<summary>Creates the log dump file used for logging.</summary>
        void HandleDumpLogCreation()
        {
            logDumpPath = Path.Combine(TEMP_FOLDER_PATH, TEMP_FOLDER_NAME, logDumpFileName);
            _logDumpPath = logDumpPath;

            if (!File.Exists(logDumpPath))
            { File.Create(logDumpPath); }
        }

        private void Start()
        {
            submissionPanel.onSubmitPressed += CaptureScreenshot;
            submissionPanel.onSubmitPressed += PackagingSequence;
            onSerializeConfigInfo += SerializeFieldsToJSON;

            //For initial tester info file creation
            if (!File.Exists(infoSaveFilePath))
            {
                ConfigInfo temp = configPanel.GetConfigInfo();
                SerializeFieldsToJSON(temp);
            }
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
        void PackagingSequence()
        {
            string userJsonPath = Path.Combine(TEMP_FOLDER_PATH, TEMP_FOLDER_NAME, JSON_FILE_NAME);
            string finalZipPath = Path.Combine(Application.dataPath, MAIN_ZIP_NAME);

            string optFilesPath;
            string downFilesPath;
            bool canDownload;

            TesterInfo tempInfo = InfoSerialization.DeserializeJsonToObj(userJsonPath);
            if (tempInfo != null)
            {
                optFilesPath = tempInfo.optionalsPath;
                downFilesPath = tempInfo.downloadPath;
                canDownload = tempInfo.onsSubmitDownloadValue;
            }
            else { throw new NullReferenceException("No JSON file deserialized!"); }

            Task.Run(async () =>
            {
                //Task blocking 
                if (AuxProccess())
                { Log("Aux process completed"); }

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

            }).ContinueWith((t) => //File uploading
            {
                Log("Starting zip uploading");
                PostHandler postHandler = new PostHandler(hostname, finalZipPath, serverUsername, serverPassword);
                postHandler.BeginFileTransfer();

            }).ContinueWith((t) => //File downloading
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
            });
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
        #endregion

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

            if (!_showDebug) { return; }

            Debug.Log(msg);
        }
    }
}