using System;
using System.Threading;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace testdo
{
    [TestClass]
    public class TestDispatcher
    {
        [TestMethod]
        public void CurrentDispatcher()
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

            Assert.IsNotNull(dispatcher);
        }

        [TestMethod]
        public void CurrentIsSameAsThread()
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            Dispatcher threadDispatcher = Dispatcher.FromThread(Thread.CurrentThread);

            Assert.AreSame(dispatcher, threadDispatcher);
        }

        [TestMethod]
        public void CurrentIsDifferentToOtherThread()
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            Dispatcher threadDispatcher = CreateDispatcherOnOtherThread();

            Assert.IsNotNull(dispatcher);
            Assert.IsNotNull(threadDispatcher);
            Assert.AreNotSame(dispatcher, threadDispatcher);
        }

        [TestMethod]
        public void DispatcherThreadProperty()
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

            Assert.IsNotNull(dispatcher.Thread);
            Assert.AreSame(dispatcher.Thread, Thread.CurrentThread);
        }

        [TestMethod]
        public void HasShutdownStartedFinishedProperties()
        {
            Dispatcher dispatcher = null;
            var dispatcherAvailableEvent = new AutoResetEvent(false);

            var thread = new Thread(() =>
            {
                dispatcher = Dispatcher.CurrentDispatcher; // Creates dispatcher for thread
                dispatcherAvailableEvent.Set();
                Dispatcher.Run();
            });
            thread.Start();

            dispatcherAvailableEvent.WaitOne();

            Assert.IsFalse(dispatcher.HasShutdownStarted);
            Assert.IsFalse(dispatcher.HasShutdownFinished);
            dispatcher.InvokeShutdown();
            Assert.IsTrue(dispatcher.HasShutdownStarted);

            thread.Join();
            Assert.IsTrue(dispatcher.HasShutdownFinished);
        }

        [TestMethod]
        public void InvokeThread()
        {
            Dispatcher dispatcher = null;
            bool invoked = false;
            var dispatcherAvailableEvent = new AutoResetEvent(false);

            var thread = new Thread(() =>
            {
                dispatcher = Dispatcher.CurrentDispatcher; // Creates dispatcher for thread
                dispatcherAvailableEvent.Set();
                Dispatcher.Run();
            });
            thread.Start();

            dispatcherAvailableEvent.WaitOne();

            dispatcher.Invoke(() =>
            {
                invoked = true;
                Assert.AreSame(thread, Thread.CurrentThread);
            });

            Assert.IsTrue(invoked);

            dispatcher.InvokeShutdown();
            thread.Join();
        }

        [TestMethod]
        public void Yield()
        {
            Dispatcher dispatcher = null;

            var dispatcherAvailableEvent = new AutoResetEvent(false);

            var thread = new Thread(() =>
            {
                dispatcher = Dispatcher.CurrentDispatcher; // Creates dispatcher for thread
                dispatcherAvailableEvent.Set();
                Dispatcher.Run();
            });
            thread.Start();
            dispatcherAvailableEvent.WaitOne();

            var stop = new AutoResetEvent(false); // Don't think I need this, Yield breaks the loop
            dispatcher.BeginInvoke(new Action(async () =>
            {
                while (!stop.WaitOne(10))
                {
                    await Dispatcher.Yield();
                }
            }));

            bool flag = false;
            dispatcher.Invoke(() =>
            {
                flag = true;
            });

            Assert.IsTrue(flag);

            stop.Set();
            dispatcher.InvokeShutdown();
            thread.Join();
        }

        private static Dispatcher CreateDispatcherOnOtherThread()
        {
            Dispatcher dispatcher = null;

            var thread = new Thread(() =>
            {
                dispatcher = Dispatcher.CurrentDispatcher;
            });

            thread.Start();
            thread.Join();

            return dispatcher;
        }
    }
}