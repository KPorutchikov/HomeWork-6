using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class Program
{
    static string exePath = AppDomain.CurrentDomain.BaseDirectory;
    static string file_1 = exePath + "Crime_and_Punishment.txt";
    static string file_2 = exePath + "Demons.txt";
    static string file_3 = exePath + "Idiot.txt";

    private static void Main()
    {
        Stopwatch stopwatch = new Stopwatch();

        //-------------------------------------------------------------------------------------------
        // I - Часть
        Console.WriteLine($"Асинхронный старт (основной поток N: {Thread.CurrentThread.ManagedThreadId})\n");
        
        stopwatch.Start();

        var a_file_res1 = FileSpaceAsync(file_1);
        var a_file_res2 = FileSpaceAsync(file_2);
        var a_file_res3 = FileSpaceAsync(file_3);

        Console.WriteLine($"\nФайл - {file_1},  количество пробелов: {a_file_res1.Result}");
        Console.WriteLine($"Файл - {file_2},  количество пробелов: {a_file_res2.Result}");
        Console.WriteLine($"Файл - {file_3},  количество пробелов: {a_file_res3.Result}\n");

        stopwatch.Stop();

        Console.WriteLine($"Завершено  [всего пробелов : {a_file_res1.Result + a_file_res2.Result + a_file_res3.Result}]  (основной поток N: {Thread.CurrentThread.ManagedThreadId})");
        Console.WriteLine($"Асинхронным методом было затрачено времени : {stopwatch.ElapsedMilliseconds} миллисекунд\n\n");


        //-------------------------------------------------------------------------------------------
        // II - Часть
        Console.WriteLine($"Синхронный старт\n");
        stopwatch.Reset();
        stopwatch.Start();

        int file_res1 = FileSpace(file_1);
        int file_res2 = FileSpace(file_2);
        int file_res3 = FileSpace(file_3);

        stopwatch.Stop();

        Console.WriteLine($"Файл - {file_1},  количество пробелов: {file_res1}");
        Console.WriteLine($"Файл - {file_2},  количество пробелов: {file_res2}");
        Console.WriteLine($"Файл - {file_3},  количество пробелов: {file_res3}\n");

        Console.WriteLine($"Завершено");
        Console.WriteLine($"Синхронным методом было затрачено времени : {stopwatch.ElapsedMilliseconds} миллисекунд\n\n");


        //-------------------------------------------------------------------------------------------
        // III - Часть
        Console.WriteLine($"Асинхронный старт, сканирование и обработка каталога");

        string[] dirs = MyGetFiles(exePath, "*.txt");

        if (dirs.Length > 0)
        {
            int TotalRes = 0;
            Task[] tasks = new Task[dirs.Length];
            ConcurrentDictionary<int, string> FileList = [];

            for (int i = 0; i < dirs.Length; i++)
            {
                FileList.TryAdd(i, dirs[i]);
            }

            stopwatch.Reset();
            stopwatch.Start();

            foreach(var f in FileList) 
            {
                tasks[f.Key] = Task.Run(() => FileSpaceAsync(f.Value));
            }

            Task.WaitAll(tasks);
            stopwatch.Stop();

            for (int i = 0; i < tasks.Length;i++)
            {
                TotalRes += ((Task<int>)tasks[i]).Result;
            }

            Console.WriteLine($"\nСканирование завершено, обработано {dirs.Length} файлов, суммарное количество пробелов {TotalRes}.");
            Console.WriteLine($"Было затрачено времени : {stopwatch.ElapsedMilliseconds} миллисекунд\n\n");
        }
        //-------------------------------------------------------------------------------------------

        Console.WriteLine("Press any key to continue ...");
        Console.ReadKey();
    }

    private static async Task<int> FileSpaceAsync(string FileName)
    {
        int Count = 0;
        
        await Task.Run(() =>
            {   if (File.Exists(FileName))
                {
                    Console.Write($"[создан поток N:{Thread.CurrentThread.ManagedThreadId}] ");

                    string str;
                    using (var stream = new FileStream(FileName, FileMode.Open, FileAccess.Read))
                    using (var s = new StreamReader(stream))
                    {
                        str = s.ReadToEnd();
                    }
                    Count += str.Split(' ').Length;
                }
            }
        );
        return Count;
    }

    private static int FileSpace(string FileName)
    {
        int Count = 0;
        if (File.Exists(FileName))
        {   string str;
            using (var stream = new FileStream(FileName, FileMode.Open, FileAccess.Read))
            using (var s = new StreamReader(stream))
            {
                str = s.ReadToEnd();
            }
            Count += str.Split(' ').Length;
        }
        return Count;
    }

    private static string[] MyGetFiles (string Path, string Pattern)
    {
        string[] res = [];

        if (Directory.Exists(Path))
        {
            res = Directory.GetFiles(Path, Pattern);
        }

        return res;
    }
}