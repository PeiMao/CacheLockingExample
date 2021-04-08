using InterSystems.Data.CacheClient;
using InterSystems.Data.CacheTypes;
using System;

namespace CacheLockingExample
{
    class LockID
    {
        static void Main(string[] args)
        {
            CacheConnection CacheConnect = new CacheConnection();
            CacheConnect.ConnectionString = "Server = 192.168.1.10; "
              + "Port = 5641; " + "Namespace = COMPANY2; "
              + "Password = SYS; " + "User ID = _SYSTEM;";
            CacheConnect.Open();

            var startTime = DateTime.Now;

            CacheConnect.DynamicMode = true;
            CacheMethodSignature mtdSignature = CacheConnect.GetMtdSignature();

            mtdSignature.Clear();
            mtdSignature.Add(375399, false); //id
            mtdSignature.Add(0, false); //exclusive lock
            mtdSignature.Add(0, false); //immediate timeout
            mtdSignature.SetReturnType(CacheConnect, ClientTypeId.tStatus);
            CacheObject.RunClassMethod(CacheConnect, "User.Customer", "%LockId", mtdSignature);
            CacheStatus status = (CacheStatus)((CacheStatusReturnValue)(mtdSignature.ReturnValue)).Value;
            if (status.IsOK)
            {
                Console.WriteLine("Lock acquired");

                Console.ReadLine();

                mtdSignature.Clear();
                mtdSignature.Add(1, false); //id
                mtdSignature.SetReturnType(CacheConnect, ClientTypeId.tStatus);
                CacheObject.RunClassMethod(CacheConnect, "Sample.Person", "%UnlockId", mtdSignature);
                Console.WriteLine("Unlocked");
            }
            else
            {
                Console.WriteLine("Already Locked");
            }
            var endTime = DateTime.Now;
            var diff = endTime.Subtract(startTime);
            var res = String.Format("{0} milliseconds", diff.TotalMilliseconds);
            Console.WriteLine($"Time Spend on lock checking: {res}");
            CacheConnect.Close();

            

        }
    }
}
