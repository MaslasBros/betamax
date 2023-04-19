namespace BetaMax.UI
{
    using System.Collections.Generic;
    using System;

    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    using BetaMax.Core;

    ///<summary>This class is responsible for raising SubmissionHandler.OnIssuePause and OnSubmitPressed events along with
    /// automatically populating the dropdown field with the SubmissionHandler issue categories.</summary>
    public class SubmissionPanelHandler : MonoBehaviour, IPanelHandler
    {
        ///<summary>The UI dropdown for issueCategory</summary>
        [Header("Set in inspector")]
        [SerializeField, Tooltip("The UI dropdown for issueCategory")]
        TMP_Dropdown issueCategory;
        ///<summary>The UI text area for issue description</summary>
        [SerializeField, Tooltip("The UI text area for issue description")]
        TMP_InputField issueDescr;
        ///<summary>The UI text area for issue reproduction steps</summary>
        [SerializeField, Tooltip("The UI text area for issue reproduction steps")]
        TMP_InputField stepsToReproduce;
        ///<summary>The UI button used for submition</summary>
        [SerializeField, Tooltip("The UI button used for submition")]
        Button submitButton;
        ///<summary>The UI button used for closing the panel</summary>
        [SerializeField, Tooltip("The UI button used for closing the panel")]
        Button backButton;

        ///<summary>The active state of the panel</summary>
        bool isActive = true;

        #region PUBLIC_VARS
        ///<summary>The active state of the panel</summary>
        public bool IsActive { get => isActive; }

        ///<summary>The currently selected issue category from the dropdown.</summary>
        public int SelectedIssueCategory => issueCategory.value;
        ///<summary>The issue description text value.</summary>
        public string IssueDescription => issueDescr.text;
        ///<summary>The issue reproduction text value.</summary>
        public string IssueReproduction => stepsToReproduce.text;
        #endregion

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
            if (SubmissionHandler.S != null)
            {
                if (SubmissionHandler.S.PauseOnIssue)
                { SubmissionHandler.S.OnIssuePause(); }
            }
        }

        ///<summary>Assigns TogglePanel() to the back button</summary>
        void AssignBackButtonMethod(ref Button backButton)
        {
            backButton.onClick.AddListener(() => TogglePanel());
        }

        ///<summary>Assigns OnSubmitPressed() and TogglePanel() to the back button</summary>
        void AssignSubmitButtonMethod(ref Button submitButton)
        {
            submitButton.onClick.AddListener(() => OnSubmitPressed());
            submitButton.onClick.AddListener(() => TogglePanel());
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

        ///<summary>Populates the dropdown UI element selection with the string from the passed argument.</summary>
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