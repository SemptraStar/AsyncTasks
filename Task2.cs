using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace WorkTimeTest
{
    class Program
    {
        static void Main(string[] args)
        {   
            var foundation = new TaskWork("Foundation", 5);

            var wall_1 = new TaskWork("Wall 1", 3);
            var wall_2 = new TaskWork("Wall 2", 3);
            var wall_3 = new TaskWork("Wall 3", 3);
            var wall_4 = new TaskWork("Wall 4", 3);

            foundation.AddNextTaskWork(wall_1);
            foundation.AddNextTaskWork(wall_2);
            foundation.AddNextTaskWork(wall_3);
            foundation.AddNextTaskWork(wall_4);

            var door = new TaskWork("Door", 2);

            wall_1.AddNextTaskWork(door);

            var window_1 = new TaskWork("Window 1", 1);
            var window_2 = new TaskWork("Window 2", 1);
            var window_3 = new TaskWork("Window 3", 1);

            wall_2.AddNextTaskWork(window_1);
            wall_3.AddNextTaskWork(window_2);
            wall_3.AddNextTaskWork(window_3);

            var roof = new TaskWork("Roof", 4);
         
            door.AddNextTaskWork(roof);
            window_1.AddNextTaskWork(roof);
            window_2.AddNextTaskWork(roof);
            window_3.AddNextTaskWork(roof);
            wall_4.AddNextTaskWork(roof);

            roof.EnableWorkEndNotify();
            

            Stopwatch sw = new Stopwatch();

            sw.Start();

            foundation.TriggerWork();
            roof.WorkEnded.WaitOne();

            sw.Stop();

            Console.WriteLine();
            Console.WriteLine("TOTAL TIME = " + (double)sw.ElapsedMilliseconds / 1000);
        }
    }

    class TaskWork
    {
        public readonly string Name;
        public readonly int TaskTime;

        public Task Work;
        public bool WorkStarted;

        public AutoResetEvent WorkEnded;

        public List<TaskWork> TasksToWaitFor;
        public List<TaskWork> NextTasks;

        public TaskWork()
        {
            TasksToWaitFor = new List<TaskWork>();
            NextTasks = new List<TaskWork>();
            WorkStarted = false;
        }
        public TaskWork(string name, int taskTime) : this()
        {
            Name = name;
            TaskTime = taskTime;
        }

        public void AddNextTaskWork(TaskWork nextTaskWork)
        {
            NextTasks.Add(nextTaskWork);
            nextTaskWork.TasksToWaitFor.Add(this);
        }

        public void EnableWorkEndNotify()
        {
            WorkEnded = new AutoResetEvent(false);
        }

        public void TriggerWork()
        {
            var taskWorksToWaitFor = TasksToWaitFor.Select(taskWork => taskWork.Work);

            if (taskWorksToWaitFor.Contains(null))
                return;

            WorkStarted = true;

            Task.WhenAll(taskWorksToWaitFor).ContinueWith(_ => StartWork());     
        }

        public void StartWork()
        {
            Work = Task.Run(() =>
            {
                int timeLeft = TaskTime;

                Console.WriteLine("Task {0} started. Time left: {1} hours", Name, TaskTime);             

                while(timeLeft-- > 0)
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("Task {0} time left: {1}", Name, timeLeft);
                }                            
            }).ContinueWith(_ => 
            {
                WorkEnded?.Set();

                NextTasks.ForEach(taskWork => 
                {
                    if (!taskWork.WorkStarted)                                     
                        taskWork.TriggerWork();                                        
                });
            });          
        }
    }
}
