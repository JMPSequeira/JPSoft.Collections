using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JPSoft.Collections.Generics
{
    public class IndexedDictionary<T> : IList<T> where T : IComparable<T>
    {
        readonly Dictionary<IndexMap, T> _mapToItem = new Dictionary<IndexMap, T>();

        readonly Dictionary<T, IndexMap> _itemToMap = new Dictionary<T, IndexMap>();

        int count = 0;

        public int Count => _itemToMap.Count;

        public bool IsReadOnly => false;

        public T this [int index]
        {
            get => _mapToItem[GetMap(index)];

            set => Insert(index, value);
        }

        public void Add(T item)
        {
            Include(item, count);
        }

        public void Add(IEnumerable<T> items)
        {
            if (!items.Any())
                return;

            Include(items, count);
        }

        public int IndexOf(T item)
        {
            if (_itemToMap.TryGetValue(item, out var existing))
                return existing.Key;

            return -1;
        }

        public void Insert(int index, T item)
        {
            ThrowOnInvalid(index);

            Include(item, index);
        }

        public void InsertRange(int index, IEnumerable<T> items)
        {
            if (!items.Any())
                return;

            ThrowOnInvalid(index);

            Include(items, index);
        }

        public void RemoveAt(int index)
        {
            ThrowOnInvalid(index);

            Exclude(index, 1);
        }

        public bool Remove(T item)
        {
            if (Contains(item))
            {
                Exclude(_itemToMap[item].Key, 1);
                return true;
            }

            return false;
        }

        public void RemoveRange(int startingIndex, int itemCount)
        {
            ThrowOnInvalid(startingIndex);

            var lastIndex = startingIndex + itemCount;

            ThrowOnInvalid(lastIndex);

            Exclude(startingIndex, itemCount);
        }

        public void Clear()
        {
            _itemToMap.Clear();
            _mapToItem.Clear();
        }

        public bool Contains(T item)
        {
            if (_itemToMap.TryGetValue(item, out var existing))
                return true;

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex) =>
        throw new NotImplementedException();

        public IEnumerator<T> GetEnumerator() => _mapToItem.OrderBy(a => a.Key).Select(a => a.Value).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void Include(T item, int index)
        {
            ThrowOnInvalid(item);

            var map = new IndexMap(index);

            if (index > 0)
                SetPreviousAndNext(GetMap(index - 1), map);

            if (index < count - 1)
            {
                var next = GetMap(index + 1);

                next.Increment(1);

                SetPreviousAndNext(map, next);
            }

            _itemToMap.Add(item, map);

            _mapToItem.Add(map, item);

            count++;
        }

        void Include(IEnumerable<T> items, int startingIndex)
        {
            ThrowOnInvalid(items);

            var itemCount = items.Count();

            var map = new IndexMap(startingIndex);

            if (startingIndex > 0)
                SetPreviousAndNext(GetMap(startingIndex - 1), map);

            IndexMap last = null;

            if (startingIndex < count)
            {
                last = GetMap(startingIndex);

                last.Increment(itemCount);
            }

            var previous = map;

            foreach (var item in items)
            {
                map = new IndexMap(++startingIndex);

                SetPreviousAndNext(previous, map);

                _mapToItem.Add(previous, item);

                _itemToMap.Add(item, previous);

                previous = map;
            }

            if (last == null)
                GetMap(startingIndex - 1).Next = null;
            else
                SetPreviousAndNext(GetMap(startingIndex - 1), last);

            count += itemCount;
        }

        void Exclude(int index, int times)
        {
            var lastIndex = index + times;

            for (var i = index; i < lastIndex; i++)
            {
                var map = GetMap(i);
                var value = _mapToItem[map];

                _itemToMap.Remove(value);
                _mapToItem.Remove(map);
            }

            if (lastIndex < count - 1)
            {
                var next = GetMap(lastIndex + 1);

                next.Decrement(times);

                if (index > 0)
                    SetPreviousAndNext(GetMap(index - 1), next);
                else
                    next.Previous = null;
            }
            else if (index > 0)
                GetMap(index - 1).Next = null;

            count -= times;
        }

        static void SetPreviousAndNext(IndexMap previous, IndexMap next)
        {
            previous.Next = next;
            next.Previous = previous;
        }

        IndexMap GetMap(int index) => _itemToMap[_mapToItem[IndexMap.GetDummy(index)]];

        void ThrowOnInvalid(IEnumerable<T> collection)
        {
            if (collection is null)
                throw new ArgumentNullException();

            foreach (var item in collection)
                ThrowOnInvalid(item);
        }

        void ThrowOnInvalid(int index, T item)
        {
            ThrowOnInvalid(index);
            ThrowOnInvalid(item);
        }

        void ThrowOnInvalid(T item)
        {
            if (item == null)
                throw new ArgumentNullException();

            if (_itemToMap.TryGetValue(item, out var indexMap))
                throw new ArgumentException();
        }

        void ThrowOnInvalid(int index)
        {
            if (index < 0 || index >= count)
                throw new ArgumentOutOfRangeException();
        }

    }
    class IndexMap : IComparable<IndexMap>, IEquatable<IndexMap>
    {
        public IndexMap Next { get; set; }

        public IndexMap Previous { get; set; }

        public int Key { get; private set; }

        public IndexMap(int key) => Key = key;

        static IndexMap dummy = new IndexMap(-1);

        public static IndexMap GetDummy(int key)
        {
            dummy.Key = key;

            return dummy;
        }

        public void Increment(int by)
        {
            if (by > 0)
            {
                Next?.Increment(by);

                Key += by;
            }
        }

        public void Decrement(int by)
        {
            if (by > 0)
            {
                Key -= by;

                Next?.Decrement(by);
            }
        }
        public int CompareTo(IndexMap other) => Key.CompareTo(other.Key);

        public bool Equals(IndexMap other) => Key.Equals(other.Key);

        public override bool Equals(object obj) => obj is IndexMap other ? Equals(other) : false;

        public override int GetHashCode() => Key;

    }
}