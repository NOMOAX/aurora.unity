namespace Aurora.Unity.Threading
{
    /// <summary>
    /// 计时器回调方法。
    /// </summary>
    /// <param name="timer">计时器。</param>
    /// <param name="state">用户传入的状态对象；或 <see langword="null"/>。</param>
    public delegate void TimerCallback(ITimer timer, object state);
}
