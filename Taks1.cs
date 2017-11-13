using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace AsyncTasks
{
    class Program
    {
        static void Main(string[] args)
        {
            Directory.CreateDirectory(@"C:\TestAsyncDir");
            File.WriteAllText(@"C:\TestAsyncDir\test.txt", "Async test file, read this");

            DemoAsync();

            File.Delete(@"C:\TestAsyncDir\test.txt");
            Directory.Delete(@"C:\TestAsyncDir");

            Console.WriteLine("Main ends.");
        }

        static async Task DemoAsync()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            try
            {
                string text = await ReadAllTextAsync(@"C:\TestAsyncDir\test.txt", tokenSource.Token);
                Console.WriteLine(text);

                text = await ReadAllTextTrueAsync(@"C:\TestAsyncDir\test.txt", tokenSource.Token);
                Console.WriteLine(text);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        } 

        static async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken)
        {
            return await File.ReadAllTextAsync(path, cancellationToken);
        }

        static async Task<string> ReadAllTextTrueAsync(string path, CancellationToken cancellationToken)
        {
            return await Task.Run(() => File.ReadAllText(path), cancellationToken);
        }
    }
}
