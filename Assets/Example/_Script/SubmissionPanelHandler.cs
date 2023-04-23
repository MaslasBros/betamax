using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using BetaMax.Core;

///<summary>This class is responsible for raising SubmissionHandler.OnIssuePause and OnSubmitPressed events along with
/// automatically populating the dropdown field with the SubmissionHandler issue categories.</summary>
public class SubmissionPanelHandler : MonoBehaviour
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

    private void Awake()
    {
        gameObject.SetActive(false);

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

    ///<summary>Deactivates the panel upon pressing the back button.</summary>
    void AssignBackButtonMethod(ref Button backButton)
    {
        backButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    ///<summary>Assigns SubmissionHandler.OnSubmitPressed() and SubmissionHandler.SetSubmitInfo to the back button</summary>
    void AssignSubmitButtonMethod(ref Button submitButton)
    {
        submitButton.onClick.AddListener(() => SubmissionHandler.S.SetSubmitInfo(GetSubmitInfo()));
        submitButton.onClick.AddListener(() => SubmissionHandler.S.OnSubmitPressed());
        submitButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    private void Start()
    {
        PopulateIssueCategories(SubmissionHandler.S.IssueCategories);
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

    ///<summary>Returns the needed panel's fields inside a data container.</summary>
    SubmitInfo GetSubmitInfo()
    {
        SubmitInfo info = new SubmitInfo();

        info.selectedIssueCategory = issueCategory.value;
        info.issueDescription = issueDescr.text;
        info.issueReproduction = stepsToReproduce.text;

        return info;
    }
}