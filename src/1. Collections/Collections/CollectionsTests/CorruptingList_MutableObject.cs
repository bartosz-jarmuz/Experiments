using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace CollectionsTests
{
    [TestClass]
    public class CorruptingList_MutableObject
    {
        public class Extension
        {
            public Extension(string value)
            {
                Value = value;
            }

            public string Value { get; set; }
        }

        public class FileType
        {
            private readonly List<Extension> extensions = new List<Extension>();

            public FileType(params Extension[] extensions)
            {
                this.extensions.AddRange(extensions);
            }

            public List<Extension> Extensions_List => extensions;
            public IEnumerable<Extension> Extensions_IEnumerable => this.extensions;
            public IEnumerable<Extension> Extensions_IEnumerable_Copy => new List<Extension>(extensions);
            public IReadOnlyCollection<Extension> Extensions_IReadOnlyCollection_RealImplementation => extensions.AsReadOnly();

            public ImmutableList<Extension> Extensions_ImmutableList => ImmutableList.CreateRange<Extension>(this.extensions);
            public IReadOnlyCollection<Extension> Extensions_ImmutableIReadOnlyCollection => ImmutableList.CreateRange<Extension>(this.extensions);
        }

        [TestMethod]
        public void ChangeListItems_DereferencingIsNotAnOption_CorruptionFailed()
        {
            FileType picture = new FileType(new Extension(".bmp"), new Extension(".jpg"));

            //we can get the bmp object from the list... 
            Extension bmp = picture.Extensions_List.FirstOrDefault(x => x.Value == ".bmp");
            //but changing the value will only dereference the current variable, *not* the actual referenced object
            bmp = new Extension(".exe");

            //...so we still have a valid state
            Assert.AreEqual(0, picture.Extensions_List.Count(x => x.Value == ".exe"));
            Assert.AreEqual(1, picture.Extensions_List.Count(x => x.Value == ".bmp"));
        }

        [TestMethod]
        public void ChangeListItems_IEnumerable_Dereference_CorruptionSuccess()
        {
            FileType picture = new FileType(new Extension(".bmp"), new Extension(".jpg"));

            //we can get the bmp object from the list... 
            Extension bmp = picture.Extensions_IEnumerable.FirstOrDefault(x => x.Value == ".bmp");
            int index = ((List<Extension>)picture.Extensions_IEnumerable).IndexOf(bmp);
            //and dereference the item at that index
            ((List<Extension>)picture.Extensions_IEnumerable)[index] = new Extension(".exe");

            //...and it's changed in the object, we have an invalid state
            Assert.AreEqual(1, picture.Extensions_IEnumerable.Count(x => x.Value == ".exe"));
            Assert.AreEqual(0, picture.Extensions_IEnumerable.Count(x => x.Value == ".bmp"));
        }

        [TestMethod]
        public void ChangeListItems_IEnumerableCopy_Dereference_CorruptionFailed()
        {
            FileType picture = new FileType(new Extension(".bmp"), new Extension(".jpg"));

            //we can get the bmp object from the list... 
            Extension bmp = picture.Extensions_IEnumerable_Copy.FirstOrDefault(x => x.Value == ".bmp");
            int index = ((List<Extension>) picture.Extensions_IEnumerable_Copy).IndexOf(bmp);
            //and dereference the item at that index
            ((List<Extension>)picture.Extensions_IEnumerable_Copy)[index] = new Extension(".exe");

            //...but the list is a copy, thus we're dereferencing the item on the copy...
            Assert.AreEqual(0, picture.Extensions_IEnumerable_Copy.Count(x => x.Value == ".exe"));
            Assert.AreEqual(1, picture.Extensions_IEnumerable_Copy.Count(x => x.Value == ".bmp"));
        }
        
        [TestMethod]
        public void ChangeListItems_IEnumerableCopy_ChangeObjectValue_CorruptionSucceeds()
        {
            FileType picture = new FileType(new Extension(".bmp"), new Extension(".jpg"));

            //we can get the bmp object from the list... 
            Extension bmp = picture.Extensions_IEnumerable_Copy.FirstOrDefault(x => x.Value == ".bmp");
            //and since it's mutable, change its property
            bmp.Value = ".exe";

            //...and regardless whether list is a copy, the object has changed, and the list itself is broken...
            Assert.AreEqual(1, picture.Extensions_IEnumerable_Copy.Count(x => x.Value == ".exe"));
            Assert.AreEqual(0, picture.Extensions_IEnumerable_Copy.Count(x => x.Value == ".bmp"));
        }

        [TestMethod]
        public void ChangeListItems_ImmutableList_CorruptionFailed()
        {
            FileType picture = new FileType(new Extension(".bmp"), new Extension(".jpg"));
            //immutable list allows you to make changes...
            picture.Extensions_ImmutableList.RemoveAll(x => x.Value == ".bmp");
            picture.Extensions_ImmutableList.Add(new Extension(".exe"));

            //...but the changes only happen on the new copy of the object, whereas the real one is still there
            Assert.AreEqual(0, picture.Extensions_ImmutableList.Count(x=>x.Value == ".exe"));
            Assert.AreEqual(1, picture.Extensions_ImmutableList.Count(x=>x.Value == ".bmp"));

            //however, much like with the string, operations on this object (which are void on 'regular' Lists) now return the changed value
            var modifiedList = picture.Extensions_ImmutableList.Add(new Extension(".exe"));
            Assert.AreEqual(1, modifiedList.Count(x => x.Value == ".exe"));
        }

    }
}
