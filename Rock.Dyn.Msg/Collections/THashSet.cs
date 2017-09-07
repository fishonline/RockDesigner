using System;
using System.Collections;
using System.Collections.Generic;

namespace Rock.Dyn.Msg
{
    [Serializable]
    public class THashSet<T> : ICollection<T>
    {
#if NET_2_0
		TDictSet<T> set = new TDictSet<T>();
#else
        HashSet<T> set = new HashSet<T>();
#endif
        public int Count
        {
            get { return set.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(T item)
        {
            set.Add(item);
        }

        public void Clear()
        {
            set.Clear();
        }

        public bool Contains(T item)
        {
            return set.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            set.CopyTo(array, arrayIndex);
        }

        public IEnumerator GetEnumerator()
        {
            return set.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return ((IEnumerable<T>)set).GetEnumerator();
        }

        public bool Remove(T item)
        {
            return set.Remove(item);
        }

#if NET_2_0
		private class TDictSet<V> : ICollection<V>
		{
			Dictionary<V, TDictSet<V>> dict = new Dictionary<V, TDictSet<V>>();

			public int Count
			{
				get { return dict.Count; }
			}

			public bool IsReadOnly
			{
				get { return false; }
			}

			public IEnumerator GetEnumerator()
			{
				return ((IEnumerable)dict.Keys).GetEnumerator();
			}

			IEnumerator<V> IEnumerable<V>.GetEnumerator()
			{
				return dict.Keys.GetEnumerator();
			}

			public bool Add(V item)
			{
				if (!dict.TryGetValue(item))
				{
					dict[item] = this;
					return true;
				}

				return false;
			}

			void ICollection<V>.Add(V item)
			{
				Add(item);
			}

			public void Clear()
			{
				dict.Clear();
			}

			public bool Contains(V item)
			{
				return dict.TryGetValue(item);
			}

			public void CopyTo(V[] array, int arrayIndex)
			{
				dict.Keys.CopyTo(array, arrayIndex);
			}

			public bool Remove(V item)
			{
				return dict.Remove(item);
			}
		}
#endif
    }

}
