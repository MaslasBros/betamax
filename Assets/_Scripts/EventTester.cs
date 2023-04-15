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
        SubmissionHandler.Log("Issue commited called");
    }

    void OnIssuePause()
    {
        SubmissionHandler.Log("Issue paused called");
    }

    bool AuxTest()
    {
        SubmissionHandler.Log("Aux called");
        for (int i = 0; i < 10000; i++)
        {
            _ = i;
        }
        return true;
    }

    private void OnDestroy()
    {
        if (SubmissionHandler.S != null)
        {
            SubmissionHandler.S.onIssueCommited -= OnIssueCommit;
            SubmissionHandler.S.onIssuePause -= OnIssuePause;
            SubmissionHandler.S.AuxProccess = null;
        }
    }
}
