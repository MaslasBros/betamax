namespace BetaMax.UI
{
    using System.IO;

    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    using BetaMax.Core;
    using BetaMax.Core.IO;
    using System.Text.RegularExpressions;
    using System;

    public class ConfigPanelHandler : MonoBehaviour, IPanelHandler
    {
        [Header("Configuration panel fields")]
        [SerializeField] Toggle onIssuePauseToggle;
        [SerializeField] Toggle onSubmitDownloadToggle;

        [Space(), Header("Paths")]
        [SerializeField] TMP_InputField downloadsPathField;
        [SerializeField] TMP_InputField optionalsPathField;

        [Space(), Header("Fields")]
        [SerializeField] TMP_InputField betaTester;
        [SerializeField] TMP_InputField osField;
        [SerializeField] TMP_InputField cpuField;
        [SerializeField] TMP_InputField ramField;
        [SerializeField] TMP_InputField svgaField;

        [Header("Buttons")]
        [SerializeField] Button backButton;
        [SerializeField] Button saveButton;

        string testerName, os, cpu, ram, gpu;
        string downloadPath, optionalsPath;

        string infoSavePath = Application.dataPath;
        string infoSavePathFinal = string.Empty;

        Color cCache;
        Color testerColorCache;

        bool isActive = true;
        public bool IsActive { get => isActive; }

        public string OptionalFilesPath => optionalsPathField.text;
        public string DownloadsPathField => downloadsPathField.text;

        private void Awake()
        {
            infoSavePathFinal = Path.Combine(infoSavePath, SubmissionHandler.S.TEMP_FOLDER_NAME, SubmissionHandler.S.JSON_FILE_NAME);

            AssignBackButtonMethod(ref backButton);
            AssignSaveButtonMethod(ref saveButton);
        }

        ///<summary>Adds TogglePanel() as an onClick method to the back button.</summary>
        void AssignBackButtonMethod(ref Button backButton)
        {
            backButton.onClick.AddListener(() => TogglePanel());
        }

        ///<summary>Adds SerializeFieldsToJSON(...) as an onClick method to the save button.</summary>
        void AssignSaveButtonMethod(ref Button saveButton)
        {
            saveButton.onClick.AddListener(() => SaveButtonCallback());
        }

        private void Start()
        {
            if (optionalsPathField != null)
            { cCache = optionalsPathField.image.color; }

            if (betaTester != null)
            { testerColorCache = betaTester.image.color; }

            PopulateInputFields();

            //For initial data creation
            if (!File.Exists(infoSavePathFinal))
            {
                SerializeFieldsToJSON(infoSavePathFinal);
            }

            TogglePanel(true);
        }

        ///<summary>Toggles the UI panel ON/OFF based on its state.
        /// <para>If refreshFields is passed true, the UI Panel fields will retrieve the saved JSON data from the users device.</para>
        /// </summary>
        public void TogglePanel(bool refreshFields = false)
        {
            if (refreshFields)
            {
                TesterInfo info = InfoSerialization.DeserializeJsonToObj(infoSavePathFinal);

                if (info != null)
                { PopulateInputFields(info); }
                else PopulateInputFields();
            }

            isActive = !isActive;
            gameObject.SetActive(isActive);
        }

        ///<summary>Populates the UI elements fields with default values.</summary>
        void PopulateInputFields()
        {
            testerName = string.Empty;
            os = SystemInfo.operatingSystem;
            cpu = SystemInfo.processorType;
            ram = SystemInfo.systemMemorySize.ToString() + "_MB";
            gpu = SystemInfo.graphicsDeviceName + "_" + SystemInfo.graphicsMemorySize + "_MB_DVERSION_" + SystemInfo.graphicsDeviceVersion;

            betaTester.text = testerName;
            osField.text = os;
            cpuField.text = cpu;
            ramField.text = ram;
            svgaField.text = gpu;
        }

        ///<summary>Populates the UI elements fields with the info present in the passed TesterInfo object.</summary>
        void PopulateInputFields(TesterInfo info)
        {
            //Retrieve the data
            onIssuePauseToggle.isOn = info.onIssueToggleValue;
            onSubmitDownloadToggle.isOn = info.onsSubmitDownloadValue;

            downloadPath = info.downloadPath;
            optionalsPath = info.optionalsPath;

            testerName = info.testerName;
            os = info.osField;
            cpu = info.cpuField;
            ram = info.ramField;
            gpu = info.svgaField;

            //Set the fields
            downloadsPathField.text = downloadPath;
            optionalsPathField.text = optionalsPath;

            betaTester.text = testerName;
            osField.text = os;
            cpuField.text = cpu;
            ramField.text = ram;
            svgaField.text = gpu;
        }

        ///<summary>Serializes the UI elements info and saves them to the passed string representing a path.</summary>
        void SerializeFieldsToJSON(string infoSavePath)
        {
            TesterInfo tInfo = new TesterInfo();

            //Togglers
            tInfo.onIssueToggleValue = onIssuePauseToggle.isOn;
            tInfo.onsSubmitDownloadValue = onSubmitDownloadToggle.isOn;

            //Paths
            tInfo.downloadPath = downloadsPathField.text;
            tInfo.optionalsPath = optionalsPathField.text;

            //Fields
            tInfo.testerName = betaTester.text;
            tInfo.osField = osField.text;
            tInfo.cpuField = cpuField.text;
            tInfo.ramField = ramField.text;
            tInfo.svgaField = svgaField.text;

            SubmissionHandler.S.PauseOnIssue = tInfo.onIssueToggleValue;

            Debug.Log("Serialization result:" + InfoSerialization.SerializeTesterInfo(tInfo, infoSavePath));
        }

        void SaveButtonCallback()
        {
            if (ValidateTesterName(betaTester.text) & ValidateOptionalsPath(optionalsPathField.text))
            { SerializeFieldsToJSON(infoSavePathFinal); }
        }

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

        ///<summary>Call to validate the optionals path input field text.</summary>
        bool ValidateOptionalsPath(string inputPath)
        {
            if (String.IsNullOrEmpty(inputPath) || String.IsNullOrWhiteSpace(inputPath))
            {
                optionalsPathField.image.color = cCache;
                return true;
            }

            string lastChar = inputPath.ToCharArray()[inputPath.Length - 1].ToString();

            if (Regex.IsMatch(lastChar, @"[^A-Za-z0-9\s]")
                    || !Directory.Exists(inputPath))
            {
                optionalsPathField.image.color = Color.red;
                return false;
            }
            else
            {
                optionalsPathField.image.color = cCache;
                return true;
            }
        }
    }
}