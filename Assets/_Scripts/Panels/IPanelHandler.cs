namespace BetaMax.UI
{
    internal interface IPanelHandler
    {
        public bool IsActive { get; }

        public void TogglePanel(bool refreshFields);
    }
}