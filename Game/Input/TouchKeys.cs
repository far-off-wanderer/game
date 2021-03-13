namespace Far_Off_Wanderer
{
    public class TouchKeys
    {
        bool backButton;
        bool backButtonTriggered;

        public void TriggerBackButton() => backButtonTriggered = true;

        public bool OnBackButton => backButton;

        public void Update()
        {
            if (backButtonTriggered)
            {
                backButton = true;
            }
            else
            {
                backButton = false;
            }
            backButtonTriggered = false;
        }
    }
}
