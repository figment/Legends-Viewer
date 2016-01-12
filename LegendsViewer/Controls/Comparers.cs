using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsViewer.Controls
{
    class LambaComparer<T> : IComparer<T>
    {
        readonly Func<T, T, int> _comparerFunc;

        public LambaComparer(Func<T,T,int> comparerFunc )
        {
            _comparerFunc = comparerFunc;
        }

        public int Compare(T x, T y)
        {
            return _comparerFunc(x, y);
        }
    }
}
