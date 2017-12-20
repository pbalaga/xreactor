#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Agdur;
using xReactor.Common;

namespace xReactor.Tests.Benchmarks
{
    class Program
    {
        public const int DefaultNumTimes = 1000;
        public const int DefaultNumInstances = 1000;
        public const int SmallNumInstances = 100;

        static void Main(string[] args)
        {
            Console.Write("Retrieving benchmarks...");
            BenchmarkData[] benchmarks = GetBenchmarks().ToArray();
            Console.WriteLine(" {0} found", benchmarks.Length);

            Console.WriteLine("This may take a long time to finish. ");
            Console.WriteLine("Got bored? Well, there's one thing you can do:");
            Console.WriteLine("Optimize!");

            ExecuteBenchmarks(benchmarks);

            Console.WriteLine();
            Console.WriteLine("Done!");
            Console.ReadKey();
        }

        static IEnumerable<BenchmarkData> GetBenchmarks()
        {
            foreach (var type in Assembly.GetEntryAssembly().GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Static |
                                                       BindingFlags.Public |
                                                       BindingFlags.NonPublic))
                {
                    var attr = method.GetCustomAttribute<BenchmarkAttribute>();
                    if (attr != null)
                    {
                        yield return new BenchmarkData()
                        {
                            Title = attr.Title ?? method.Name,
                            WorkToDo = () => method.Invoke(null, null)
                        };
                    }
                }
            }
        }

        static void ExecuteBenchmarks(IEnumerable<BenchmarkData> benchmarks)
        {
            foreach (var benchmark in benchmarks)
            {
                try
                {
                    if (benchmark == null)
                        continue;

                    Console.WriteLine();
                    Console.WriteLine("-- {0} -- ", benchmark.Title);
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Benchmark.This(benchmark.WorkToDo).
                        Times(DefaultNumTimes).Average().InMilliseconds().
                        ToConsole().AsFormattedString();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Error occured in benchmark '{0}' with message: {1}\nInner:{2}",
                        benchmark.Title,
                        e.Message,
                        e.InnerException == null ? "None" : e.InnerException.Message);
                }
                finally
                {
                    Console.ResetColor();
                }
            }
        }
    }
}
