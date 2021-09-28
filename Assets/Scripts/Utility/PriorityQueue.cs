using System;
using System.Collections.Generic;

namespace Utility
{
    /// <summary>
    /// A fast priority queue or heap implementation.
    /// </summary>
    public class PriorityQueue<T> where T : IComparable<T>
    {
        /// <summary>
        /// The internal list used by the queue. Use with care.
        /// </summary>
        public readonly List<T> _items;

        public bool Contains(T item)
        {
            lock (this)
            {
                return _items.Contains(item);
            }
        }

        public PriorityQueue()
        {
            _items = new List<T>();
        }

        public bool Empty
        {
            get
            {
                lock (this)
                {
                    return _items.Count == 0;
                }
            }
        }

        public T First
        {
            get
            {
                lock (this)
                {
                    return _items.Count > 1 ? _items[0] : _items[_items.Count - 1];
                }
            }
        }

        public void Push(T item)
        {
            lock (this)
            {
                _items.Add(item);
                SiftDown(0, _items.Count - 1);
            }
        }

        public T Pop()
        {
            lock (this)
            {
                T item;
                var last = _items[_items.Count - 1];
                _items.RemoveAt(_items.Count - 1);
                if (_items.Count > 0)
                {
                    item = _items[0];
                    _items[0] = last;
                    SiftUp(0);
                }
                else
                {
                    item = last;
                }
                return item;
            }
        }

        private static int Compare(T a, T b)
        {
            return a.CompareTo(b);
        }

        private void SiftDown(int startPosition, int pos)
        {
            var newItem = _items[pos];
            while (pos > startPosition)
            {
                var parentPosition = (pos - 1) >> 1;
                var parent = _items[parentPosition];
                if (Compare(parent, newItem) <= 0)
                    break;
                _items[pos] = parent;
                pos = parentPosition;
            }
            _items[pos] = newItem;
        }

        private void SiftUp(int pos)
        {
            var endPosition = _items.Count;
            var startPosition = pos;
            var newItem = _items[pos];
            var childPosition = 2 * pos + 1;
            while (childPosition < endPosition)
            {
                var rightPosition = childPosition + 1;
                if (rightPosition < endPosition && Compare(_items[rightPosition], _items[childPosition]) <= 0)
                    childPosition = rightPosition;
                _items[pos] = _items[childPosition];
                pos = childPosition;
                childPosition = 2 * pos + 1;
            }
            _items[pos] = newItem;
            SiftDown(startPosition, pos);
        }
    }
}