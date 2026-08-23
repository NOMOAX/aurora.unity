using System;
using System.Collections.Generic;
using Aurora.Diagnostics;
using Aurora.Pooling;
using UnityEngine;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// A button group.
    /// </summary>
    public sealed class EnhancedButtonGroup : MonoBehaviour
    {
        private static readonly IPool<HashSet<EnhancedButton>> EnhancedButtonHashSetPool =
            new Pool<HashSet<EnhancedButton>>(new PooledHashSetPolicy<EnhancedButton>(), 4);

        private readonly HashSet<EnhancedButton> _buttons = new(UnityEngineObjectEqualityComparer.Instance);

        /// <summary>
        /// Gets a value indicating whether at least one button is on.
        /// </summary>
        public bool AnyButtonsOn
        {
            get
            {
                foreach (var button in _buttons)
                {
                    if (button.IsOn)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Gets all buttons and puts them into the specified list.
        /// </summary>
        /// <param name="result">The list used to hold the results.</param>
        /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
        public void GetButtons(List<EnhancedButton> result)
        {
            if (result is null)
            {
                throw new ArgumentNullException();
            }
            result.AddRange(_buttons);
        }

        /// <summary>
        /// Turns off all buttons.
        /// </summary>
        /// <param name="except">Except for this button.</param>
        /// <remarks>Note that a button can exist in the button group only when the button is <see cref="Behaviour.isActiveAndEnabled"/>.</remarks>
        public void SetAllButtonsOff(EnhancedButton except = null)
        {
            InternalSetAllButtonsOff(except, true);
        }

        /// <summary>
        /// Turns off all buttons. This operation does not trigger callbacks.
        /// </summary>
        /// <param name="except">Except for this button.</param>
        /// <remarks>Note that a button can exist in the button group only when the button is <see cref="Behaviour.isActiveAndEnabled"/>.</remarks>
        public void SetAllButtonsOffWithoutNotify(EnhancedButton except = null)
        {
            InternalSetAllButtonsOff(except, false);
        }

        private void InternalSetAllButtonsOff(EnhancedButton except, bool sendCallback)
        {
            // Use another hash set to broadcast events to prevent add and remove operations inside a button's callback
            var buttons = EnhancedButtonHashSetPool.Get();
            try
            {
                buttons.UnionWith(_buttons);
                if (except is not null)
                {
                    buttons.Remove(except);
                }
                foreach (var button in buttons)
                {
                    if (!button)
                    {
                        continue;
                    }
                    try
                    {
                        if (sendCallback)
                        {
                            button.IsOn = false;
                        }
                        else
                        {
                            button.SetIsOnWithoutNotify(false);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.E(e);
                    }
                }
            }
            finally
            {
                EnhancedButtonHashSetPool.Return(buttons);
            }
        }

        internal void RegisterButton(EnhancedButton button)
        {
            if (button)
            {
                _buttons.Add(button);
            }
        }

        internal void UnregisterButton(EnhancedButton button)
        {
            if (button)
            {
                _buttons.Remove(button);
            }
        }
    }
}
