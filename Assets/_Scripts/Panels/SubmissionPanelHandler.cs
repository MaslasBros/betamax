namespace BetaMax.UI
{
    using System.Collections.Generic;
    using System;

    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    using BetaMax.Core;

    public class SubmissionPanelHandler : MonoBehaviour, IPanelHandler
    {
        [Header("Set in inspector")]
        [SerializeField] TMP_Dropdown issueCategory;
        [SerializeField] TMP_InputField issueDescr;
        [SerializeField] TMP_InputField stepsToReproduce;
        [SerializeField] Button submitButton;
        [SerializeField] Button backButton;

        bool isActive = true;
        public bool IsActive { get => isActive; }

        public int SelectedIssueCategory => issueCategory.value;
        public string IssueDescription => issueDescr.text;
        public string IssueReproduction => stepsToReproduce.text;

        ///<summary>Subscribe to this event to get notified when the submit button gets pressed.</summary>
        public event Action onSubmitPressed;
        private void OnSubmitPressed()
        {
            if (onSubmitPressed != null)
            { onSubmitPressed(); }
        }

        private void Awake()
        {
            AssignBackButtonMethod(ref backButton);
            AssignSubmitButtonMethod(ref submitButton);
        }

        private void OnEnable()
        {
            if (SubmissionHandler.S.PauseOnIssue)
            { SubmissionHandler.S.OnIssuePause(); }
        }

        private void OnDisable()
        {
            if (SubmissionHandler.S.PauseOnIssue)
            { SubmissionHandler.S.OnIssuePause(); }
        }

        ///<summary>Assigns TogglePanel() to the back button</summary>
        void AssignBackButtonMethod(ref Button backButton)
        {
            backButton.onClick.AddListener(() => TogglePanel());
        }

        ///<summary>Raises the OnSubmitPressed event when pressed</summary>
        void AssignSubmitButtonMethod(ref Button submitButton)
        {
            submitButton.onClick.AddListener(() => OnSubmitPressed());
        }

        private void Start()
        {
            PopulateIssueCategories(SubmissionHandler.S.IssueCategories);
            TogglePanel();
        }

        ///<summary>Activates/Deactivates the panel based on its state.</summary>
        public void TogglePanel(bool refreshFields = false)
        {
            isActive = !isActive;
            gameObject.SetActive(isActive);
        }

        void PopulateIssueCategories(string[] categories)
        {
            List<string> options = new List<string>();

            for (int i = 0; i < categories.Length; i++)
            {
                options.Add(categories[i]);
            }

            issueCategory.AddOptions(options);
        }
    }
}