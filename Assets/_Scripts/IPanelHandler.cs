using UnityEngine;

public interface IPanelHandler
{
    public bool IsActive { get; }

    public void TogglePanel();
}