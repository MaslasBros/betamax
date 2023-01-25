using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using TMPro;

public class ConfigPanelHandler : MonoBehaviour, IPanelHandler
{
    [Header("Configuration panel fields")]
    [SerializeField] Toggle onIssuePauseToggle;
    [SerializeField] TMP_InputField betaTester;
    [SerializeField] TMP_InputField osField;
    [SerializeField] TMP_InputField cpuField;
    [SerializeField] TMP_InputField ramField;
    [SerializeField] TMP_InputField svgaField;

    [Header("Buttons")]
    [SerializeField] Button backButton;
    [SerializeField] Button saveButton;

    string testerName, os, cpu, ram, gpu;
    string infoSavePath = "URI=file:" + Application.dataPath + "/testerInfo.json";

    bool isActive = true;
    public bool IsActive { get => isActive; }

    private void Awake()
    {
        AssignBackButtonMethod(ref backButton);
        AssignSaveButtonMethod(ref saveButton);
    }

    void AssignBackButtonMethod(ref Button backButton)
    {
        backButton.onClick.AddListener(() => TogglePanel());
    }

    void AssignSaveButtonMethod(ref Button saveButton)
    {
        saveButton.onClick.AddListener(() => SerializeFieldsToJSON());
    }

    private void Start()
    {
        PopulateInputFields();
        TogglePanel(true);
    }

    public void TogglePanel(bool refreshFields = false)
    {
        if (refreshFields)
        {
            TesterInfo info = DeserializeJsonToObj();

            if (info != null)
            { PopulateInputFields(info); }
            else PopulateInputFields();
        }

        isActive = !isActive;
        gameObject.SetActive(isActive);
    }

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

    void PopulateInputFields(TesterInfo info)
    {
        onIssuePauseToggle.isOn = info.onIssueToggleValue;
        testerName = info.testerName;
        os = info.osField;
        cpu = info.cpuField;
        ram = info.ramField;
        gpu = info.svgaField;

        betaTester.text = testerName;
        osField.text = os;
        cpuField.text = cpu;
        ramField.text = ram;
        svgaField.text = gpu;
    }

    void SerializeFieldsToJSON()
    {
        TesterInfo tInfo = new TesterInfo();
        tInfo.onIssueToggleValue = onIssuePauseToggle.isOn;
        tInfo.testerName = betaTester.text;
        tInfo.osField = osField.text;
        tInfo.cpuField = cpuField.text;
        tInfo.ramField = ramField.text;
        tInfo.svgaField = svgaField.text;

        string jsonString = JsonUtility.ToJson(tInfo, true);

        string trimmedPath = infoSavePath.Substring(9);
        using (FileStream fs = File.Create(trimmedPath))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(jsonString);
            }
        }
    }

    TesterInfo DeserializeJsonToObj()
    {
        TesterInfo temp = new TesterInfo();

        string jsonString = string.Empty;

        string trimmedPath = infoSavePath.Substring(9);

        if (!File.Exists(trimmedPath)) { return null; }

        using (StreamReader sr = new StreamReader(trimmedPath))
        {
            jsonString = sr.ReadToEnd();
        }

        temp = JsonUtility.FromJson<TesterInfo>(jsonString);

        return temp;
    }
}
