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
            wall_1.TasksToWaitFor.Add(foundation);
            foundation.NextTasks.Add(wall_1);

            var wall_2 = new TaskWork("Wall 2", 3);
            wall_2.TasksToWaitFor.Add(foundation);
            foundation.NextTasks.Add(wall_2);

            var wall_3 = new TaskWork("Wall 3", 3);
            wall_1.TasksToWaitFor.Add(foundation);
            foundation.NextTasks.Add(wall_3);

            var wall_4 = new TaskWork("Wall 4", 3);
            wall_1.TasksToWaitFor.Add(foundation);
            foundation.NextTasks.Add(wall_4);

            var door = new TaskWork("Door", 2);
            door.TasksToWaitFor.Add(wall_1);
            wall_1.NextTasks.Add(door);

            var window_1 = new TaskWork("Window 1", 1);
            window_1.TasksToWaitFor.Add(wall_2);
            wall_2.NextTasks.Add(window_1);

            var window_2 = new TaskWork("Window 2", 1);
            window_2.TasksToWaitFor.Add(wall_3);
            wall_3.NextTasks.Add(window_2);

            var window_3 = new TaskWork("Window 3", 1);
            window_3.TasksToWaitFor.Add(wall_3);
            wall_3.NextTasks.Add(window_3);

            var roof = new TaskWork("Roof", 4);
            roof.TasksToWaitFor.Add(window_1);
            roof.TasksToWaitFor.Add(window_2);
            roof.TasksToWaitFor.Add(window_3);
            roof.TasksToWaitFor.Add(door);
            window_1.NextTasks.Add(roof);
            window_2.NextTasks.Add(roof);
            window_3.NextTasks.Add(roof);
            door.NextTasks.Add(roof);
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

    class BuildObject
    {
        public string ObjectName;
        public int BuildTime;

        public bool IsBuildingStarted;

        public ManualResetEvent BuildFinishedEvent;

        public List<BuildObject> ObjectsToWaitFor;
        public List<BuildObject> NextObjects = new List<BuildObject>();

        public BuildObject(string objectName, int buildTime, List<BuildObject> objectsToWaitFor = null)
        {
            ObjectName = objectName;
            BuildTime = buildTime;
            IsBuildingStarted = false;
            ObjectsToWaitFor = objectsToWaitFor ?? new List<BuildObject>();
            BuildFinishedEvent = new ManualResetEvent(false);
        }

        public void StartBuilding()
        {
            IsBuildingStarted = true;

            foreach (var waitObject in ObjectsToWaitFor)
            {
                if (!waitObject.BuildFinishedEvent.WaitOne())
                {
                    Console.WriteLine("Wait for {0} to build...", waitObject.ObjectName);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Start building " + ObjectName);

            var t = Task.Run(() => Build());
            
        }

        private void Build()
        {
            for (int i = 0; i < BuildTime; i++)
            {
                Console.WriteLine("Building {0}. {1} hours left...", ObjectName, BuildTime - i);
                Thread.Sleep(1000);
            }

            Console.WriteLine("Bulding {0} was finished!", ObjectName);

            BuildFinishedEvent.Set();

            foreach (var nextObject in NextObjects)
            {
                if (!nextObject.IsBuildingStarted)
                {
                    nextObject.StartBuilding();
                }
            }
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

        public void EnableWorkEndNotify()
        {
            WorkEnded = new AutoResetEvent(false);
        }

        public void TriggerWork()
        {
            if (WorkStarted)
                return;
            else
                WorkStarted = true;

            Task.WhenAll(TasksToWaitFor.Select(taskWork => taskWork.Work))
                .ContinueWith(_ => StartWork());              
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

                NextTasks.ForEach(taskWork => taskWork.TriggerWork());
            });          
        }
    }
}
