using System;
using System.Threading;
using System.Threading.Tasks;
//=========================================
//Event
//=========================================
namespace ServerCore
{
    class Lock
    {

        AutoResetEvent _available=new AutoResetEvent(true);//AutoResetEvent는 자동으로 lock을 걸어줌
        public void Acquire()//획득
        {
           _available.WaitOne();//입장 시도
        }
        public void Release()//해제
        {
            _available.Set();//flag=true
        }
    }
    class Program
    {
        static int _num = 0;
        static Lock _lock = new Lock();


        static void Thread_1()
        {
            for(int i = 0; i < 100000; i++)
            {
                _lock.Acquire();
                _num++;
                _lock.Release();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
            {
                _lock.Acquire();
                _num--;
                _lock.Release();
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

