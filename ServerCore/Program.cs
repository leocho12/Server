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
        volatile int _locked = 0;
        public void Acquire()//획득
        {
            //while (_locked)//잠금이 풀릴때까지 반복
            //{

            //}
            //_locked = true;//잠금 획득
            //위 코드는 잠금을 획득하는 동안 다른 스레드가 lock획득 가능해서 제대로 동작하지 않음
            while(true)
            {
                //int original = Interlocked.Exchange(ref _locked, 1);//원래 값을 original에 저장하고 _locked를 1로 바꿈
                //if (original == 0)//original이 0이면 다른 스레드가 잠금을 획득하지 않은 상태이므로 잠금 획득
                //    break;
                //아래가 좀 더 일반적인 방법

                int expected = 0;
                int desired = 1;
                int original = Interlocked.CompareExchange(ref _locked, desired, expected);//_locked가 expected와 일치하면 desired로 바꾸고, 일치하지 않으면 아무것도 안함
                if (original == 0)//original이 0이면 다른 스레드가 잠금을 획득하지 않은 상태이므로 잠금 획득
                    break;

                // 쉬기
                //Thread.Sleep(1);    //무조건 휴식 => 무조건 1ms 쉬고 다시 시도
                //Thread.Sleep(0);    //조건부 양보 => 자신보다 우선순위가 높은 스레드가 있으면 양보하고, 없으면 바로 돌아옴
                Thread.Yield();     //관대한 양보 => 조건 없이 지금 실행가능한 스레드에게 양보 => 실행가능한 스레드 없으면 남은시간 소진
            }
        }
        public void Release()//해제
        {
            _locked = 0;
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

