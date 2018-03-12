using System;
using System.Collections.Generic;
using System.Linq;
using JPSoft.Collections.Generics;
using NUnit.Framework;

namespace JPSoft.Collections.Tests
{
	public class HashListTests
	{

		public class Dummy : IComparable<Dummy>
		{
			static int count = 0;
			readonly int index = count++;

			public int CompareTo(Dummy other)
			{
				return index.CompareTo(other.index);
			}
		}

		HashList<Dummy> dic;

		[SetUp]
		public void Setup()
		{
			dic = new HashList<Dummy>();
		}

		[Test]
		public void Add_Valid_Adds()
		{
			dic.Add(new Dummy());

			Assert.IsNotEmpty(dic);
		}

		[Test]
		public void Add_Valid_CountReflectsAdded()
		{
			dic.Add(new Dummy());
			dic.Add(new Dummy());

			Assert.AreEqual(2, dic.Count);
		}

		[Test]
		public void Add_Existing_ThrowsArgumentException()
		{
			var same = new Dummy();

			dic.Add(same);

			Assert.Throws<ArgumentException>(() => dic.Add(same));
		}

		[Test]
		public void Add_Null_ThrowsArgumentNullException()
		{
			Dummy invalid = null;
			Assert.Throws<ArgumentNullException>(() => dic.Add(invalid));
		}

		[Test]
		public void AddRange_Valid_Adds()
		{
			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy()
			};

			dic.AddRange(items);

			Assert.IsNotEmpty(dic);
		}

		[Test]
		public void AddRange_Valid_Appends()
		{
			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy()
			};

			dic.Add(new Dummy());

			dic.AddRange(items);

			Assert.IsTrue(dic.Skip(1).SequenceEqual(items));
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

