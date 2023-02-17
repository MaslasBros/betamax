namespace BetaMax.Core
{
    using System;
    using UnityEngine;

    using BetaMax.UI;
    using BetaMax.Core.IO;
    using System.IO;

    public class SubmissionHandler : MonoBehaviour
    {
        static SubmissionHandler _s;
        public static SubmissionHandler S;

        public string TEMP_FOLDER_NAME = "SubTemp";
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

        ///<summary>Subscribe to this event to get notified when the user opens the submission form.</summary>
        public event Action onIssueCommited;
        void OnIssueCommited()
        {
            if (onIssueCommited != null)
            { onIssueCommited(); }
        }

        ///<summary>Subscribe to this event to get notified when the user opens the submission form.</summary>
        public event Action onIssuePause;
        void OnIssuePause()
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

        ///<summary>Captures a screen shot through the screenshot handler</summary>
        void CaptureScreenshot()
        {
            screenshotHandler.CaptureScreenshot(screenshotCamera);
        }

        #region SUBMISSION_SEQUENCE
        void PackagingSequence()
        {
            if (ManageAuxProccess())
            { Debug.Log("Aux process completed"); }

            //collect optional data
            string optFilesPath = configPanel.OptionalFilesPath;
            if (!String.IsNullOrEmpty(optFilesPath) | !String.IsNullOrWhiteSpace(optFilesPath))
            {
                string finalOptZip = Path.Combine(Application.dataPath, OPT_ZIP_NAME);
                if (File.Exists(finalOptZip))
                { File.Delete(finalOptZip); }

                CompressionHandler.CompressToZip(optFilesPath, finalOptZip);
            }

            //TODO: Package the whole tempfolder
        }

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