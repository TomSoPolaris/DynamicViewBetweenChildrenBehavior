using System;
using Microsoft.Maui.Layouts;

namespace EwDavidForms
{
    public class GenericLayout<T> : Layout, IList<T> where T : View
    {
        T IList<T>.this[int index]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public void Add(T item)
        {
            base.Add(item);
        }

        public bool Contains(T item)
        {
            return base.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        public int IndexOf(T item)
        {
            return base.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            base.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return base.Remove(item);
        }

        protected override ILayoutManager CreateLayoutManager()
        {
            throw new NotImplementedException();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}

