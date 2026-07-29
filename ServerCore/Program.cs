using System;
using System.Threading;
using System.Threading.Tasks;
//=========================================
//Spin Lock
//=========================================
namespace ServerCore
{
    class SpinLock
    {
        volatile bool _locked = false;
        public void Acquire()//획득
        {
            while (_locked)//잠금이 풀릴때까지 반복
            {

            }
            _locked = true;//잠금 획득
        }
        public void Release()//해제
        {

        }
    }
    class Program
    {
        static int _num = 0;
        static SpinLock _lock = new SpinLock();


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

