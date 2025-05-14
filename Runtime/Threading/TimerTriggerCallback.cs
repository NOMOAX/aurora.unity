namespace Aurora.Unity.Threading
{
    /// <summary>
    /// 表示当计时器触发时执行的方法。
    /// </summary>
    /// <param name="timer">计时器。</param>
    /// <param name="state">用户传入的状态对象；或 <see langword="null"/>。</param>
    public delegate void TimerTriggerCallback(ITimer timer, object state);
}
