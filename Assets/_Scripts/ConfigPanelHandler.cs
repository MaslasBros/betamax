using UnityEngine;
using UnityEngine.UI;
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

    bool isActive = true;

    public bool IsActive { get => isActive; }

    public void TogglePanel()
    {
        isActive = !isActive;

        gameObject.SetActive(isActive);
    }

    private void Start()
    {
        PopulateInputFields();
        TogglePanel();
    }

    //TODO: Text resets on game entry.
    void PopulateInputFields()
    {
        osField.textComponent.SetText(SystemInfo.operatingSystem);
        cpuField.textComponent.SetText(SystemInfo.processorType);
        ramField.textComponent.SetText(SystemInfo.systemMemorySize.ToString());
        svgaField.textComponent.SetText(SystemInfo.graphicsDeviceName + SystemInfo.graphicsMemorySize);
    }
}
