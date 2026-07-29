using System;
using System.Threading;
using System.Threading.Tasks;
//=========================================
//Event
//=========================================
namespace ServerCore
{
    class Program
    {
        static int _num = 0;
        static Mutex _lock = new Mutex();//커널동기화 객체여서 느림
        //Mutex는 잠군 횟수,잠금 유무, 스레드 아이디를 확인할 수 있음. (재진입 가능) => 비용이 많이 든다

        static void Thread_1()
        {
            for(int i = 0; i < 100000; i++)
            {
                _lock.WaitOne();
                _num++;
                _lock.ReleaseMutex();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
            {
                _lock.WaitOne();
                _num--;
                _lock.ReleaseMutex();
            }
        }
        static void Main(string[] args)
        {
            Task t1=new Task(Thread_1);
            Task t2=new Task(Thread_2);
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(_num);
        }
    }
}

