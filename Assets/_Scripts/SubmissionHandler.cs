using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
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
        }

        if (Input.GetKeyDown(configPanelKey))
        {
            configPanel.TogglePanel();
        }
    }

    void FeedIssueCategories()
    {
        string[] clone = (string[])issueCategories.Clone();

        submissionPanel.PopulateIssueCategories(clone);
    }
}
