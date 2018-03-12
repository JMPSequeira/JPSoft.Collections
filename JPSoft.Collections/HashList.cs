using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JPSoft.Collections.Generics
{
	public class HashList<T> : IList<T> where T : IComparable<T>
	{
		protected readonly Dictionary<T, int> _indexes = new Dictionary<T, int>();

		protected readonly Dictionary<int, T> _items = new Dictionary<int, T>();

		protected int InnerCount;

		public int Count => InnerCount;

		public virtual bool IsReadOnly => false;

		public virtual T this[int index]
		{
			get
			{
				ThrowOnInvalid(index);

				return _items[index];
			}

			set
			{
				if (index < 0 && index > InnerCount)
					throw new ArgumentOutOfRangeException();

				ThrowOnInvalid(value);

				Include(index, value);

			}
		}

		public void Clear()
		{
			_items.Clear();
			_indexes.Clear();
		}

		public bool Contains(T item) => _indexes.ContainsKey(item);

		public void CopyTo(T[] array, int arrayIndex)
		{
			if (array is null)
				throw new ArgumentNullException();

			if (array.Rank > 1)
				throw new ArgumentException("Only one-dimensional array is supported.");

			if (arrayIndex < 0)
				throw new ArgumentOutOfRangeException("arrayIndex is less than array lower bound.");

			if (array.Length < arrayIndex + InnerCount)
				throw new ArgumentException("Target array cannot hold source array, starting at index " + arrayIndex);

			for (var i = 0; i < InnerCount; i++)
				array[i] = _items[i];

		}

		public void Add(T item)
		{
			ThrowOnInvalid(item);

			Include(InnerCount, item);
		}

		public void AddRange(IEnumerable<T> items) =>
			Include(InnerCount, items);

		public int IndexOf(T item)
		{
			if (_indexes.TryGetValue(item, out var index))
				return index;

			return -1;
		}

		public void Insert(int index, T item)
		{
			ThrowOnInvalid(index);

			ThrowOnInvalid(item);

			var i = InnerCount;

			for (; i > index; i--)
			{
				var value = _items[i - 1];

				_indexes[value]++;

				_items[i] = value;
			}

			Include(index, item);
		}

		public void InsertRange(int index, IEnumerable<T> items)
		{
			ThrowOnInvalid(index);

			Include(index, items);
		}

		public bool Remove(T item)
		{
			if (_indexes.TryGetValue(item, out var index))
			{
				var threshold = InnerCount - 1;

				for (; index < threshold; index++)
				{
					var value = _items[index + 1];

					_items[index] = value;

					_indexes[value]--;
				}

				Exclude(index, item);

				return true;
			}

			return false;
		}

		public void RemoveAt(int index)
		{
			ThrowOnInvalid(index);

			Remove(_items[index]);
		}

		public void RemoveRange(int index, int count)
		{
			ThrowOnInvalid(index);

			if (count < 0)
				throw new ArgumentOutOfRangeException();

			var lastIndex = index + count;

			if (lastIndex > InnerCount)
				throw new ArgumentException();

			for (; index < lastIndex; index++)
			{
				var item = _items[index];

				_items.Remove(index);

				_indexes.Remove(item);
			}

			for (; lastIndex < InnerCount; lastIndex++)
			{
				var item = _items[lastIndex];

				_items.Remove(lastIndex);

				var current = _indexes[item] -= count;

				_items[current] = item;
			}

			InnerCount -= count;
		}

		public IEnumerator<T> GetEnumerator() =>
		_items.OrderBy(o => o.Key).Select(o => o.Value).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		protected virtual void Include(int index, T item)
		{
			_indexes[item] = index;

			_items[index] = item;

			InnerCount++;
		}

		protected virtual void Include(int index, IEnumerable<T> items)
		{
			var count = GetCountOrThrowOnInvalid(items);

			var i = InnerCount - 1;

			var threshold = index - 1;

			for (; i != threshold; i--)
			{
				var value = _items[i];

				_indexes[value] += count;

				_items[i + count] = value;
			}

			foreach (var item in items)
				Include(index++, item);
		}

		protected virtual void Exclude(int index, T item)
		{
			_indexes.Remove(item);

			_items.Remove(index);

			InnerCount--;
		}

		protected virtual int GetCountOrThrowOnInvalid(IEnumerable<T> items)
		{
			var count = 0;

			foreach (var item in items)
			{
				ThrowOnInvalid(item);

				count++;
			}

			return count;
		}

		protected virtual void ThrowOnInvalid(T item)
		{
			if (item == null)
				throw new ArgumentNullException();

			if (_indexes.ContainsKey(item))
				throw new ArgumentException();
		}

		protected virtual void ThrowOnInvalid(int index)
		{
			if (index < 0 || index >= InnerCount)
				throw new ArgumentOutOfRangeException();
		}
	}
}