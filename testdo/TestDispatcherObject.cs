using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Threading;

namespace testdo
{
    [TestClass]
    public class TestDispatcherObject
    {
        private class MyDispatcherObject : DispatcherObject
        {
        }

        [TestMethod]
        public void TestCheckAccessOnSameThread()
        {
            DispatcherObject d = new MyDispatcherObject();
            Assert.IsTrue(d.CheckAccess());
        }

        [TestMethod]
        public void TestVerifyAccessOnSameThread()
        {
            DispatcherObject d = new MyDispatcherObject();
            d.VerifyAccess();
        }

        [TestMethod]
        public void TestCheckAccessOnDifferentThread()
        {
            var dispatcherObject = CreateDispatcherObjectOnThread();

            Assert.IsFalse(dispatcherObject.CheckAccess());
        }

        [TestMethod]
        public void TestVerifyAccessOnDifferentThread()
        {
            var dispatcherObject = CreateDispatcherObjectOnThread();

            Assert.ThrowsException<InvalidOperationException>(() => dispatcherObject.VerifyAccess());
        }

        [TestMethod]
        public void TestDispatcherOnSameThread()
        {
            var d1 = new MyDispatcherObject();
            var d2 = new MyDispatcherObject();

            Assert.IsNotNull(d1);
            Assert.IsNotNull(d2);
            Assert.AreSame(d1.Dispatcher, d2.Dispatcher);
        }

        [TestMethod]
        public void TestDispatcherOnDifferentThreads()
        {
            var d1 = CreateDispatcherObjectOnThread();
            var d2 = CreateDispatcherObjectOnThread();

            Assert.IsNotNull(d1);
            Assert.IsNotNull(d2);
            Assert.AreNotSame(d1.Dispatcher, d2.Dispatcher);
        }

        private static DispatcherObject CreateDispatcherObjectOnThread()
        {
            DispatcherObject dispatcherObject = null;
            var thread = new Thread(() => dispatcherObject = new MyDispatcherObject());
            thread.Start();
            thread.Join();

            return dispatcherObject;
        }
    }
}