using System;
using System.Collections.Generic;
using Aurora.Diagnostics;
using Aurora.Pooling;
using UnityEngine;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// 按钮组。
    /// </summary>
    public sealed class EnhancedButtonGroup : MonoBehaviour
    {
        private static readonly IPool<HashSet<EnhancedButton>> EnhancedButtonHashSetPool =
            new Pool<HashSet<EnhancedButton>>(new PooledHashSetPolicy<EnhancedButton>(), 4);

        private readonly HashSet<EnhancedButton> _buttons =
            new HashSet<EnhancedButton>(UnityEngineObjectEqualityComparer.Instance);

        /// <summary>
        /// 获取一个值，这个值指示是否有至少一个按钮处于开启状态。
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
        /// 获取所有按钮，并将它们放入指定的列表。
        /// </summary>
        /// <param name="result">用于存放结果的列表。</param>
        /// <exception cref="ArgumentNullException"><paramref name="result"/> 为 <see langword="null"/>。</exception>
        public void GetButtons(List<EnhancedButton> result)
        {
            if (result is null)
            {
                throw new ArgumentNullException();
            }
            result.AddRange(_buttons);
        }

        /// <summary>
        /// 关闭所有按钮。
        /// </summary>
        /// <param name="except">除了这个按钮之外。</param>
        /// <remarks>请注意，只有当按钮 <see cref="Behaviour.isActiveAndEnabled"/> 时，按钮才可能会在按钮组中存在。</remarks>
        public void SetAllButtonsOff(EnhancedButton except = null)
        {
            InternalSetAllButtonsOff(except, true);
        }

        /// <summary>
        /// 关闭所有按钮。此操作不会触发回调。
        /// </summary>
        /// <param name="except">除了这个按钮之外。</param>
        /// <remarks>请注意，只有当按钮 <see cref="Behaviour.isActiveAndEnabled"/> 时，按钮才可能会在按钮组中存在。</remarks>
        public void SetAllButtonsOffWithoutNotify(EnhancedButton except = null)
        {
            InternalSetAllButtonsOff(except, false);
        }

        private void InternalSetAllButtonsOff(EnhancedButton except, bool sendCallback)
        {
            // 借助另外一个哈希集来广播事件，防止在某个按钮的回调中出现了添加和删除操作
            var buttons = EnhancedButtonHashSetPool.Get();
            try
            {
                buttons.UnionWith(_buttons);
                if (except != null)
                {
                    buttons.Remove(except);
                }
                foreach (var button in buttons)
                {
                    if (button == null)
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
            if (button == null)
            {
                return;
            }
            _buttons.Add(button);
        }

        internal void UnregisterButton(EnhancedButton button)
        {
            if (button == null)
            {
                return;
            }
            _buttons.Remove(button);
        }
    }
}
