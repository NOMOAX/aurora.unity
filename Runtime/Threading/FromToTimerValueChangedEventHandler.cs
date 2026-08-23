namespace Aurora.Unity.Threading
{
    /// <summary>
    /// Wraps the method executed after the value of <see cref="IFromToTimer"/> changes.
    /// </summary>
    public delegate void FromToTimerValueChangedEventHandler(
        IFromToTimer                        fromToTimer,
        in FromToTimerValueChangedEventArgs args);
}
