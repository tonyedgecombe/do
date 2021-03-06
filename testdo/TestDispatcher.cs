﻿using System;
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
            Dispatcher threadDispatcher = CreateDispatcherOnNewThread();

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
            Dispatcher dispatcher = CreateDispatcherOnNewThread();

            Assert.IsFalse(dispatcher.HasShutdownStarted);
            Assert.IsFalse(dispatcher.HasShutdownFinished);
            dispatcher.InvokeShutdown();
            Assert.IsTrue(dispatcher.HasShutdownStarted);

            dispatcher.Thread.Join();
            Assert.IsTrue(dispatcher.HasShutdownFinished);
        }

        [TestMethod]
        public void InvokeThread()
        {
            Dispatcher dispatcher = CreateDispatcherOnNewThread();
            bool invoked = false;

            dispatcher.Invoke(() =>
            {
                invoked = true;
                Assert.AreSame(dispatcher.Thread, Thread.CurrentThread);
            });

            Assert.IsTrue(invoked);

            dispatcher.InvokeShutdown();
            dispatcher.Thread.Join();
        }

        [TestMethod]
        public void Yield()
        {
            Dispatcher dispatcher = CreateDispatcherOnNewThread();

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
            dispatcher.Thread.Join();
        }


        [TestMethod]
        public void Frame()
        {
            Dispatcher dispatcher = null;
            DispatcherFrame frame = null;
            var dispatcherAvailableEvent = new AutoResetEvent(false);

            var thread = new Thread(() =>
            {
                dispatcher = Dispatcher.CurrentDispatcher; // Creates dispatcher for thread
                frame = new DispatcherFrame(false);

                dispatcherAvailableEvent.Set();
                Dispatcher.PushFrame(frame);
            });
            thread.Start();
            dispatcherAvailableEvent.WaitOne();

            dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                frame.Continue = false;
            }));

            dispatcher.Thread.Join();
        }

        [TestMethod]
        public void NestedFrames()
        {
            Dispatcher dispatcher = null;
            DispatcherFrame frame1 = null;
            DispatcherFrame frame2 = null;
            var dispatcherAvailableEvent = new AutoResetEvent(false);
            var frame2AvailableEvent = new AutoResetEvent(false);

            var thread = new Thread(() =>
            {
                dispatcher = Dispatcher.CurrentDispatcher; // Creates dispatcher for thread
                frame1 = new DispatcherFrame(false);

                dispatcherAvailableEvent.Set();
                Dispatcher.PushFrame(frame1);
            });
            thread.Start();
            dispatcherAvailableEvent.WaitOne();

            var dispatcherOperation = dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                frame2 = new DispatcherFrame(false);
                frame2AvailableEvent.Set();
                Dispatcher.PushFrame(frame2);
            }));

            frame2AvailableEvent.WaitOne();

            frame2.Continue = false;
            dispatcherOperation.Wait();

            frame1.Continue = false;
            dispatcher.Thread.Join();
        }


        private Dispatcher CreateDispatcherOnNewThread()
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

            return dispatcher;
        }
    }
}