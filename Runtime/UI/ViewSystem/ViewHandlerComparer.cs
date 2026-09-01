using System.Collections.Generic;

namespace Aurora.Unity.UI.ViewSystem
{
    internal readonly struct ViewHandlerComparer : IComparer<ViewHandler>
    {
        public int Compare(ViewHandler x, ViewHandler y)
        {
            if (x is null)
            {
                return y is null ? 0 : -1;
            }
            if (y is null)
            {
                return 1;
            }
            var t1 = x.HandledViewType;
            var t2 = y.HandledViewType;
            return t1 == t2 ? 0 : new BaseTypeCountComparer().Compare(t1, t2);
        }
    }
}
