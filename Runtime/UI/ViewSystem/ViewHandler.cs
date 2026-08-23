using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Collections;
using Aurora.Pooling;
using Aurora.Sorting;
using Object = UnityEngine.Object;

namespace Aurora.Unity.UI.ViewSystem
{
    /// <summary>
    /// 界面处理程序。
    /// </summary>
    public abstract class ViewHandler
    {
        private static readonly HashSet<ViewHandler> ViewHandlers = new();

        private static readonly IComparer<ViewHandler> ReversedViewHandlerComparer =
            new ReversedComparer<ViewHandler>(new ViewHandlerComparer());

        /// <summary>
        /// 注册界面处理程序。
        /// </summary>
        /// <param name="viewHandler">界面处理程序。</param>
        /// <exception cref="ArgumentNullException"><paramref name="viewHandler"/> 为 <see langword="null"/>。</exception>
        public static void Register(ViewHandler viewHandler)
        {
            if (viewHandler is null)
            {
                throw new ArgumentNullException(nameof(viewHandler));
            }
            var handleableLeastDerivedViewType = viewHandler.HandleableLeastDerivedViewType;
            if (handleableLeastDerivedViewType == null)
            {
                throw new ArgumentException($"{nameof(viewHandler)}.{nameof(HandleableLeastDerivedViewType)} 为 null");
            }
            if (handleableLeastDerivedViewType != typeof(View) &&
                !handleableLeastDerivedViewType.IsSubclassOf(typeof(View)))
            {
                throw new ArgumentException($"{nameof(viewHandler)}.{nameof(HandleableLeastDerivedViewType)} 不是界面类型");
            }
            ViewHandlers.Add(viewHandler);
        }

        /// <summary>
        /// 获取最适合于处理指定类型界面的界面处理程序。
        /// </summary>
        /// <typeparam name="T">界面的类型。</typeparam>
        /// <returns>最适合于处理指定类型界面的界面处理程序。</returns>
        public static ViewHandler Get<T>() where T : View
        {
            return InternalGet(typeof(T));
        }

        /// <summary>
        /// 获取最适合于处理指定类型界面的界面处理程序。
        /// </summary>
        /// <param name="viewType">界面的类型。</param>
        /// <returns>最适合于处理指定类型界面的界面处理程序。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewType"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="viewType"/> 不是界面类型。</exception>
        public static ViewHandler Get(Type viewType)
        {
            if (viewType == null)
            {
                throw new ArgumentNullException(nameof(viewType));
            }
            if (viewType != typeof(View) && !viewType.IsSubclassOf(typeof(View)))
            {
                throw new ArgumentException($"{nameof(viewType)} 不是界面类型", nameof(viewType));
            }
            return InternalGet(viewType);
        }

        private static ViewHandler InternalGet(Type viewType)
        {
            var list = PredefinedPools<ViewHandler>.List.Get();
            try
            {
                foreach (var viewHandler in ViewHandlers)
                {
                    var handleableLeastDerivedViewType = viewHandler.HandleableLeastDerivedViewType;
                    if (viewType == handleableLeastDerivedViewType ||
                        viewType.IsSubclassOf(handleableLeastDerivedViewType))
                    {
                        list.Add(viewHandler);
                    }
                }
                TimSort.Sort(list, ReversedViewHandlerComparer);
                return list.Count > 0 ? list[0] : null;
            }
            finally
            {
                PredefinedPools<ViewHandler>.List.Return(list);
            }
        }

        /// <summary>
        /// 获取此 <see cref="ViewHandler"/> 可处理的最小派生界面类型。
        /// </summary>
        public abstract Type HandleableLeastDerivedViewType { get; }

        /// <summary>
        /// 创建“所关联的游戏物体处于未激活状态，或者其本身禁用”的界面。
        /// </summary>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <typeparam name="T">要创建的界面的类型。</typeparam>
        /// <returns>异步操作的任务对象。</returns>
        /// <exception cref="ArgumentException"><typeparamref name="T"/> 是抽象类型，或 <typeparamref name="T"/> 与 <see cref="HandleableLeastDerivedViewType"/> 冲突。</exception>
        /// <remarks>要求界面满足“所关联的游戏物体处于未激活状态，或者其本身禁用”的原因是确保可以在界面脚本的 <c>OnEnable</c> 中编写初始化代码。</remarks>
        public abstract Task<T> CreateInactiveOrDisabledViewAsync<T>(CancellationToken cancellationToken = default)
            where T : View;

        /// <summary>
        /// 释放界面。
        /// </summary>
        /// <param name="view">界面。</param>
        /// <remarks>此方法仅用于界面系统调用，请勿自行调用。</remarks>
        public virtual void ReleaseView(View view)
        {
            Object.Destroy(view.gameObject);
        }
    }
}
