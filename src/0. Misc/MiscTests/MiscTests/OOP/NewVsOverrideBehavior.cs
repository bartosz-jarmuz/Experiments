using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MiscTests.OOP
{
    [TestFixture]
    public class NewVsOverrideBehavior
    {
        public interface IAnimal
        {
            string GetDetails();
            string GetDescription();
        }

        public class Animal : IAnimal
        {
            public virtual string GetDetails()
            {
                return "[No details]";
            }

            public string GetDescription()
            {
                return "A live creature. " + GetDetails();
            }
        }

        public class Dog : Animal
        {
            public override string GetDetails()
            {
                return "Has 4 legs";
            }
        }

        public class Snake : Animal
        {
            public new string GetDetails()
            {
                return "Has no legs";
            }
        }

        [Test]
        public void TestBehaviour_DirectMethodCall()
        {
            //base class
            Animal baseAnimal = new Animal();
            Assert.AreEqual("[No details]", baseAnimal.GetDetails());

            //test 'override' approach
            Dog dog = new Dog();
            Assert.AreEqual("Has 4 legs", dog.GetDetails());

            //does not matter which 'type' we call, the Dog will always behave like the Dog we defined
            Animal dogAsAnimal = new Dog();
            Assert.AreEqual("Has 4 legs", dogAsAnimal.GetDetails());
            IAnimal dogAsInterface = new Dog();
            Assert.AreEqual("Has 4 legs", dogAsInterface.GetDetails());

            //test 'new' approach
            Snake snake = new Snake();
            Assert.AreEqual("Has no legs", snake.GetDetails());

            //when the snake is called as an Animal type, it does not access the hiding method!
            Animal snakeAsAnimal = new Snake();
            IAnimal snakeAsInterface = new Snake();
            Assert.AreEqual("[No details]", snakeAsAnimal.GetDetails());
            Assert.AreEqual("[No details]", snakeAsInterface.GetDetails());
        }

        [Test]
        public void TestBehaviour_IndirectMethodCall()
        {
            //base class
            Animal baseAnimal = new Animal();
            Assert.AreEqual("A live creature. [No details]", baseAnimal.GetDescription());

            //test 'override' approach
            Dog dog = new Dog();
            Assert.AreEqual("A live creature. Has 4 legs", dog.GetDescription());

            //does not matter which 'type' we call, the Dog will always behave like the Dog we defined
            Animal dogAsAnimal = new Dog();
            IAnimal dogAsInterface = new Dog();
            Assert.AreEqual("A live creature. Has 4 legs", dogAsAnimal.GetDescription());
            Assert.AreEqual("A live creature. Has 4 legs", dogAsInterface.GetDescription());


            //test 'new' approach
            //regardless which Type we call (Snake or Animal) the snake never has access to the hiding method when it is called from a base class method!
            Snake snake = new Snake();
            Assert.AreEqual("A live creature. [No details]", snake.GetDescription());

            Animal snakeAsAnimal = new Snake();
            IAnimal snakeAsInterface = new Snake();
            Assert.AreEqual("A live creature. [No details]", snakeAsAnimal.GetDescription());
            Assert.AreEqual("A live creature. [No details]", snakeAsInterface.GetDescription());
        }
    }


}
