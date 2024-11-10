namespace AppDrawers.JavaScriptMessages
{
    public class DirectoryChanged
    {
        public bool Clipping { get; set; } = false;
        public int CursorX { get; set; } = 0;
        public int CursorY { get; set; } = 0;
        public string Directory { get; set; } = null;
    }
}