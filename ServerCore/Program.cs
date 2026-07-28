using System;
using System.Threading;
using System.Threading.Tasks;
//=========================================
//하드웨어 최적화
//=========================================
namespace ServerCore
{
    //메모리 배리어
    //1. 코드 재배치 억제
    //2. 가시성

    //Full Memory Barrier: store, load 모두 억제
    //Store Memory Barrier: store 억제, load 허용
    //Load Memory Barrier: load 억제, store 허용
    class Program
    {
        static int x = 0;
        static int y = 0;
        static int r1 = 0;
        static int r2 = 0;

        static void Thread1()
        {
            y = 1;//store y

            //--------------------------------
            Thread.MemoryBarrier();//메모리 배리어

            r1 = x;//load x
        }

        static void Thread2()
        {
            x = 1;//store x

            //--------------------------------
            Thread.MemoryBarrier();//메모리 배리어

            r2 = y;//load y
        }

        static void Main(string[] args)
        {
            int count = 0;
            while (true)
            {
                count++;
                x = y = r1 = r2 = 0;
                Task t1=new Task(Thread1);
                Task t2=new Task(Thread2);
                t1.Start();
                t2.Start();

                Task.WaitAll(t1, t2);

                if (r1 == 0 && r1 == 0)
                    break;
            }
            Console.WriteLine($"{count}번 만에 빠져나옴");
        }
    }
}

