using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JPSoft.Collections.Generics
{
	/// <summary>
	/// A collection accepting no nulls or duplicates.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class HashList<T> : IList<T>
	{
		struct Entry
		{
			public int Hash { get; set; }

			public int Next { get; set; }

			public int Index { get; set; }
		}

		int _count;

		int _freeCount;

		int _freeEntry;

		int _length;

		T[] _items;

		Entry[] _entries;

		int[] _buckets;

		IEqualityComparer<T> _comparer;

		public HashList() : this(2, EqualityComparer<T>.Default) { }

		public HashList(IEnumerable<T> items, IEqualityComparer<T> comparer) : this(items.Count(), comparer)
			=> InsertRange(0, items);

		public HashList(IEnumerable<T> items) : this(items, EqualityComparer<T>.Default) { }

		public HashList(int capacity) : this(capacity, EqualityComparer<T>.Default) { }

		public HashList(int capacity, IEqualityComparer<T> comparer)
		{
			if (capacity < 0)
				throw new ArgumentException("Capacity cannot be negative", nameof(capacity));

			_length = GetNextPowerOfTwo(capacity);

			_freeEntry = -1;

			_entries = new Entry[_length];

			_buckets = new int[_length];

			for (var i = 0; i < _length; i++)
				_buckets[i] = -1;

			_items = new T[_length];

			_comparer = comparer ?? EqualityComparer<T>.Default;
		}

		public T this[int index]
		{
			get
			{
				if (index > -1 && index < _count)
					return _items[index];

				throw new ArgumentOutOfRangeException(nameof(index));
			}
			set => Include(value, index, false);
		}

		public IEqualityComparer<T> Comparer => _comparer;

		public int Count => _count;

		public bool IsReadOnly => false;

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
				throw new ArgumentNullException(nameof(items));

			if (index < 0 || index > _count)
				throw new ArgumentOutOfRangeException(nameof(index));

			var backup = MemberwiseClone();

			var count = 0;

			if (items is ICollection<T> collection)
				count = collection.Count;
			else
				count = items.Count();

			if (count + _count > _length)
			{
				_length = count + _count;

				Resize(true, count, index);
			}

			try
			{
				foreach (var item in items)
					Include(item, index++, true, false);
			}
			catch (ArgumentNullException)
			{
				Rollback();

				throw new ArgumentException("Inserted collection contained at least one null item.");
			}
			catch (DuplicateEntryException)
			{
				Rollback();

				if (items.Distinct().Count() == items.Count())
					throw new DuplicateEntryException("Collection contains at least one item that already exist.", nameof(items));

				throw new DuplicateEntryException("Collection contains at least one duplicate.", nameof(items));
			}

			void Rollback()
			{
				var restored = backup as HashList<T>;

				_count = restored._count;
				_freeCount = restored._freeCount;
				_freeEntry = restored._freeEntry;
				_length = restored._length;
				_items = restored._items;
				_buckets = restored._buckets;
				_entries = restored._entries;
				_comparer = restored._comparer;
			}
		}

		public bool Remove(T item)
		=> item == null ? false : Remove(item, true);

		public void RemoveAt(int index)
		{
			if (index < 0 || index >= _count)
				throw new ArgumentOutOfRangeException(nameof(index));

			Remove(_items[index], true);
		}

		public void RemoveRange(int startingIndex, int count)
		{
			if (startingIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(startingIndex));

			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));


			var plusCount = startingIndex + count;

			if (plusCount > _count)
				throw new ArgumentException($"The provided startingIndex {startingIndex} and count {count} do not represent a valid range");

			for (var i = startingIndex; i < plusCount; i++)
				Remove(_items[i], false);

			RemoveItem(startingIndex, count);

			_count -= count;
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (var i = 0; i < _count; i++)
				yield return _items[i];
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		int FindIndex(T item)
		{
			var hash = _comparer.GetHashCode(item);

			return FindIndex(item, hash, hash & (_length - 1));
		}

		int FindIndex(T item, int hash, int bucket)
		{
			for (var i = _buckets[bucket]; i >= 0; i = _entries[i].Next)
				if (_entries[i].Hash == hash && _comparer.Equals(item, _items[_entries[i].Index]))
					return _entries[i].Index;

			return -1;
		}

		int FindEntry(T item)
		{
			var hash = _comparer.GetHashCode(item);

			return FindEntry(item, hash, hash & (_length - 1));
		}

		int FindEntry(T item, int hash, int bucket)
		{
			for (var i = _buckets[bucket]; i >= 0; i = _entries[i].Next)
				if (_entries[i].Hash == hash && _comparer.Equals(item, _items[_entries[i].Index]))
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

		void Include(T item, int index, bool add, bool copy = true)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			if (index < 0 || index > _count)
				throw new ArgumentOutOfRangeException(nameof(index));

			var hash = _comparer.GetHashCode(item);

			var bucket = hash & (_length - 1);

			var existingIndex = FindIndex(item, hash, bucket);

			if (existingIndex >= 0)
				if (add || existingIndex != index)
					throw new DuplicateEntryException($"HashList already contains item {_items[existingIndex]}", nameof(item));

			var entry = GetFreeEntry();

			if (add)
			{
				if (_count == _length)
				{
					Resize();
					bucket = hash & (_length - 1);
				}

				IncludeItem(item, index, copy);

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

		void IncludeItem(T item, int index, bool copy)
		{
			if (index != _count && copy)
				Array.Copy(_items, index, _items, index + 1, _count - index);

			_items[index] = item;
		}

		int GetNextPowerOfTwo(int value)
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
			var hash = _comparer.GetHashCode(item);

			var bucket = hash & (_length - 1);

			var last = -1;

			for (var i = _buckets[bucket]; i >= 0; i = _entries[i].Next, last = i)
			{
				if (hash == _entries[i].Hash && Comparer.Equals(item, _items[_entries[i].Index]))
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

		void RemoveItem(int startingIndex, int count)
		{
			var plusCount = startingIndex + count;

			Array.Copy(_items, plusCount, _items, startingIndex, _count - plusCount);
		}

		void Resize(bool asPowerOfTwo = false, int offset = 0, int startingIndex = 0)
		{
			if (asPowerOfTwo)
				_length = GetNextPowerOfTwo(_length);
			else
				_length = _length * 2;

			var newBuckets = new int[_length];

			var newEntries = new Entry[_length];

			var newItems = new T[_length];

			Array.Copy(_entries, newEntries, _count);

			for (var i = 0; i < newBuckets.Length; i++)
				newBuckets[i] = -1;

			if (offset > 0)
			{
				for (var i = 0; i < _count; i++)
				{
					if (newEntries[i].Hash >= 0)
					{
						var bucket = newEntries[i].Hash & (_length - 1);

						newEntries[i].Next = newBuckets[bucket];

						if (newEntries[i].Index >= startingIndex)
							newEntries[i].Index += offset;

						newBuckets[bucket] = i;
					}
				}

				if (startingIndex > 0)
					Array.Copy(_items, newItems, startingIndex);

				Array.Copy(_items, startingIndex, newItems, startingIndex + offset, _count - startingIndex);
			}
			else
			{
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
			}

			_buckets = newBuckets;
			_entries = newEntries;
			_items = newItems;
		}
	}
}