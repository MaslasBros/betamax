namespace BetaMax.Core
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using UnityEngine;

    using BetaMax.UI;
    using BetaMax.Core.IO;
    using BetaMax.Posts;

    public class SubmissionHandler : MonoBehaviour
    {
        static SubmissionHandler _s;
        public static SubmissionHandler S;

        public string JSON_FILE_NAME = "testerInfo.json";
        public string TEMP_FOLDER_NAME = "SubTemp";
        public string USER_OPT_FOLDER = "UserOptFiles";
        public string MAIN_ZIP_NAME = "mainZip.zip";
        public string OPT_ZIP_NAME = "optZip.zip";

        [Header("Submitter settings")]
        [SerializeField] KeyCode submitPanelKey;
        [SerializeField] KeyCode configPanelKey;
        [SerializeField] string[] issueCategories;
        [Header("Server Info")]
        [SerializeField] string hostname;
        [SerializeField] string serverUsername;
        [SerializeField] string serverPassword;
        [Header("Screenshot Camera")]
        [SerializeField] Camera screenshotCamera;

        [Header("UI Panels")]
        [SerializeField] SubmissionPanelHandler submissionPanel;
        [SerializeField] ConfigPanelHandler configPanel;

        ScreenshotHandler screenshotHandler;

        public string[] IssueCategories => (string[])issueCategories.Clone();
        public bool PauseOnIssue { get; set; }

        ///<summary>Subscribe to this event to get notified when the user opens the submission form.</summary>
        public event Action onIssueCommited;
        void OnIssueCommited()
        {
            if (onIssueCommited != null)
            { onIssueCommited(); }
        }

        ///<summary>Subscribe to this event to get notified when the user opens the submission form.</summary>
        public event Action onIssuePause;
        public void OnIssuePause()
        {
            if (onIssuePause != null)
            {
                onIssuePause();
            }
        }

        public Func<bool> AuxProccess = null;

        private void Awake()
        {
            S = this;

            screenshotHandler = gameObject.AddComponent<ScreenshotHandler>();
        }

        private void Start()
        {
            submissionPanel.onSubmitPressed += CaptureScreenshot;
            submissionPanel.onSubmitPressed += PackagingSequence;
        }

        ///<summary>Captures a screenshot through the screenshot handler</summary>
        void CaptureScreenshot()
        {
            screenshotHandler.CaptureScreenshot(screenshotCamera);
        }

        #region SUBMISSION_SEQUENCE
        void PackagingSequence()
        {
            string userJsonPath = Path.Combine(Application.dataPath, TEMP_FOLDER_NAME, JSON_FILE_NAME);
            string finalZipPath = Path.Combine(Application.dataPath, "finalZip.zip");

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
                if (ManageAuxProccess())
                { Debug.Log("Aux process completed"); }

                if (!String.IsNullOrEmpty(optFilesPath) && !String.IsNullOrWhiteSpace(optFilesPath))
                {
                    string userTempFolder = Path.Combine(Application.dataPath, TEMP_FOLDER_NAME, USER_OPT_FOLDER);
                    string finalOptZip = Path.Combine(Application.dataPath, TEMP_FOLDER_NAME, USER_OPT_FOLDER, OPT_ZIP_NAME);

                    if (!Directory.Exists(userTempFolder))
                    { Directory.CreateDirectory(userTempFolder); }
                    if (File.Exists(finalOptZip))
                    { File.Delete(finalOptZip); }

                    Debug.Log("Started packaging");

                    await Task.Run(() => CompressionHandler.CompressToZip(optFilesPath, finalOptZip));

                    Debug.Log("Finished file generation");
                }

                string tempFolder = Path.Combine(Application.dataPath, TEMP_FOLDER_NAME);
                string txtFilePath = Path.Combine(tempFolder, "issue.txt");

                if (File.Exists(txtFilePath))
                { File.Delete(txtFilePath); }

                using (FileStream fs = File.Create(txtFilePath))
                {
                    using (TextWriter tw = new StreamWriter(fs))
                    {
                        await tw.WriteLineAsync("Issue Category:\n" + issueCategories[submissionPanel.SelectedIssueCategory]);
                        await tw.WriteLineAsync("\nIssue Description:\n" + submissionPanel.IssueDescription);
                        await tw.WriteLineAsync("\nIssue Repsoduction:\n" + submissionPanel.IssueReproduction);
                    }
                }

                Debug.Log("Starting final zip generation");

                //Create the final zip containing all files.
                await Task.Run(() => CompressionHandler.CompressToZip(Path.Combine(Application.dataPath, TEMP_FOLDER_NAME), finalZipPath));

            }).ContinueWith((t) =>
            {
                Debug.Log("Starting zip uploading");
                PostHandler postHandler = new PostHandler(hostname, finalZipPath, serverUsername, serverPassword);
                postHandler.BeginFileTransfer();

            }).ContinueWith((t) =>
                {
                    if (canDownload && !String.IsNullOrEmpty(downFilesPath) && !String.IsNullOrWhiteSpace(downFilesPath))
                    {
                        string userTempZipPath = Path.Combine(Application.dataPath, TEMP_FOLDER_NAME, USER_OPT_FOLDER, OPT_ZIP_NAME);
                        string copyToPath = Path.Combine(downFilesPath, OPT_ZIP_NAME);

                        if (Directory.Exists(downFilesPath))
                        {
                            Debug.Log("downloads exists");

                            if (File.Exists(copyToPath))
                            { File.Delete(copyToPath); }

                            File.Copy(userTempZipPath, copyToPath);
                        }
                        else
                        {
                            Directory.CreateDirectory(downFilesPath);

                            Debug.Log("downloads did not exist./nCreating folder.");

                            if (File.Exists(copyToPath))
                            { File.Delete(copyToPath); }

                            File.Copy(userTempZipPath, copyToPath);
                        }
                    }
                });
        }

        ///<summary>Dummy method that represents an auxiliary method.</summary>
        bool ManageAuxProccess()
        {
            if (AuxProccess != null)
            { return AuxProccess(); }

            return false;
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
            S = null;
        }
    }
}