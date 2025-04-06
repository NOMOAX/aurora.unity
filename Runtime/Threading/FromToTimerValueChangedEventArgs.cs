using System;

namespace Aurora.Unity.Threading
{
    /// <summary>
    /// 由 <see cref="FromToTimerValueChangedEventHandler"/> 使用的参数。
    /// </summary>
    public readonly struct FromToTimerValueChangedEventArgs : IFormattable
    {
        /// <summary>
        /// 引起 <see cref="IFromToTimer"/> 值变化的原因。
        /// </summary>
        public readonly FromToTimerValueChangingCausation Causation;

        /// <summary>
        /// 旧值。
        /// </summary>
        public readonly double PreviousValue;

        /// <summary>
        /// 新值。
        /// </summary>
        public readonly double NewValue;

        /// <summary>
        /// 初始化 <see cref="FromToTimerValueChangedEventArgs"/> 结构的新实例。
        /// </summary>
        /// <param name="causation">引起 <see cref="IFromToTimer"/> 值变化的原因。</param>
        /// <param name="previousValue">旧值。</param>
        /// <param name="newValue">新值。</param>
        public FromToTimerValueChangedEventArgs(
            FromToTimerValueChangingCausation causation,
            double                            previousValue,
            double                            newValue)
        {
            Causation     = causation;
            PreviousValue = previousValue;
            NewValue      = newValue;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(Causation)} = {Causation}, {PreviousValue} -> {NewValue}";
        }

        /// <inheritdoc />
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return
                $"{nameof(Causation)} = {Causation}, {PreviousValue.ToString(format, formatProvider)} -> {NewValue.ToString(format, formatProvider)}";
        }
    }
}
