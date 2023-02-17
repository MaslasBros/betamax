using BetaMax.Core;
using UnityEngine;

public class EventTester : MonoBehaviour
{
    private void Start()
    {
        SubmissionHandler.S.onIssueCommited += OnIssueCommit;
        SubmissionHandler.S.onIssuePause += OnIssuePause;
        SubmissionHandler.S.AuxProccess = AuxTest;
    }

    void OnIssueCommit()
    {
        Debug.Log("Issue commited called");
    }

    void OnIssuePause()
    {
        Debug.Log("Issue commited called");
    }

    bool AuxTest()
    {
        Debug.Log("Aux called");
        return true;
    }

    private void OnDestroy()
    {
        SubmissionHandler.S.onIssueCommited -= OnIssueCommit;
        SubmissionHandler.S.onIssuePause -= OnIssuePause;
        SubmissionHandler.S.AuxProccess = null;
    }
}
