using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using BetaMax.Core;

///<summary>
/// This class is responsible for validating and transfering the configuration panel fields to the 
/// Submission Handler class for serialization.
///</summary>
public class ConfigPanelHandler : MonoBehaviour
{
    ///<summary>The unity Toggle in UI for Pause on Issue</summary>
    [Header("Configuration panel fields")]
    [SerializeField, Tooltip("The unity Toggle in UI for Pause on Issue")] Toggle onIssuePauseToggle;
    ///<summary>The unity Toggle in UI for Download on submit</summary>
    [SerializeField, Tooltip("The unity Toggle in UI for Download on submit")] Toggle onSubmitDownloadToggle;

    ///<summary>The UI text field for the download path</summary>
    [Space(), Header("Paths")]
    [SerializeField, Tooltip("The UI text field for the download path")] TMP_InputField downloadsPathField;
    ///<summary>The UI text field for the optional folder path</summary>
    [SerializeField, Tooltip("The UI text field for the optional folder path")] TMP_InputField optionalsPathField;

    ///<summary>The UI text field for the beta tester name</summary>
    [Space(), Header("Fields")]
    [SerializeField, Tooltip("The UI text field for the beta tester name")] TMP_InputField betaTester;
    ///<summary>The UI text field for OS information</summary>
    [SerializeField, Tooltip("The UI text field for OS information")] TMP_InputField osField;
    ///<summary>The UI text field for CPU information</summary>
    [SerializeField, Tooltip("The UI text field for CPU information")] TMP_InputField cpuField;
    ///<summary>The UI text field for RAM information</summary>
    [SerializeField, Tooltip("The UI text field for RAM information")] TMP_InputField ramField;
    ///<summary>The UI text field for GPU information</summary>
    [SerializeField, Tooltip("The UI text field for GPU information")] TMP_InputField svgaField;

    ///<summary>The UI Button for the Close functionality</summary>
    [Header("Buttons")]
    [SerializeField, Tooltip("The UI Button for the Close functionality")] Button closeButton;
    ///<summary>The UI Button for the Save/Serialize functionality</summary>
    [SerializeField, Tooltip("The UI Button for the Save/Serialize functionality")] Button saveButton;

    #region PRIVATE_VARS
    ///<summary>The default colour of the optionals field.</summary>
    Color optFieldColorCache;
    ///<summary>The default colour of the beta tester field.</summary>
    Color testerColorCache;
    #endregion

    private void Awake()
    {
        CacheDefaultColors(ref optFieldColorCache, ref testerColorCache);

        AssignCloseButtonMethod(ref closeButton);
        AssignSaveButtonMethod(ref saveButton);

        PopulateInputFields();

        RefreshFields();

        gameObject.SetActive(false);

        //Pass the UI info to the Submission Handler
        SubmissionHandler.S.SetConfigInfo(GetConfigInfo());
    }

    private void OnEnable()
    { RefreshFields(); }

    ///<summary>Caches the default optionals field and beta tester field colours to the passed arguments</summary>
    void CacheDefaultColors(ref Color optFieldColorCache, ref Color testerColorCache)
    {
        if (optionalsPathField != null)
        { optFieldColorCache = optionalsPathField.image.color; }

        if (betaTester != null)
        { testerColorCache = betaTester.image.color; }
    }

    ///<summary>Deactivates the gameObject upon onClick method to the back button.</summary>
    void AssignCloseButtonMethod(ref Button backButton)
    {
        backButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    ///<summary>Adds SaveButtonCallback(...) as an onClick method to the save button.</summary>
    void AssignSaveButtonMethod(ref Button saveButton)
    {
        saveButton.onClick.AddListener(() => SaveButtonCallback());
    }

    ///<summary>Refreshes the UI fields by retrieving the saved JSON data from the users device.</summary>
    void RefreshFields()
    {
        TesterInfo info = SubmissionHandler.S.GetTesterInfo();

        if (info != null)
        { PopulateInputFields(info); }
        else
        { PopulateInputFields(); }
    }

    ///<summary>Populates the UI elements fields with default values.</summary>
    void PopulateInputFields()
    {
        betaTester.text = string.Empty;
        osField.text = SystemInfo.operatingSystem;
        cpuField.text = SystemInfo.processorType;
        ramField.text = SystemInfo.systemMemorySize.ToString() + "_MB";
        svgaField.text = SystemInfo.graphicsDeviceName + "_" + SystemInfo.graphicsMemorySize + "_MB_DVERSION_" + SystemInfo.graphicsDeviceVersion;
    }

    ///<summary>Populates the UI elements fields with the info present in the passed TesterInfo object.</summary>
    void PopulateInputFields(TesterInfo info)
    {
        onIssuePauseToggle.isOn = info.onIssueToggleValue;
        onSubmitDownloadToggle.isOn = info.onsSubmitDownloadValue;

        downloadsPathField.text = info.downloadPath;
        optionalsPathField.text = info.optionalsPath;

        betaTester.text = info.testerName;
        osField.text = info.osField;
        cpuField.text = info.cpuField;
        ramField.text = info.ramField;
        svgaField.text = info.svgaField;
    }

    ///<summary>Creates a ConfigInfo object and sends it through the  SubmissionHandler.OnSerializeConfigInfo event.</summary>
    void SaveButtonCallback()
    {
        if (ValidateTesterName(betaTester.text) & SubmissionHandler.S.ValidateOptionalsPath(optionalsPathField.text))
        {
            optionalsPathField.image.color = optFieldColorCache;

            ConfigInfo configInfo = GetConfigInfo();

            SubmissionHandler.S.OnSerializeConfigInfo(configInfo);
        }
        else
        { optionalsPathField.image.color = Color.red; }
    }

    ///<summary>Returns true if the beta tester name is not null or empty</summary>
    bool ValidateTesterName(string input)
    {
        if (String.IsNullOrEmpty(input) || String.IsNullOrWhiteSpace(input))
        {
            betaTester.image.color = Color.red;
            return false;
        }

        betaTester.image.color = testerColorCache;
        return true;
    }

    ///<summary>Returns the needed panel's fields inside a data container.</summary>
    ConfigInfo GetConfigInfo()
    {
        ConfigInfo configInfo = new ConfigInfo();
        configInfo.onIssuePauseValue = onIssuePauseToggle.isOn;
        configInfo.onSubmitDownloadValue = onSubmitDownloadToggle.isOn;

        configInfo.downloadsPathValue = downloadsPathField.text;
        configInfo.optionalsPathValue = optionalsPathField.text;

        configInfo.betaTesterValue = betaTester.text;
        configInfo.osFieldValue = osField.text;
        configInfo.cpuFieldValue = cpuField.text;
        configInfo.ramFieldValue = ramField.text;
        configInfo.svgaFieldValue = svgaField.text;

        return configInfo;
    }
}