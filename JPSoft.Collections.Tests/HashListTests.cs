using System;
using System.Collections.Generic;
using System.Linq;
using JPSoft.Collections.Generics;
using NUnit.Framework;

namespace JPSoft.Collections.Tests
{
	public class HashListTests
	{

		public class Dummy : IComparable<Dummy>, IEquatable<Dummy>
		{
			static int count = 0;

			readonly int index = count++;

			public int CompareTo(Dummy other)
			{
				return index.CompareTo(other.index);
			}

			public bool Equals(Dummy other) => index == other.index;
		}

		HashList<Dummy> hashList;

		[SetUp]
		public void Setup()
		{
			hashList = new HashList<Dummy>();
		}

		[Test]
		public void Add_Valid_Adds()
		{
			hashList.Add(new Dummy());

			Assert.IsNotEmpty(hashList);
		}

		[Test]
		public void Add_Valid_CountReflectsAdded()
		{
			hashList.Add(new Dummy());
			hashList.Add(new Dummy());

			Assert.AreEqual(2, hashList.Count);
		}

		[Test]
		public void Add_Existing_ThrowsArgumentException()
		{
			var same = new Dummy();

			hashList.Add(same);

			Assert.Throws<DuplicateEntryException>(() => hashList.Add(same));
		}

		[Test]
		public void Add_Null_ThrowsArgumentNullException()
		{
			Dummy invalid = null;
			Assert.Throws<ArgumentNullException>(() => hashList.Add(invalid));
		}

		[Test]
		public void AddRange_Valid_Adds()
		{
			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy()
			};

			hashList.AddRange(items);

			Assert.IsNotEmpty(hashList);
		}

		[Test]
		public void AddRange_Valid_Appends()
		{
			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy()
			};

			hashList.Add(new Dummy());

			hashList.AddRange(items);

