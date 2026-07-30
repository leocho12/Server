using System;
using System.Threading;
using System.Threading.Tasks;
//=========================================
//Thread local storage
//=========================================
namespace ServerCore
{
    class Program
    {
        //스레드 고유의 전역 변수를 만들고 싶을 때 ThreadLocal<T>를 사용
        static ThreadLocal<string> ThreadName = new ThreadLocal<string>(() => { return $"my name is {Thread.CurrentThread.ManagedThreadId}"; });// 이미 사용한 스레드를 재사용함

        static void WhoAmI()
        {
            bool repeat = ThreadName.IsValueCreated;
            if(repeat)
                Console.WriteLine(ThreadName.Value+"(repeat)");
            else
                Console.WriteLine(ThreadName.Value);
        }
        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(3, 3);
            Parallel.Invoke(WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI);

            ThreadName.Dispose();
        }
    }
}

