namespace Aurora.Unity.Threading
{
    /// <summary>
    /// 计数器回调方法。
    /// </summary>
    /// <param name="counter">计数器。</param>
    /// <param name="state">用户传入的状态对象；或 <see langword="null"/>。</param>
    public delegate void CounterCallback(ICounter counter, object state);
}
