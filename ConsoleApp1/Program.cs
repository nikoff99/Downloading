using System;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleApp1 {
    internal class Program {
        static Semaphore semaphore = new Semaphore(3, 3, "DownloaderSemaphore");

        static CancellationTokenSource cts = new CancellationTokenSource();

        static CancellationToken token = cts.Token;

        static List<Thread> thread_List = new List<Thread>();

        static int[] progresses = new int[8];

        static int active = 0;
        static int waiting = 0;
        static int completed = 0;

        static object lockObject = new object();

        static void AddThreads(Thread thread) {
            thread_List.Add(thread);
        }

        static void Draw() {
            lock (lockObject) {
                Console.Clear();

                Console.WriteLine(
                    "========== VIDEO DOWNLOADER =========="
                );

                Console.WriteLine();

                for (int i = 0; i < progresses.Length; i++) {
                    int progress = progresses[i];

                    int barLength = 25;

                    int filled =
                        progress * barLength / 100;

                    string bar =
                        new string('█', filled) +
                        new string(
                            '░',
                            barLength - filled
                        );

                    Console.WriteLine(
                        $"Download {i + 1}: [{bar}] {progress}%"
                    );
                }

                Console.WriteLine();

                Console.WriteLine("--------------------------------------");

                Console.WriteLine(
                    $"Active    : {active}"
                );

                Console.WriteLine(
                    $"Waiting   : {waiting}"
                );

                Console.WriteLine(
                    $"Completed : {completed}"
                );

                Console.WriteLine("--------------------------------------");

                Console.WriteLine();
                Console.WriteLine("ESC -> Cancel All");
            }
        }
        static void Download(object obj) {
            int id = (int)obj;

            lock (lockObject) {
                waiting++;
            }

            semaphore.WaitOne();

            lock (lockObject) {
                waiting--;
                active++;
            }

            try {
                for (int i = 0; i <= 100; i++) {
                    if (token.IsCancellationRequested) {
                        return;
                    }

                    progresses[id - 1] = i;

                    Draw();

                    Thread.Sleep(100);
                }

                lock (lockObject) {
                    active--;
                    completed++;
                }

                Draw();
            }
            finally {
                semaphore.Release();
            }
        }

        static void Main(string[] args) {
            for (int i = 0; i < 8; i++) {
                Thread thread =  new Thread(Download);

                AddThreads(thread);
            }

            for (int i = 0; i < thread_List.Count; i++) {
                thread_List[i].Start(i + 1);
            }

            while (!token.IsCancellationRequested) {
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Escape || completed == thread_List.Count()) {
                    cts.Cancel();
                    break;
                }
                Thread.Sleep(100);
            }
        }
    }
}