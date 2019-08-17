using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using NUnit.Framework;

//performance measuring makes no sense in debug
#if !DEBUG
namespace MiscTests.LoopPerformance
{


    [TestFixture, Ignore("I am not sure yet what the outcome should be.")]
    public class LoopPerformanceTests
    {
        class TestObject
        {
            public Guid Identifier { get; set; }

            public string Name { get; set; }

            public void DoSomething()
            {
                this.Name = "Name";
            }
        }

        private List<TestObject> GetList(int count)
        {
            List<TestObject> list = new List<TestObject>();
            for (int i = 0; i < count; i++)
            {
                list.Add(new TestObject()
                {
                    Identifier = Guid.NewGuid()
                });
            }

            return list;
        }

        private List<int> GetListOfNumbers(int count)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < count; i++)
            {
                list.Add(i);
            }

            return list;
        }

        [Test]
        public void MeasureForLoop()
        {
            int count = 10000000;
            TestObject[] list = GetList(count).ToArray();

            Stopwatch foreachStopwatch = Stopwatch.StartNew();
            foreach (TestObject testObject in list)
            {
                testObject.DoSomething();
            }
            foreachStopwatch.Stop();

            list = GetList(count).ToArray();
            Stopwatch forStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                TestObject testObject = list[i];
                testObject.DoSomething();
            }
            forStopwatch.Stop();

            Console.WriteLine($"{count} repetitions - foreach {foreachStopwatch.ElapsedMilliseconds}ms -  for - {forStopwatch.ElapsedMilliseconds}ms ");


        }

        [Test]
        public void WithoutAccessingTheObject([Values(5_000_000, 25_000_000, 100_000_000)] int count)
        {
            //this test verifies the findings of this article (pretty much)
            //http://web.archive.org/web/20140827024150/http://codebetter.com/patricksmacchia/2008/11/19/an-easy-and-efficient-way-to-improve-net-code-performances/
            //however, it makes no sense because iterating over a for loop without getting the object is just like counting to X.
            // results change significantly if object as accessed

            List<int> list = this.GetListOfNumbers(count);
            Stopwatch foreachStopwatch = Stopwatch.StartNew();
            foreach (int testObject in list)
            {
                //nothing is done with the object
            }
            foreachStopwatch.Stop();

            list = GetListOfNumbers(count);
            Stopwatch forStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < list.Count; i++)
            {
                //nothing is done with the object - and it's not even assigned
            }
            forStopwatch.Stop();
            Console.WriteLine($"{count} repetitions - foreach {foreachStopwatch.ElapsedMilliseconds}ms -  for - {forStopwatch.ElapsedMilliseconds}ms ");

            Assert.IsTrue(foreachStopwatch.ElapsedMilliseconds > forStopwatch.ElapsedMilliseconds);

            //its not only faster - it's a lot faster
            Assert.IsTrue(foreachStopwatch.ElapsedMilliseconds > forStopwatch.ElapsedMilliseconds + (forStopwatch.ElapsedMilliseconds*2));
        }

        [Test]
        public void AccessTheObject([Values(5_000_000, 25_000_000, 100_000_000)]
            int count)
        {
            //and adding it into a loop also changes the results - even if it's just a loop of ONE iteration!
            //for (int testExecutionIteration = 0; testExecutionIteration < 1; testExecutionIteration++)
            //{

                List<int> list = this.GetListOfNumbers(count);
                int[] array = list.ToArray();

                //warmup - affects the results a lot
                //foreach (int testObject in list)
                //{
                //    if (testObject == -1)
                //    {
                //        Assert.Fail();
                //    }
                //}


                //foreach (int testObject in array)
                //{
                //    if (testObject == -1)
                //    {
                //        Assert.Fail();
                //    }
                //}
                //for (int i = 0; i < list.Count; i++)
                //{
                //    int testObject = list[i];
                //    if (testObject == -1)
                //    {
                //        Assert.Fail();
                //    }
                //}
                //for (int i = 0; i < array.Length; i++)
                //{
                //    int testObject = array[i];
                //    if (testObject == -1)
                //    {
                //        Assert.Fail();
                //    }
                //}

                // results change significantly if object as accessed
                Stopwatch foreachStopwatch = Stopwatch.StartNew();
                foreach (int testObject in list)
                {
                    if (testObject == -1)
                    {
                        Assert.Fail();
                    }
                }

                foreachStopwatch.Stop();

                Stopwatch forStopwatch = Stopwatch.StartNew();
                for (int i = 0; i < list.Count; i++)
                {
                    int testObject = list[i];
                    if (testObject == -1)
                    {
                        Assert.Fail();
                    }
                }

                forStopwatch.Stop();

                Stopwatch foreachArrayStopwatch = Stopwatch.StartNew();
                foreach (int testObject in array)
                {
                    if (testObject == -1)
                    {
                        Assert.Fail();
                    }
                }

                foreachArrayStopwatch.Stop();



                Stopwatch forArrayStopwatch = Stopwatch.StartNew();
                for (int i = 0; i < array.Length; i++)
                {
                    int testObject = array[i];
                    if (testObject == -1)
                    {
                        Assert.Fail();
                    }
                }

                forArrayStopwatch.Stop();

                Console.WriteLine(
                    $"{count} repetitions - foreach:\tlist {foreachStopwatch.ElapsedMilliseconds}ms\t-\tarray\t{foreachArrayStopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine(
                    $"{count} repetitions - for:\t\tlist {forStopwatch.ElapsedMilliseconds}ms\t-\tarray\t{forArrayStopwatch.ElapsedMilliseconds}ms");

                Assert.IsTrue(foreachStopwatch.ElapsedMilliseconds > forStopwatch.ElapsedMilliseconds);

                //its not as fast

                double expected = forStopwatch.ElapsedMilliseconds + (forStopwatch.ElapsedMilliseconds * 1);
                Assert.IsFalse(foreachStopwatch.ElapsedMilliseconds > expected,
                    $"Did not expect the value to be less than {expected}");

                //but still a bit faster
                expected = forStopwatch.ElapsedMilliseconds + (forStopwatch.ElapsedMilliseconds * 0.2);
                Assert.IsTrue(foreachStopwatch.ElapsedMilliseconds > expected,
                    $"Expected the value to be less than {expected}");
            //}
        }




    }


}
#endif
