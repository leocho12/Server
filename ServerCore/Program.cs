using System;
using System.Threading;
using System.Threading.Tasks;
//=========================================
//Reader Writer Lock
//=========================================
namespace ServerCore
{
    //재귀적 락을 허용할지(no)
    //스핀 락 정책(5000번 -> 양보)
    class Program
    {
        const int EMPTY_FLAG= 0x0000000;
        const int WRITE_MASK = 0x7FF0000;
        const int READ_MASK = 0x000FFFF;
        const int MAX_SPIN_COUNT = 5000;

        //[unused(1)] [writerthreadid(15)] [readcount(16)] total 32 bytes
        int _flag = EMPTY_FLAG;

        public void WriteLock()
        {
            //아무도 writelock이나readlock을 가지고 있지 않으면 경합해서 소유권 얻음
            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            while (true)
            {
                for(int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        //시도 해서 성공하면 return
                        return;
                    }
                }

                //max_spin_count만큼 시도했는데도 실패하면 스레드 양보
                Thread.Yield();
            }
        }
        public void WriteUnlock()
        {
            Interlocked.Exchange(ref _flag, EMPTY_FLAG);
        } 
        public void ReadLock()
        {
            //아무도 writelock을 가지고 있지 않으면 readcount를 증가시킴
            while (true)
            {
                for(int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    int expected = _flag & READ_MASK;
                    if (Interlocked.CompareExchange(ref _flag,expected+1,expected) == expected)
                    {
                        return;
                    }
                }
                Thread.Yield();
            }
        }
        public void ReadUnlock() 
        {
            
        }
    }
}

