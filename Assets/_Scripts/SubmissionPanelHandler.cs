using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SubmissionPanelHandler : MonoBehaviour, IPanelHandler
{
    [SerializeField] TMP_Dropdown issueCategory;
    [SerializeField] TMP_InputField issueDescr;
    [SerializeField] TMP_InputField stepsToReproduce;
    [SerializeField] Button submitButton;

    bool isActive = true;

    public bool IsActive { get => isActive; }

    public void TogglePanel()
    {
        isActive = !isActive;

        gameObject.SetActive(isActive);
    }

    private void Start()
    {
        TogglePanel();
    }

    public void PopulateIssueCategories(string[] categories)
    {
        List<string> options = new List<string>();

        for (int i = 0; i < categories.Length; i++)
        {
            options.Add(categories[i]);
        }

        issueCategory.AddOptions(options);
    }
}