			Assert.Throws<ArgumentNullException>(() => dic.AddRange(items));
		}

		[Test]
		public void AddRange_CollectionWithExisting_ThrowsArgumentException()
		{
			var same = new Dummy();
			dic.Add(same);

			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy(),
				same
			};

			Assert.Throws<ArgumentException>(() => dic.AddRange(items));
		}

		[Test]
		public void AddRange_Valid_CountReflectsAdded()
		{
			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy(),
			};

			dic.AddRange(items);

			Assert.AreEqual(2, dic.Count);
		}

		[Test]
		public void IndexOf_Existing_CorrectIndex()
		{
			var dummyAtOne = new Dummy();

			dic.AddRange(new Dummy[] { dummyAtOne, new Dummy(), new Dummy() });

			Assert.AreEqual(0, dic.IndexOf(dummyAtOne));
		}

		[Test]
		public void IndexOf_NonExisting_CorrectIndex()
		{
			dic.AddRange(new Dummy[] { new Dummy(), new Dummy() });

			Assert.AreEqual(-1, dic.IndexOf(new Dummy()));
		}

		[Test]
		[TestCase(0)]
		[TestCase(1)]
		[TestCase(2)]
		public void Insert_ValidIndex_Inserts(int validIndex)
		{
			dic.AddRange(new Dummy[] { new Dummy(), new Dummy(), new Dummy() });

			var dummy = new Dummy();

			dic.Insert(validIndex, dummy);

			Assert.AreEqual(dummy, dic[validIndex]);
		}

		public void Insert_ValidIndex_CountReflectsInserted(int validIndex)
		{
			dic.AddRange(new Dummy[] { new Dummy(), new Dummy(), new Dummy() });

			var dummy = new Dummy();

			dic.Insert(validIndex, dummy);

			Assert.AreEqual(4, dic.Count);
		}

		[Test]
		[TestCase(-1)]
		[TestCase(3)]
		public void Insert_InvalidIndex_ThrowsArgumentOutOfRange(int invalidIndex)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() =>
				dic.Insert(invalidIndex, new Dummy()));
		}

		[Test]
		public void Insert_Existing_ThrowsArgumentException()
		{
			var dummy = new Dummy();

			dic.Add(dummy);

			Assert.Throws<ArgumentException>(() =>
				dic.Insert(0, dummy));
		}

		[Test]
		public void Insert_Valid_CountReflectsAdded()
		{
			dic.Add(new Dummy());
			dic.Insert(0, new Dummy());

			Assert.AreEqual(2, dic.Count);
		}

		[Test]
		public void Insert_Null_ThrowsArgumentNullException()
		{
			var dummy = new Dummy();

			dic.Add(dummy);

			Assert.Throws<ArgumentNullException>(() =>
				dic.Insert(0, null));
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

			dic.AddRange(new Dummy[] { new Dummy(), new Dummy(), new Dummy() });

			dic.InsertRange(index, items);

			Assert.AreEqual(first, dic[index]);
		}

		[Test]
		public void InsertRange_CollectionWithNull_ThrowsArgumentNullException()
		{
			dic.Add(new Dummy());

			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy(),
				null
			};

			Assert.Throws<ArgumentNullException>(() => dic.InsertRange(0, items));
		}

		[Test]
		public void InsertRange_CollectionWithExisting_ThrowsArgumentException()
		{
			var same = new Dummy();
			dic.Add(same);

			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy(),
				same
			};

			Assert.Throws<ArgumentException>(() => dic.InsertRange(0, items));
		}

		[Test]
		[TestCase(-1)]
		[TestCase(3)]
		public void InsertRange_InvalidIndex_ThrowsArgumentOutOfRange(int invalidIndex)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() =>
				dic.InsertRange(invalidIndex, new Dummy[] { new Dummy(), new Dummy() }));
		}

		[Test]
		public void InsertRange_ValidCollection_CountReflectsAdded()
		{
			dic.Add(new Dummy());

			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy()
			};

			dic.InsertRange(0, items);

			Assert.AreEqual(3, dic.Count);
		}

		[Test]
		public void Remove_Existing_Removes()
		{
			var existing = new Dummy();

			dic.Add(existing);

			dic.Remove(existing);

			Assert.IsEmpty(dic);
		}

		[Test]
		public void Remove_Existing_True()
		{
			var existing = new Dummy();

			dic.Add(existing);

			Assert.IsTrue(dic.Remove(existing));
		}

		[Test]
		public void Remove_Existing_CountReflectsRemoved()
		{
			var existing = new Dummy();

			dic.Add(existing);

			dic.Add(new Dummy());

			dic.Remove(existing);

			Assert.AreEqual(1, dic.Count);
		}

		[Test]
		public void Remove_Existing_False()
		{
			Assert.IsFalse(dic.Remove(new Dummy()));
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

			dic.AddRange(items);

			dic.RemoveAt(index);

			Assert.AreNotEqual(expected, dic[index]);
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

			dic.AddRange(items);

			dic.RemoveAt(1);

			Assert.AreEqual(3, dic.Count);
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

			dic.AddRange(items);

			Assert.Throws<ArgumentOutOfRangeException>(() => dic.RemoveAt(index));
		}

		[Test]
		public void RemoveRange_Valid_CountReflects()
		{
			dic.Add(new Dummy());

			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy()
			};

			dic.AddRange(items);

			dic.RemoveRange(0, 2);

			Assert.AreEqual(1, dic.Count);
		}

		[Test]
		public void RemoveRange_Valid_Removes()
		{
			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy()
			};

			dic.AddRange(items);

			dic.RemoveRange(0, 2);

			Assert.IsEmpty(dic);
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

			dic.AddRange(items);
			items.RemoveRange(index, count);
			dic.RemoveRange(index, count);

			Assert.IsTrue(items.SequenceEqual(dic));
		}

		[Test]
		[TestCase(-1)]
		[TestCase(5)]
		public void RemoveRange_InvalidIndex_ThrowsArgumentOutOfRangeException(int index)
		{

			var items = new List<Dummy>
			{
				new Dummy(),
				new Dummy()
			};

			dic.AddRange(items);

			Assert.Throws<ArgumentOutOfRangeException>(() => dic.RemoveRange(index, 1));
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

			dic.AddRange(items);

			Assert.Throws<ArgumentOutOfRangeException>(() => dic.RemoveRange(0, count));
		}

		[Test]
		public void IndexGet_Valid_Item()
		{
			var dummy = new Dummy();

			dic.Add(dummy);

			Assert.AreEqual(dummy, dic[0]);
		}


		[Test]
		[TestCase(-1)]
		[TestCase(2)]
		public void IndexGet_Invalid_ThrowsKeyNotFoundException(int index)
		{
			var dummy = new Dummy();

			dic.Add(dummy);

			Assert.Throws<ArgumentOutOfRangeException>(() => dic[index].ToString());
		}

		[Test]
		public void IndexSet_Valid_Adds()
		{
			var dummy = new Dummy();

			dic[0] = dummy;

			Assert.AreEqual(dummy, dic.ElementAt(0));
		}

		[Test]
		public void IndexSet_Valid_CountReflects()
		{

			dic[0] = new Dummy();

			Assert.AreEqual(1, dic.Count);
		}

		[Test]
		public void IndexSet_InvalidIndex_ThrowsKeyNotFoundE()
		{

			dic[0] = new Dummy();

			Assert.AreEqual(1, dic.Count);
		}
	}
}