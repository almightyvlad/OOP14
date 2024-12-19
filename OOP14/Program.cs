using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;

namespace OOP14
{
    public class Program
    {
        private static bool isPaused = false;
        private static object lockVar = new object();
        private static bool isEven = false;

        private static System.Timers.Timer timer;
        private static int count = 0;
        private static bool isPrime(int number)
        {
            if (number < 1) return false;
            for (int i = 2; i <= Math.Sqrt(number); i++)
            {
                if (number % i == 0) return false;
            }
            return true;
        }
        private static void WritePrimesToFile(string filePath, int n)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                for (int i = 1; i <= n; i++)
                {
                    lock (lockVar)
                    {
                        while (isPaused)
                        {
                            Monitor.Wait(lockVar);
                        }
                    }

                    if (isPrime(i))
                    {
                        Console.WriteLine(i);
                        writer.WriteLine(i);
                    }

                    Thread.Sleep(100);
                }
            }
        }
        private static void OddFunc(int n, List<int> oddNumbers)
        {
            for (int i = 1; i < n; i += 2)
            {
                lock (lockVar)
                {
                    while (isEven)
                    {
                        Monitor.Wait(lockVar);
                    }

                    Console.WriteLine($"Нечетное: {i}");
                    oddNumbers.Add(i);

                    isEven = true;
                    Monitor.Pulse(lockVar);
                }

            }
        }
        private static void EvenFunc(int n, List<int> evenNumbers)
        {
            for (int i = 0; i < n; i += 2)
            {
                lock (lockVar)
                {
                    while (!isEven)
                    {
                        Monitor.Wait(lockVar);
                    }

                    Console.WriteLine($"Четное: {i}");
                    evenNumbers.Add(i);

                    isEven = false;
                    Monitor.Pulse(lockVar);
                }

            }
        }
        private static void OddFuncSecond(int n, List<int> oddNumbers)
        {
            lock (lockVar)
            {
                while (!isEven)
                {
                    Monitor.Wait(lockVar);
                }
            }

            for (int i = 1; i < n; i += 2)
            {
                Console.WriteLine($"Нечетное: {i}");
                oddNumbers.Add(i);
            }

        }
        private static void EvenFuncSecond(int n, List<int> evenNumbers)
        {
            for (int i = 0; i < n; i += 2)
            {
                Console.WriteLine($"Четное: {i}");
                evenNumbers.Add(i);
            }

            lock (lockVar)
            {
                isEven = true;
                Monitor.Pulse(lockVar);
            }
        }
        private static void WriteToFile(string filePath, List<int> numbers)
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                foreach (int number in numbers)
                {
                    writer.WriteLine(number);
                }
            }
        }
        static void Main(string[] args)
        {
            // 1
            string filePath = "C:\\Users\\vlad\\source\\repos\\OOP14\\file.txt";

            using (StreamWriter write = new StreamWriter(filePath))
            {
                foreach (var process in Process.GetProcesses())
                {
                    try
                    {
                        write.WriteLine($"{process.Id}, " +
                            $"{process.ProcessName}, " +
                            $"{process.BasePriority.ToString()}, " +
                            $"{process.StartTime}, " +
                            $"{process.TotalProcessorTime}");
                    }
                    catch (Exception ex)
                    {
                        write.WriteLine($"Error: {ex.Message}");
                    }
                }
            }

            using (StreamReader read = new StreamReader(filePath))
            {
                string line;

                while ((line = read.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
            }
            Console.WriteLine();

            // 2

            AppDomain domain = AppDomain.CurrentDomain;

            Console.WriteLine($"Name: {domain.FriendlyName}");
            Console.WriteLine($"Base dir: {domain.BaseDirectory}");

            Console.WriteLine("Assemblies: ");
            foreach (var assembly in domain.GetAssemblies())
            {
                Console.WriteLine(assembly.FullName);
            }

            Console.WriteLine();

            AppDomain newDomain = AppDomain.CreateDomain("NewDomain");

            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            newDomain.Load(AssemblyName.GetAssemblyName(assemblyPath));

            foreach (var assembly in newDomain.GetAssemblies())
            {
                Console.WriteLine(assembly.FullName);
            }

            AppDomain.Unload(newDomain);

            Console.WriteLine();

            // 3
            string fileThreadPath = "C:\\Users\\vlad\\source\\repos\\OOP14\\fileThread.txt";

            Thread newThread = new Thread(() => WritePrimesToFile(fileThreadPath, 100));

            newThread.Start();

            Console.WriteLine($"Status: {newThread.GetApartmentState()} , Name: {newThread.Name}, Priority: {newThread.Priority}, ID: {newThread.ManagedThreadId}");

            Console.WriteLine("[P] Приостановить");
            Console.WriteLine("[R] Возобновить");
            Console.WriteLine("[S] Завершить");
            Console.WriteLine("[Q] Выйти");

            while (newThread.IsAlive)
            {
                char command = Console.ReadKey(true).KeyChar;

                switch (command)
                {
                    case 'P':
                    case 'p':
                        lock (lockVar)
                        {
                            isPaused = true;
                        }
                        break;
                    case 'R':
                    case 'r':
                        lock (lockVar)
                        {
                            isPaused = false;
                            Monitor.PulseAll(lockVar);
                        }
                        break;
                    case 'S':
                    case 's':
                        newThread.Interrupt();
                        break;
                    case 'Q':
                    case 'q':
                        newThread.Join();
                        break;
                    default:
                        return;
                }
            }

            // 4
            var oddNumbers = new List<int>();
            var evenNumbers = new List<int>();

            int n = 100;

            Thread evenThread = new Thread(() => EvenFuncSecond(n, evenNumbers));
            Thread oddThread = new Thread(() => OddFuncSecond(n, oddNumbers));

            evenThread.Priority = ThreadPriority.Highest;
            oddThread.Priority = ThreadPriority.Lowest;

            //evenThread.Start();
            //oddThread.Start();

            //evenThread.Join();
            //oddThread.Join();

            WriteToFile("C:\\Users\\vlad\\source\\repos\\OOP14\\fourth.txt", oddNumbers);
            WriteToFile("C:\\Users\\vlad\\source\\repos\\OOP14\\fourth.txt", evenNumbers);

            // 5 

            timer = new System.Timers.Timer(1000);

            timer.Elapsed += OnTimerElapsed;

            timer.Start();

            Console.ReadLine();
        }

        private static void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            count++;
            Console.WriteLine("Таймер сработал");

            if (count >= 5)
            {
                timer.Stop();
                Console.WriteLine("Таймер остановлен");
            }
        }


    }
}
