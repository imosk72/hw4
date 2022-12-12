using System;
using System.Collections.Generic;
using System.Threading;

namespace CustomThreadPool
{ 
    public class MyThreadPool: IThreadPool
    {
        private readonly Queue<Action> mainQueue = new Queue<Action>();
        private long processedTasksCount;
        
        public MyThreadPool()
        {
            processedTasksCount = 0;
            
            var threadsCount = Environment.ProcessorCount * 4;
            for (var i = 0; i < threadsCount; i++)
            {
                var thread = new Thread(Worker) {IsBackground = true};
                thread.Start();
            }
        }

        private void Worker()
        {
            while (true)
            {
                Action currentTask = default;
                lock (mainQueue)
                {
                    if (!mainQueue.TryDequeue(out currentTask))
                    {
                        Monitor.Wait(mainQueue);
                        continue;
                    }
                }

                if (currentTask != default)
                {
                    currentTask.Invoke();
                    Interlocked.Increment(ref processedTasksCount);
                }
            }
        }

        public void EnqueueAction(Action action)
        {
            lock (mainQueue)
            {
                mainQueue.Enqueue(action);
                Monitor.Pulse(mainQueue);
            }
        }

        public long GetTasksProcessedCount()
        {
            return processedTasksCount;
        }
    }
}