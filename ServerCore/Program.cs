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
        static ThreadLocal<string> ThreadName = new ThreadLocal<string>();// 스레드마다 고유한 공간이 생김

        static void WhoAmI()
        {
            ThreadName.Value = $"my name is {Thread.CurrentThread.ManagedThreadId}";

            Thread.Sleep(1000);

            Console.WriteLine(ThreadName.Value);
        }
        static void Main(string[] args)
        {
            Parallel.Invoke(WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI);
        }
    }
}

