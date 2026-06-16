namespace Aurora.Unity.PlayerLoop
{
    /// <summary>
    /// 定义在主循环中运行的方法。
    /// </summary>
    public interface IPlayerLoopItem
    {
        /// <summary>
        /// 运行。
        /// </summary>
        /// <param name="playerLoopPhase">主循环阶段。</param>
        void Run(PlayerLoopPhase playerLoopPhase);
    }
}
