namespace Far_Off_Wanderer
{
    public class Input
    {
        public Keyboard Keyboard { get; } = new Keyboard();
        public TouchKeys TouchKeys { get; } = new TouchKeys();
        public GamePad GamePad { get; } = new GamePad();
        public TouchPanel TouchPanel { get; } = new TouchPanel();

        public void Update()
        {
            Keyboard.Update();
            TouchKeys.Update();
            GamePad.Update();
            TouchPanel.Update();
        }
    }
}
