using System;

namespace Aurora.Unity.Threading
{
    /// <summary>
    /// Arguments used by <see cref="FromToTimerValueChangedEventHandler"/>.
    /// </summary>
    public readonly struct FromToTimerValueChangedEventArgs : IFormattable
    {
        /// <summary>
        /// The cause of the value change of <see cref="IFromToTimer"/>.
        /// </summary>
        public readonly FromToTimerValueChangingCausation Causation;

        /// <summary>
        /// The previous value.
        /// </summary>
        public readonly double PreviousValue;

        /// <summary>
        /// The new value.
        /// </summary>
        public readonly double NewValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="FromToTimerValueChangedEventArgs"/> struct.
        /// </summary>
        /// <param name="causation">The cause of the value change of <see cref="IFromToTimer"/>.</param>
        /// <param name="previousValue">The previous value.</param>
        /// <param name="newValue">The new value.</param>
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