			Assert.IsTrue(hashList.Skip(1).SequenceEqual(items));
		}

		[Test]
		public void AddRange_CollectionWithNull_ThrowsArgumentNullException()
		{
			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy(),
				null
			};

			Assert.Throws<ArgumentNullException>(() => hashList.AddRange(items));
		}

		[Test]
		public void AddRange_CollectionWithExisting_ThrowsArgumentException()
		{
			var same = new Dummy();

			hashList.Add(same);

			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy(),
				same
			};

			Assert.Throws<DuplicateEntryException>(() => hashList.AddRange(items));
		}

		[Test]
		public void AddRange_Valid_CountReflectsAdded()
		{
			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy(),
			};

			hashList.AddRange(items);

			Assert.AreEqual(2, hashList.Count);
		}

		[Test]
		public void IndexOf_Existing_CorrectIndex()
		{
			var dummyAtOne = new Dummy();

			hashList.AddRange(new Dummy[] { dummyAtOne, new Dummy(), new Dummy() });

			Assert.AreEqual(0, hashList.IndexOf(dummyAtOne));
		}

		[Test]
		public void IndexOf_NonExisting_CorrectIndex()
		{
			hashList.AddRange(new Dummy[] { new Dummy(), new Dummy() });

			Assert.AreEqual(-1, hashList.IndexOf(new Dummy()));
		}

		[Test]
		[TestCase(0)]
		[TestCase(1)]
		[TestCase(2)]
		public void Insert_ValidIndex_Inserts(int validIndex)
		{
			hashList.AddRange(new Dummy[] { new Dummy(), new Dummy(), new Dummy() });

			var dummy = new Dummy();

			hashList.Insert(validIndex, dummy);

			Assert.AreEqual(dummy, hashList[validIndex]);
		}

		public void Insert_ValidIndex_CountReflectsInserted(int validIndex)
		{
			hashList.AddRange(new Dummy[] { new Dummy(), new Dummy(), new Dummy() });

			var dummy = new Dummy();

			hashList.Insert(validIndex, dummy);

			Assert.AreEqual(4, hashList.Count);
		}

		[Test]
		[TestCase(-1)]
		[TestCase(3)]
		public void Insert_InvalidIndex_ThrowsArgumentOutOfRange(int invalidIndex)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() =>
				hashList.Insert(invalidIndex, new Dummy()));
		}

		[Test]
		public void Insert_Existing_ThrowsArgumentException()
		{
			var dummy = new Dummy();

			hashList.Add(dummy);

			Assert.Throws<DuplicateEntryException>(() =>
				hashList.Insert(0, dummy));
		}

		[Test]
		public void Insert_Valid_CountReflectsAdded()
		{
			hashList.Add(new Dummy());
			hashList.Insert(0, new Dummy());

			Assert.AreEqual(2, hashList.Count);
		}

		[Test]
		public void Insert_Null_ThrowsArgumentNullException()
		{
			var dummy = new Dummy();

			hashList.Add(dummy);

			Assert.Throws<ArgumentNullException>(() =>
				hashList.Insert(0, null));
		}

		[Test]
		[TestCase(0)]
		[TestCase(1)]
		[TestCase(2)]
		public void InsertRange_ValidCollection_Insert(int index)
		{
			var first = new Dummy();

			var items = new List<Dummy>
			{
				first,
				new Dummy(),
				new Dummy()
			};

			hashList.AddRange(new Dummy[] { new Dummy(), new Dummy(), new Dummy() });

			hashList.InsertRange(index, items);

			Assert.AreEqual(first, hashList[index]);
		}

		[Test]
		public void InsertRange_CollectionWithNull_ThrowsArgumentNullException()
		{
			hashList.Add(new Dummy());

			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy(),
				null
			};

			Assert.Throws<ArgumentNullException>(() => hashList.InsertRange(0, items));
		}

		[Test]
		public void InsertRange_CollectionWithExisting_ThrowsArgumentException()
		{
			var same = new Dummy();
			hashList.Add(same);

			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy(),
				same
			};

			Assert.Throws<DuplicateEntryException>(() => hashList.InsertRange(0, items));
		}

		[Test]
		[TestCase(-1)]
		[TestCase(3)]
		public void InsertRange_InvalidIndex_ThrowsArgumentOutOfRange(int invalidIndex)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() =>
				hashList.InsertRange(invalidIndex, new Dummy[] { new Dummy(), new Dummy() }));
		}

		[Test]
		public void InsertRange_ValidCollection_CountReflectsAdded()
		{
			hashList.Add(new Dummy());

			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy()
			};

			hashList.InsertRange(0, items);

			Assert.AreEqual(3, hashList.Count);
		}

		[Test]
		public void Remove_Existing_Removes()
		{
			var existing = new Dummy();

			hashList.Add(existing);

			hashList.Remove(existing);

			Assert.IsEmpty(hashList);
		}

		[Test]
		public void Remove_Existing_True()
		{
			var existing = new Dummy();

			hashList.Add(existing);

			Assert.IsTrue(hashList.Remove(existing));
		}

		[Test]
		public void Remove_Existing_CountReflectsRemoved()
		{
			var existing = new Dummy();

			hashList.Add(existing);

			hashList.Add(new Dummy());

			hashList.Remove(existing);

			Assert.AreEqual(1, hashList.Count);
		}

		[Test]
		public void Remove_Existing_False()
		{
			Assert.IsFalse(hashList.Remove(new Dummy()));
		}

		[Test]
		[TestCase(0)]
		[TestCase(1)]
		[TestCase(2)]
		public void RemoveAt_Existing_Removes(int index)
		{
			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy(),
				new Dummy(),
				new Dummy(),
				new Dummy()
			};

			var expected = items.ElementAt(index);

			hashList.AddRange(items);

			hashList.RemoveAt(index);

			Assert.AreNotEqual(expected, hashList[index]);
		}

		[Test]
		public void RemoveAt_Existing_CountReflects()
		{
			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy(),
				new Dummy(),
				new Dummy(),
			};

			hashList.AddRange(items);

			hashList.RemoveAt(1);

			Assert.AreEqual(3, hashList.Count);
		}

		[Test]
		[TestCase(-1)]
		[TestCase(5)]
		public void RemoveAt_InvalidIndex_ThrowsArgumentOutOfRangeException(int index)
		{
			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy(),
				new Dummy(),
				new Dummy(),
			};

			hashList.AddRange(items);

			Assert.Throws<ArgumentOutOfRangeException>(() => hashList.RemoveAt(index));
		}

		[Test]
		public void RemoveRange_Valid_CountReflects()
		{
			hashList.Add(new Dummy());

			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy()
			};

			hashList.AddRange(items);

			hashList.RemoveRange(0, 2);

			Assert.AreEqual(1, hashList.Count);
		}

		[Test]
		public void RemoveRange_Valid_Removes()
		{
			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy()
			};

			hashList.AddRange(items);

			hashList.RemoveRange(0, 2);

			Assert.IsEmpty(hashList);
		}

		[Test]
		[TestCase(0, 1)]
		[TestCase(1, 1)]
		[TestCase(0, 2)]
		[TestCase(2, 1)]
		public void RemoveRange_Valid_RemovesCorrect(int index, int count)
		{
			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy(),
				new Dummy(),
				new Dummy()
			};

			hashList.AddRange(items);
			items.RemoveRange(index, count);
			hashList.RemoveRange(index, count);

			Assert.IsTrue(items.SequenceEqual(hashList));
		}

		[Test]
		[TestCase(-1, 1)]
		[TestCase(1, -1)]
		public void RemoveRange_NegativeIndex_ThrowsArgumentOutOfRangeException(int index, int count)
		{

			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy()
			};

			hashList.AddRange(items);

			Assert.Throws<ArgumentOutOfRangeException>(() => hashList.RemoveRange(index, count));
		}

		[Test]
		public void RemoveRange_OutOfRange_ThrowsArgumentOutOfRangeException()
		{
			var count = -1;

			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy()
			};

			hashList.AddRange(items);

			Assert.Throws<ArgumentOutOfRangeException>(() => hashList.RemoveRange(0, count));
		}

		[Test]
		public void IndexGet_Valid_Item()
		{
			var dummy = new Dummy();

			hashList.Add(dummy);

			Assert.AreEqual(dummy, hashList[0]);
		}


		[Test]
		[TestCase(-1)]
		[TestCase(2)]
		public void IndexGet_Invalid_ArgumentOutOfRangeException(int index)
		{
			var dummy = new Dummy();

			hashList.Add(dummy);

			Assert.Throws<ArgumentOutOfRangeException>(() => hashList[index].ToString());
		}

		[Test]
		public void IndexSet_Valid_Adds()
		{
			var dummy = new Dummy();

			hashList[0] = dummy;

			Assert.AreEqual(dummy, hashList[0]);
		}

		[Test]
		public void IndexSet_Valid_CountReflects()
		{

			hashList[0] = new Dummy();

			Assert.AreEqual(1, hashList.Count);
		}

		[Test]
		[TestCase(-1)]
		[TestCase(2)]
		public void IndexSet_InvalidIndex_ThrowsArgumentOutOfRange(int index)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => hashList[index] = new Dummy());
		}
	}
}