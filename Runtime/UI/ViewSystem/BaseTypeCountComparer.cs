using System;
using System.Collections.Generic;

namespace Aurora.Unity.UI.ViewSystem
{
    internal readonly struct BaseTypeCountComparer : IComparer<Type>
    {
        public int Compare(Type x, Type y)
        {
            var baseTypeCountX = GetBaseTypeCount(x);
            var baseTypeCountY = GetBaseTypeCount(y);
            return baseTypeCountX.CompareTo(baseTypeCountY);
        }

        private static int GetBaseTypeCount(Type type)
        {
            var result = 0;
            while (type is not null)
            {
                ++result;
                type = type.BaseType;
            }
            return result;
        }
    }
}
