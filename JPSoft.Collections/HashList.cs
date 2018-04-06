using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JPSoft.Collections.Generics
{
	public class HashList<T> : IList<T> where T : IEquatable<T>
	{
		struct Entry
		{
			public int Hash { get; set; }

			public int Next { get; set; }

			public int Index { get; set; }
		}

		T[] _items;

		Entry[] _entries;

		int[] _buckets;

		int _count;

		int _freeCount;

		int _freeEntry;

		int _length;

		public HashList()
			=> Init(2);

		public HashList(IEnumerable<T> items)
		{
			Init(items.Count());

			InsertRange(0, items);
		}

		public HashList(int capacity)
		{
			if (capacity < 0)
				throw new ArgumentException("Capacity cannot be negative");

			Init(capacity);
		}

		void Init(int capacity)
		{
			_length = capacity;

			_freeEntry = -1;

			_entries = new Entry[_length];

			_buckets = new int[_length];

			for (var i = 0; i < _length; i++)
				_buckets[i] = -1;

			_items = new T[_length];
		}

		public bool IsReadOnly => false;

		public int Count => _count;

		public T this[int index]
		{
			get
			{
				if (index > -1 && index < _count)
					return _items[index];

				throw new ArgumentOutOfRangeException("index");
			}
			set => Include(value, index, false);
		}

		public void Add(T item) =>
			Include(item, _count, true);

		public void AddRange(IEnumerable<T> items)
			=> InsertRange(_count, items);

		public void Clear()
		{
			if (_count > 0)
			{
				Array.Clear(_buckets, 0, _length);
				Array.Clear(_entries, 0, _length);
				Array.Clear(_items, 0, _count);

				_freeEntry = -1;

				_count = _freeCount = 0;
			}
		}

		public bool Contains(T item)
			=> item != null && FindEntry(item) >= 0;

		public void CopyTo(T[] array, int arrayIndex)
			=> Array.Copy(_items, 0, array, arrayIndex, _count);

		public int IndexOf(T item)
			=> item == null ? -1 : FindIndex(item);

		public void Insert(int index, T item)
			=> Include(item, index, true);

		public void InsertRange(int index, IEnumerable<T> items)
		{
			if (items == null)
				throw new ArgumentNullException("items");

			var hashSet = new HashSet<T>();

			foreach (var item in items)
			{
				if (item == null)
					throw new ArgumentNullException("item");

				if (FindEntry(item) > -1)
					throw new DuplicateEntryException($"HashList already contains item {_items[FindIndex(item)]}", "item");

				if (!hashSet.Add(item))
					throw new DuplicateEntryException($"IEnumerable {items} contains at least one duplicate: {item}", "item");
			}

			var itemCount = hashSet.Count;

			if (_count + itemCount > _length)
			{
				_length = _count + itemCount;
				Resize(true);
			}

			Array.Copy(_items, index, _items, index + itemCount, _count - index);

			var current = index;

			foreach (var item in items)
			{
				var hash = item.GetHashCode();

				IncludeEntry(GetFreeEntry(), current, hash, hash & (_length - 1));

				_items[current++] = item;

				_count++;
			}

		}

		public bool Remove(T item)
			=> item == null ? false : Remove(item, true);

		public void RemoveAt(int index)
		{
			if (index < 0 || index >= _count)
				throw new ArgumentOutOfRangeException("index");

			Remove(_items[index], true);
		}

		public void RemoveRange(int startingIndex, int count)
		{
			if (startingIndex < 0)
				throw new ArgumentOutOfRangeException("startingIndex");

			if (count < 0)
				throw new ArgumentOutOfRangeException("count");


			var plusCount = startingIndex + count;

			if (plusCount > _count)
				throw new ArgumentException($"The provided startingIndex {startingIndex} and count {count} do not represent a valid range");

			for (var i = startingIndex; i < plusCount; i++)
				Remove(_items[i], false);

			RemoveItem(startingIndex, count);

			_count -= count;
		}

		public IEnumerator<T> GetEnumerator() => _items.Take(_count).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		int FindIndex(T item)
		{
			var hash = item.GetHashCode();

			return FindIndex(item, hash, hash & (_length - 1));
		}

		int FindIndex(T item, int hash, int bucket)
		{
			for (var i = _buckets[bucket]; i >= 0; i = _entries[i].Next)
				if (_entries[i].Hash == hash && _items[_entries[i].Index].Equals(item))
					return _entries[i].Index;

			return -1;
		}

		int FindEntry(T item)
		{
			var hash = item.GetHashCode();

			return FindEntry(item, hash, hash & (_length - 1));
		}

		int FindEntry(T item, int hash, int bucket)
		{
			for (var i = _buckets[bucket]; i >= 0; i = _entries[i].Next)
				if (_entries[i].Hash == hash && _items[_entries[i].Index].Equals(item))
					return i;

			return -1;
		}

		int GetFreeEntry()
		{
			if (_freeCount == 0)
				return _count;

			var entry = _freeEntry;

			_freeEntry = _entries[entry].Next;

			_freeCount--;

			return entry;
		}

		void Include(T item, int index, bool add)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			if (index < 0 || index > _count)
				throw new ArgumentOutOfRangeException("index");

			var hash = item.GetHashCode();

			var bucket = hash & (_length - 1);

			var existingIndex = FindIndex(item, hash, bucket);

			if (existingIndex >= 0)
			{
				if (add || existingIndex != index)
					throw new DuplicateEntryException($"HashList already contains item {_items[existingIndex]}", "item");
			}

			var entry = GetFreeEntry();

			if (add)
			{
				if (_count == _length)
				{
					Resize();
					bucket = hash & (_length - 1);
				}

				IncludeItem(item, index);

				_count++;
			}
			else
			{
				if (existingIndex > -1)
					Remove(_items[existingIndex], false);

				_items[index] = item;

				if (index == _count)
					_count++;
			}

			IncludeEntry(entry, index, hash, bucket);

		}

		void IncludeEntry(int entry, int index, int hash, int bucket)
		{
			_entries[entry].Hash = hash;
			_entries[entry].Index = index;
			_entries[entry].Next = _buckets[bucket];

			_buckets[bucket] = entry;
		}

		void IncludeItem(T item, int index)
		{
			if (index != _count)
				Array.Copy(_items, index, _items, index + 1, _count - index);

			_items[index] = item;
		}

		int NextPowerOfTwo(int value)
		{
			value--;

			value |= value >> 1;
			value |= value >> 2;
			value |= value >> 4;
			value |= value >> 8;
			value |= value >> 16;

			return ++value;
		}

		bool Remove(T item, bool removeItem)
		{
			var hash = item.GetHashCode();

			var bucket = hash & (_length - 1);

			var last = -1;

			for (var i = _buckets[bucket]; i >= 0; i = _entries[i].Next, last = i)
			{
				if (hash == _entries[i].Hash && item.Equals(_items[_entries[i].Index]))
				{
					if (last > 0)
						_entries[last].Next = _entries[i].Next;
					else
						_buckets[bucket] = _entries[i].Next;


					_entries[i].Hash = 0;

					_entries[i].Index = 0;

					_entries[i].Next = _freeEntry;

					_freeCount++;

					_freeEntry = i;

					if (removeItem)
					{
						RemoveItem(_entries[i].Index, 1);
						_count--;
					}

					return true;
				}
			}

			return false;
		}

		void RemoveItem(int startingIndex, int itemCount)
		{
			var plusCount = startingIndex + itemCount;

			Array.Copy(_items, plusCount, _items, startingIndex, _count - plusCount);
		}

		void Resize(bool asPowerOfTwo = false)
		{
			if (asPowerOfTwo)
				_length = NextPowerOfTwo(_length);
			else
				_length = _length * 2;

			var newBuckets = new int[_length];

			var newEntries = new Entry[_length];

			var newItems = new T[_length];

			Array.Copy(_entries, newEntries, _count);

			for (var i = 0; i < _length; i++)
				newBuckets[i] = -1;

			for (var i = 0; i < _count; i++)
			{
				if (newEntries[i].Hash >= 0)
				{
					var bucket = newEntries[i].Hash & (_length - 1);

					newEntries[i].Next = newBuckets[bucket];

					newBuckets[bucket] = i;
				}
			}

			Array.Copy(_items, newItems, _count);

			_buckets = newBuckets;
			_entries = newEntries;
			_items = newItems;
		}
	}

	public class DuplicateEntryException : ArgumentException
	{
		public DuplicateEntryException(string message, string paramName) : base(message, paramName) { }
	}
}