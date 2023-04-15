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

    ///<summary>
    /// A struct used entirely as a data cariage for the config panel to send 
    /// its field info to the submission handler for serialization.
    ///</summary>
    public struct ConfigInfo
    {
        ///<summary>Pause on Issue toggle state</summary>
        public bool onIssuePauseValue;
        ///<summary>Download on submit toggle state</summary>
        public bool onSubmitDownloadValue;

        ///<summary>The path to download the final zip to.</summary>
        public string downloadsPathValue;
        ///<summary>The path to the optional files folder of the user.</summary>
        public string optionalsPathValue;

        ///<summary>The beta tester's name</summary>
        public string betaTesterValue;
        ///<summary>The OS info of the user.</summary>
        public string osFieldValue;
        ///<summary>The CPU info of the user.</summary>
        public string cpuFieldValue;
        ///<summary>The RAM info of the user.</summary>
        public string ramFieldValue;
        ///<summary>The GPU info of the user.</summary>
        public string svgaFieldValue;
    }

    ///<summary>
    /// This class is responsible for validating and transfering the configuration panel fields to the 
    /// Submission Handler class for serialization.
    ///</summary>
    public class ConfigPanelHandler : MonoBehaviour, IPanelHandler
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
        ///<summary>The panel active state</summary>
        bool isActive = true;

        ///<summary>The default colour of the optionals field.</summary>
        Color optFieldColorCache;
        ///<summary>The default colour of the beta tester field.</summary>
        Color testerColorCache;
        #endregion

        #region PUBLIC_VARS
        ///<summary>The panel active state</summary>
        public bool IsActive { get => isActive; }

        ///<summary>The optional folder path value</summary>
        public string OptionalFilesPath => optionalsPathField.text;
        ///<summary>The downloads folder path value</summary>
        public string DownloadsPathField => downloadsPathField.text;
        #endregion

        private void Awake()
        {
            CacheDefaultColors(ref optFieldColorCache, ref testerColorCache);

            AssignCloseButtonMethod(ref closeButton);
            AssignSaveButtonMethod(ref saveButton);

            PopulateInputFields();

            //Updates the fields
            TogglePanel(true);
        }

        ///<summary>Caches the default optionals field and beta tester field colours to the passed arguments</summary>
        void CacheDefaultColors(ref Color optFieldColorCache, ref Color testerColorCache)
        {
            if (optionalsPathField != null)
            { optFieldColorCache = optionalsPathField.image.color; }

            if (betaTester != null)
            { testerColorCache = betaTester.image.color; }
        }

        ///<summary>Adds TogglePanel() as an onClick method to the back button.</summary>
        void AssignCloseButtonMethod(ref Button backButton)
        {
            backButton.onClick.AddListener(() => TogglePanel());
        }

        ///<summary>Adds SerializeFieldsToJSON(...) as an onClick method to the save button.</summary>
        void AssignSaveButtonMethod(ref Button saveButton)
        {
            saveButton.onClick.AddListener(() => SaveButtonCallback());
        }

        ///<summary>Toggles the UI panel ON/OFF based on its state.
        /// <para>If refreshFields is passed true, the UI Panel fields will retrieve the saved JSON data from the users device.</para>
        /// </summary>
        public void TogglePanel(bool refreshFields = false)
        {
            if (refreshFields)
            {
                TesterInfo info = InfoSerialization.DeserializeJsonToObj(SubmissionHandler.S.InfoSavePathFinal);

                if (info != null)
                { PopulateInputFields(info); }
                else
                { PopulateInputFields(); }
            }

            isActive = !isActive;
            gameObject.SetActive(isActive);
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
            if (ValidateTesterName(betaTester.text) & ValidateOptionalsPath(optionalsPathField.text))
            {
                ConfigInfo temp = GetConfigInfo();
                SubmissionHandler.S.OnSerializeConfigInfo(temp);
            }
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

        ///<summary>Call to validate the optionals path input field text.</summary>
        bool ValidateOptionalsPath(string inputPath)
        {
            if (String.IsNullOrEmpty(inputPath) || String.IsNullOrWhiteSpace(inputPath))
            {
                optionalsPathField.image.color = optFieldColorCache;
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
                optionalsPathField.image.color = optFieldColorCache;
                return true;
            }
        }

        ///<summary>Returns a struct containing all the config panel fields information.</summary>
        public ConfigInfo GetConfigInfo()
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
}