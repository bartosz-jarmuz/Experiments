using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace CollectionsTests
{
    [TestClass]
    public class CorruptingList_ImmutableObject
    {
        public class FileType
        {
            private readonly List<string> extensions = new List<string>();

            public FileType(params string[] extensions)
            {
                this.extensions.AddRange(extensions);
            }

            public IList<string> Extensions_IList => extensions;
            public IEnumerable<string> Extensions_IEnumerable => extensions;
            public IReadOnlyCollection<string> Extensions_IReadOnlyCollection_NotReally => extensions;
            public IReadOnlyCollection<string> Extensions_IReadOnlyCollection_RealImplementation => extensions.AsReadOnly();
            public IEnumerable<string> Extensions_IEnumerable_Copy => new List<string>(extensions);
        }

        [TestMethod]
        public void ChangeListItems_IList_CorruptionSuccess()
        {
            FileType picture = new FileType(".bmp", ".jpg");

            string itemToRemove = picture.Extensions_IList.First(x => x ==".bmp");
            picture.Extensions_IList.Remove(itemToRemove);

            picture.Extensions_IList.Add(".exe");

            //and we have an invalid picture object state...
            Assert.IsFalse(picture.Extensions_IList.Contains(".bmp"));
            Assert.IsTrue(picture.Extensions_IList.Contains(".exe"));
        }

        [TestMethod]
        public void ChangeListItems_IEnumerable_CorruptionSuccess()
        {
            FileType picture = new FileType(".bmp", ".jpg");

            //IEnumerable does not allow direct items modification - but you can just cast...
            ((List<string>)picture.Extensions_IEnumerable).RemoveAll(x => x == ".bmp");

            ((List<string>)picture.Extensions_IEnumerable).Add(".exe");

            //and we have an invalid picture object state...
            Assert.IsFalse(picture.Extensions_IEnumerable.Contains(".bmp"));
            Assert.IsTrue(picture.Extensions_IEnumerable.Contains(".exe"));
        }

        [TestMethod]
        public void ChangeListItems_IReadOnlyCollection_CorruptionSuccess()
        {
            FileType picture = new FileType(".bmp", ".jpg");

            //IReadOnlyCollection does not allow direct items modification - but you can just cast...
            ((List<string>)picture.Extensions_IReadOnlyCollection_NotReally).RemoveAll(x => x == ".bmp");

            ((List<string>)picture.Extensions_IReadOnlyCollection_NotReally).Add(".exe");

            //and we have an invalid picture object state...
            Assert.IsFalse(picture.Extensions_IReadOnlyCollection_NotReally.Contains(".bmp"));
            Assert.IsTrue(picture.Extensions_IReadOnlyCollection_NotReally.Contains(".exe"));
        }

        [TestMethod]
        public void ChangeListItems_IReadOnlyCollection_CorruptionFailed()
        {
            FileType picture = new FileType(".bmp", ".jpg");
            
            //Cannot cast to List, because the list is actually a readonly
            Assert.ThrowsException<InvalidCastException>(() =>
            {
                ((List<string>) picture.Extensions_IReadOnlyCollection_RealImplementation).RemoveAll(x => x == ".bmp");
            });
        }

        [TestMethod]
        public void ChangeListItems_IEnumerableCopy_CorruptionFailed()
        {
            FileType picture = new FileType(".bmp", ".jpg");

            //IEnumerable does not allow direct items modification - but you can just cast...
            ((List<string>)picture.Extensions_IEnumerable_Copy).RemoveAll(x => x == ".bmp");

            ((List<string>)picture.Extensions_IEnumerable_Copy).Add(".exe");

            //but we still have a valid picture object state... because the modification happens on the copy of the list
            Assert.IsTrue(picture.Extensions_IEnumerable.Contains(".bmp"));
            Assert.IsFalse(picture.Extensions_IEnumerable.Contains(".exe"));
        }

        [TestMethod]
        public void ChangeListItems_AccessPrivateObject()
        {
            FileType picture = new FileType(".bmp", ".jpg");
            //if somebody is really stubborn, it's hard to stop from breaking in.

            FieldInfo backingField = typeof(FileType).GetField("extensions", BindingFlags.NonPublic |BindingFlags.Instance);
            List<string> actualList = (List<string>)backingField.GetValue(picture);
            actualList.RemoveAll(x => x == ".bmp");
            actualList.Add(".exe");

            //and we have an invalid picture object state...
            Assert.IsFalse(picture.Extensions_IReadOnlyCollection_RealImplementation.Contains(".bmp"));
            Assert.IsTrue(picture.Extensions_IReadOnlyCollection_RealImplementation.Contains(".exe"));

            //however, the point is to communicate *the intent* of the class clearly to prevent misuse, not try to make it "unhackable"
        }

    }
}
