using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace PrimeNumberCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            BigInteger numberOfPrimesAsync = 0;

            sw.Start();

            // Async (for 1000000)
            // Result = 78568
            // Time = 2,817
            
            CalculateNumberOfPrimesAsync(1000000, numberOfPrimesAsync, current => numberOfPrimesAsync += current);

            sw.Stop();

            Console.WriteLine("Async.");
            Console.WriteLine("Result = " + numberOfPrimesAsync);
            Console.WriteLine("Time = " + (double)sw.ElapsedMilliseconds / 1000);


            sw.Reset();
            sw.Start();

            // Parallel (for 1000000)
            // Result = 78568
            // Time = 2,54

            var numberOfPrimesParallel = CalculateNumberOfPrimesParallel(1000000);

            sw.Stop();

            Console.WriteLine("Parallel.");
            Console.WriteLine("Result = " + numberOfPrimesParallel);
            Console.WriteLine("Time = " + (double)sw.ElapsedMilliseconds / 1000);

        }

        // Async

        static async void CalculateNumberOfPrimesAsync(BigInteger bound, BigInteger numberOfPrimes, Action<BigInteger> addNumberOfPrimes)
        {
            var calculationTasks = new List<Task<BigInteger>>();
            BigInteger part = bound / 8;

            for (int i = 0; i < 8; i++)
            {
                calculationTasks.Add(CalculatePartNumberOfPrimes(part * i, part * i + part));
            }

            foreach (var task in calculationTasks)
            {
                addNumberOfPrimes(task.Result);
            }
        }

        static async Task<BigInteger> CalculatePartNumberOfPrimes(BigInteger from, BigInteger to)
        {
            return await Task.Run(() =>
            {
                BigInteger numberOfPrimes = 0;

                for (BigInteger number = from; number < to; number++)
                {
                    if (IsPrime(number))
                    {
                        numberOfPrimes++;
                    }
                }

                return numberOfPrimes;
            });
        }

        // Parallel

        static BigInteger CalculateNumberOfPrimesParallel(BigInteger bound)
        {
            return Range(0, bound).AsParallel().Sum(number => IsPrime(number) ? 1 : 0);
        }       

        static IEnumerable<BigInteger> Range(BigInteger min, BigInteger max)
        {
            for (BigInteger num = min; num <= max; num++)
            {
                yield return num;
            }
        }
           

        static bool IsPrime(BigInteger number)
        {
            if (number >= 0 && number <= 2)
                return true;

            for (int i = 2; i <= Math.Exp(BigInteger.Log(number) / 2); i++)
            {
                if (number % i == 0)
                    return false;
            }

            return true;
        }
    }
}
