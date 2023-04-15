namespace BetaMax.UI
{
    ///<summary>Interface which houses basic Betamax panel functionalities</summary>
    internal interface IPanelHandler
    {
        ///<summary>The active state of the panel</summary>
        public bool IsActive { get; }

        ///<summary>Toggles panel on or off</summary>
        public void TogglePanel(bool refreshFields);
    }
}