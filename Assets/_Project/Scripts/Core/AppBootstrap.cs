namespace DogtorBurguer
{
    /// <summary>
    /// App-startup entry point: ensures the persistent global managers exist. Invoked from
    /// the first scene's setup (the menu) and defensively by GameManager, so this app-init
    /// logic lives outside any UI class (F-71).
    /// </summary>
    public static class AppBootstrap
    {
        public static void EnsureCoreManagers()
        {
            MonoBehaviourUtil.EnsureComponent<SaveDataManager>();
            MonoBehaviourUtil.EnsureComponent<AdManager>();
            MonoBehaviourUtil.EnsureComponent<IapManager>();
            MonoBehaviourUtil.EnsureComponent<MusicManager>();
        }
    }
}
