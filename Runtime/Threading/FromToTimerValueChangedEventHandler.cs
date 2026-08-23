namespace Aurora.Unity.Threading
{
    /// <summary>
    /// 封装在 <see cref="IFromToTimer"/> 数值改变之后执行的方法。
    /// </summary>
    public delegate void FromToTimerValueChangedEventHandler(
        IFromToTimer                        fromToTimer,
        in FromToTimerValueChangedEventArgs args);
}
