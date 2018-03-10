using System;
using System.Collections.Generic;
using System.Linq;
using JPSoft.Collections.Generics;
using NUnit.Framework;

namespace JPSoft.Collections.Tests
{
    public class IndexedDictionaryTests
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

        IndexedDictionary<Dummy> dic;

        [SetUp]
        public void Setup()
        {
            dic = new IndexedDictionary<Dummy>();
        }

        [Test]
        public void Add_Empty_Adds()
        {
            dic.Add(new Dummy());

            Assert.IsNotEmpty(dic);
        }

        [Test]
        public void Add_NotEmpty_Appends()
        {
            dic.Add(new Dummy());
            var last = new Dummy();
            dic.Add(last);

            Assert.AreEqual(last, dic[1]);
        }

        [Test]
        public void Add_Existing_Throws()
        {
            var same = new Dummy();

            dic.Add(same);

            Assert.Throws<ArgumentException>(() => dic.Add(same));
        }

        [Test]
        public void Add_Null_Throws()
        {
            Dummy invalid = null;
            Assert.Throws<ArgumentNullException>(() => dic.Add(invalid));
        }

        [Test]
        public void Add_EmptyWithValidCollection_Adds()
        {
            var items = new List<Dummy>
            {
                new Dummy(),
                new Dummy()
            };

            dic.Add(items);

            Assert.IsNotEmpty(dic);
        }

        [Test]
        public void Add_NotEmptyWithValidCollection_Appends()
        {
            var items = new List<Dummy>
            {
                new Dummy(),
                new Dummy()
            };

            dic.Add(new Dummy());

            dic.Add(items);

            Assert.IsTrue(dic.Skip(1).SequenceEqual(items));
        }

        [Test]
        public void Add_CollectionWithNull_Throws()
        {
            var items = new List<Dummy>
            {
                new Dummy(),
                new Dummy(),
                null
            };

            Assert.Throws<ArgumentNullException>(() => dic.Add(items));
        }

        [Test]
        public void Add_CollectionWithExisting_Throws()
        {
            var same = new Dummy();
            dic.Add(same);

            var items = new List<Dummy>
            {
                new Dummy(),
                new Dummy(),
                same
            };

            Assert.Throws<ArgumentException>(() => dic.Add(items));
        }
    }
}