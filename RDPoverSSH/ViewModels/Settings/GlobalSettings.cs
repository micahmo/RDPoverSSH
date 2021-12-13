namespace RDPoverSSH.ViewModels.Settings
{
    /// <summary>
    /// A set of singleton settings that can be accessed from anywhere in the application
    /// </summary>
    public static class GlobalSettings
    {
        public static DarkModeSetting DarkModeSetting => _darkModeSetting ??= new DarkModeSetting();
        private static DarkModeSetting _darkModeSetting;
    }
}
