using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MiscTests.Threads
{
    //https://stackoverflow.com/questions/251391/why-is-lockthis-bad
    [TestFixture]
    public class LockThisTest
    {
        enum LockerMode
        {
            SeparateObjects,
            SameObject,
            This
        }
        class LockerTester
        {
            private readonly LockerMode mode;

            public LockerTester(LockerMode mode)
            {
                this.mode = mode;
                switch (mode)
                {
                    case LockerMode.SeparateObjects:
                        locker = new Object();
                        otherLocker = new Object();
                        break;
                    case LockerMode.SameObject:
                        locker = new Object();
                        otherLocker = locker;
                        break;
                    case LockerMode.This:
                        locker = this;
                        otherLocker = this;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                }
            }

            private object locker = new Object();
            private object otherLocker = new Object();
            public void LongLock()
            {
                Console.WriteLine($"About to perform Long Lock (Mode: {this.mode})");

                lock (locker)
                {
                    Console.WriteLine(DateTime.Now.ToString("mm:ss.fff ") + $"Long Lock (Mode: {this.mode})");
                    System.Threading.Thread.Sleep(5000);
                }
                Console.WriteLine(DateTime.Now.ToString("mm:ss.fff ") + "Released Long Lock");

            }
            public void ShortLock()
            {
                Console.WriteLine($"About to perform Short Lock (Mode: {this.mode})");
                lock (otherLocker)
                {
                    Console.WriteLine(DateTime.Now.ToString("mm:ss.fff ") + $"Short Lock (Mode: {this.mode})");
                    System.Threading.Thread.Sleep(1000);
                }
                Console.WriteLine(DateTime.Now.ToString("mm:ss.fff ") + "Released Short Lock");
            }
        }

        [Test]
        public void TestLocker_SeparateObjects()
        {
            List<string> resultsList = new List<string>();
            int numberOfDone = 0;
            var sw = Stopwatch.StartNew();
            var tester = new LockerTester(LockerMode.SeparateObjects);
            Task.Factory.StartNew(tester.LongLock).ContinueWith((x)=> NotifyDone("Long done", ref numberOfDone, resultsList));
            Thread.Sleep(1); //ensure that first task manages to acquire the lock
            Task.Factory.StartNew(tester.ShortLock).ContinueWith((x) => NotifyDone("Short done", ref numberOfDone, resultsList));
            while (numberOfDone < 2)
            {
                Thread.Sleep(1);
            }
            Console.WriteLine($"All done in {sw.ElapsedMilliseconds}");
            Assert.IsTrue(sw.ElapsedMilliseconds < 5100 && sw.ElapsedMilliseconds > 5000);
            Assert.IsTrue(resultsList.First() == "Short done");
        }

        [Test]
        public void TestLocker_SameObject()
        {
            List<string> resultsList = new List<string>();
            int numberOfDone = 0;
            var sw = Stopwatch.StartNew();
            var tester = new LockerTester(LockerMode.SameObject);
            Task.Factory.StartNew(tester.LongLock).ContinueWith((x) => NotifyDone("Long done", ref numberOfDone, resultsList));
            Thread.Sleep(1); //ensure that first task manages to acquire the lock
            Task.Factory.StartNew(tester.ShortLock).ContinueWith((x) => NotifyDone("Short done", ref numberOfDone, resultsList));
            while (numberOfDone < 2)
            {
                Thread.Sleep(1);
            }
            Console.WriteLine($"All done in {sw.ElapsedMilliseconds}");
            Assert.IsTrue(sw.ElapsedMilliseconds >= 6000 && sw.ElapsedMilliseconds < 8000);
            Assert.AreEqual("Long done", resultsList.First());
        }

        [Test]
        public void TestLocker_LockThis()
        {
            List<string> resultsList = new List<string>();
            int numberOfDone = 0;
            var sw = Stopwatch.StartNew();
            var tester = new LockerTester(LockerMode.This);
            Task.Factory.StartNew(tester.LongLock).ContinueWith((x) => NotifyDone("Long done", ref numberOfDone, resultsList));
            Thread.Sleep(1); //ensure that first task manages to acquire the lock
            Task.Factory.StartNew(tester.ShortLock).ContinueWith((x) => NotifyDone("Short done", ref numberOfDone, resultsList));
            while (numberOfDone < 2)
            {
                Thread.Sleep(1);
            }
            Console.WriteLine($"All done in {sw.ElapsedMilliseconds}");
            Assert.IsTrue(sw.ElapsedMilliseconds >= 6000 && sw.ElapsedMilliseconds < 8000);
            Assert.AreEqual("Long done", resultsList.First());

        }

        [Test]
        public void TestLocker_WaitForCompletion_SeparateObjects_LockParentAsWell()
        {
            List<string> resultsList = new List<string>();
            int numberOfDone = 0;
            var sw = Stopwatch.StartNew();
            var tester = new LockerTester(LockerMode.SeparateObjects);
            Task task;
            lock (tester)
            {
                task = Task.Factory.StartNew(tester.ShortLock).ContinueWith((x) => NotifyDone($"Short done", ref numberOfDone, resultsList));
                task.Wait(3000);
            }
            Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
            Assert.IsTrue(sw.ElapsedMilliseconds < 1100);
            Assert.IsTrue(resultsList.First() == "Short done" && numberOfDone == 1);
        }

        [Test]
        public void TestLocker_WaitForCompletion_LockThis_LockParentAsWell()
        {
            List<string> resultsList = new List<string>();
            int numberOfDone = 0;
            var sw = Stopwatch.StartNew();
            var tester = new LockerTester(LockerMode.This); 
            Task task;
            lock (tester)
            {
                task = Task.Factory.StartNew(tester.ShortLock).ContinueWith((x) => NotifyDone($"Short done", ref numberOfDone, resultsList));
                task.Wait(5000);
            }

            Assert.AreEqual(TaskStatus.WaitingForActivation, task.Status); //not sure why the status is 'waiting for activation', as the task has started working...
            Assert.IsTrue(sw.ElapsedMilliseconds >= 5000);
            Assert.IsTrue(resultsList.Count == 0 && numberOfDone == 0);

        }


        private void NotifyDone(string msg, ref int numberOfDone, List<string> resultsQueue)
        { 
            resultsQueue.Add(msg);
            Console.WriteLine(DateTime.Now.ToString("mm:ss.fff ") + msg);
            numberOfDone++;
        }
      
    }
}
