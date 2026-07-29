using System;
using System.Threading;
using System.Threading.Tasks;
//=========================================
//Reader Writer Lock
//=========================================
namespace ServerCore
{
    //재귀적 락을 허용할지(yes) writelock가진 상태에서 writelock을 가지는 건 허용/ readlock가진 상태에서 writelock을 가지는 건 불가
    //스핀 락 정책(5000번 -> 양보)
    class Program
    {
        static volatile int count = 0;
        static Lock _lock = new Lock();

        static void Main(string[] args)
        {
            Task t1 = new Task(delegate ()
            {
                for(int i = 0; i < 100000; i++)
                {
                    _lock.WriteLock();
                    count++;
                    _lock.WriteUnlock();
                }
            });
            Task t2 = new Task(delegate ()
            {
                for (int i = 0; i < 100000; i++)
                {
                    _lock.WriteLock();
                    count--;
                    _lock.WriteUnlock();
                }
            });
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);
            Console.WriteLine(count);
        }
    }
}

