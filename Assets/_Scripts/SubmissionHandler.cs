using UnityEngine;

public class SubmissionHandler : MonoBehaviour
{
    [Header("Submitter settings")]
    [SerializeField] KeyCode submitPanelKey;
    [SerializeField] KeyCode configPanelKey;
    [SerializeField] string[] issueCategories;
    [SerializeField] string hostname;
    [SerializeField] string serverUsername;
    [SerializeField] string serverPassword;

    [Header("Set in inspector")]
    [SerializeField] SubmissionPanelHandler submissionPanel;
    [SerializeField] ConfigPanelHandler configPanel;

    private void Start()
    {
        FeedIssueCategories();
    }

    private void Update()
    {
        if (Input.GetKeyDown(submitPanelKey))
        {
            submissionPanel.TogglePanel();
            if (configPanel.IsActive) configPanel.TogglePanel();
        }

        if (Input.GetKeyDown(configPanelKey))
        {
            configPanel.TogglePanel(true);
            if (submissionPanel.IsActive) submissionPanel.TogglePanel();
        }
    }

    void FeedIssueCategories()
    {
        string[] clone = (string[])issueCategories.Clone();

        submissionPanel.PopulateIssueCategories(clone);
    }
}
