using System;
using System.Threading;
using System.Threading.Tasks;
//=========================================
//Reader Writer Lock
//=========================================
namespace ServerCore
{
    class Program
    {
        static object _lock = new object();
        class Reward
        {

        }
        static ReaderWriterLockSlim _lock3=new ReaderWriterLockSlim();

        //99.99%
        static Reward GetRewardById(int id)
        {
            _lock3.EnterReadLock();

            _lock3.ExitReadLock();

            return null;
        }

        //0.01%
        static void AddReward(Reward reward)
        {
            _lock3.EnterWriteLock();
            _lock3.ExitWriteLock();
            
        }
        static void Main(string[] args)
        {
            lock (_lock)
            {

            }
        }
    }
}

