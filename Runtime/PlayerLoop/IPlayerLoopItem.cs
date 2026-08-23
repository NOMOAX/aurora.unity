namespace Aurora.Unity.PlayerLoop
{
    /// <summary>
    /// Defines a method that runs within the player loop.
    /// </summary>
    public interface IPlayerLoopItem
    {
        /// <summary>
        /// Runs.
        /// </summary>
        /// <param name="playerLoopPhase">The player loop phase.</param>
        void Run(PlayerLoopPhase playerLoopPhase);
    }
}
