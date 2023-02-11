namespace BetaMax.Core
{
    using System;
    using UnityEngine;

    using BetaMax.UI;
    using BetaMax.Core.IO;

    public class SubmissionHandler : MonoBehaviour
    {
        static SubmissionHandler _s;
        public static SubmissionHandler S;

        [Header("Submitter settings")]
        [SerializeField] KeyCode submitPanelKey;
        [SerializeField] KeyCode configPanelKey;
        [SerializeField] string[] issueCategories;
        [SerializeField] string hostname;
        [SerializeField] string serverUsername;
        [SerializeField] string serverPassword;
        [SerializeField] Camera screenshotCamera;

        [Header("Set in inspector")]
        [SerializeField] SubmissionPanelHandler submissionPanel;
        [SerializeField] ConfigPanelHandler configPanel;

        ScreenshotHandler screenshotHandler;

        public string[] IssueCategories => (string[])issueCategories.Clone();

        ///<summary>Subscribe to this event to get notified when the user opens the submission form.</summary>
        public event Action onIssueCommited;
        private void OnIssueCommited()
        {
            if (onIssueCommited != null)
            { onIssueCommited(); }
        }

        private void Awake()
        {
            S = this;
            screenshotHandler = gameObject.AddComponent<ScreenshotHandler>();
        }

        private void Start()
        {
            submissionPanel.onSubmitPressed += CaptureScreenshot;
        }

        ///<summary>Captures a screen shot through the screenshot handler</summary>
        void CaptureScreenshot()
        {
            screenshotHandler.CaptureScreenshot(screenshotCamera);
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
            S = null;
        }
    }
}