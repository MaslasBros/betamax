namespace BetaMax.UI
{
    using System.IO;

    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    using BetaMax.Core;
    using BetaMax.Core.IO;

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
        string jsonFileName = "testerInfo.json";
        string infoSavePathFinal = string.Empty;

        bool isActive = true;
        public bool IsActive { get => isActive; }

        private void Awake()
        {
            infoSavePathFinal = Path.Combine(infoSavePath, jsonFileName);

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
            saveButton.onClick.AddListener(() => SerializeFieldsToJSON(infoSavePathFinal));
        }

        private void Start()
        {
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

            Debug.Log("Serialization result:" + InfoSerialization.SerializeTesterInfo(tInfo, infoSavePath));
        }
    }
}