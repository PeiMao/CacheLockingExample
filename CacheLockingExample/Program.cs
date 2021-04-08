using InterSystems.Data.CacheClient;
using InterSystems.Data.CacheTypes;
using System;

namespace CacheLockingExample
{
    class Program
    {
        static void Main(string[] args)
        {
            string connString = "Server=192.168.1.10;Port=5641;User=_SYSTEM;Password=SYS;Namespace=company2;Pooling=false;";
            const int ErrLogID = 354159;

            Console.WriteLine("\nProgram starting.");

            try
            {
                CacheConnection conn = new CacheConnection(connString);
                
                conn.Open();

                var locked= Lock(conn, "ErrLog", ErrLogID);

                if (locked != null)
                    Console.WriteLine("Lock Successfully");
                //Doing things

                //another program
                //Now another program is trying to edit the same row. I want to check if the row is locked.
                CacheConnection conn2 = new CacheConnection(connString);
                
                conn2.Open();

                var startTime= DateTime.Now;
                Console.WriteLine(startTime.ToString());
                locked = Lock(conn2, "ErrLog", ErrLogID);
                if (locked == null)
                    Console.WriteLine("The row is locked");
                var endTime = DateTime.Now;
                Console.WriteLine(endTime.ToString());
                var diff = endTime.Subtract(startTime);
                var res = String.Format("{0}:{1}:{2}", diff.Hours, diff.Minutes, diff.Seconds);
                Console.WriteLine($"Time Spend on lock checking {res}");

              

            }
            catch (Exception ee)
            {
                Console.WriteLine("ERROR: " + ee.Message);
            }

            Console.WriteLine("Program exited.");
        }

        static CacheObject Lock(CacheConnection conn, string tableName, int id)
        {

            try
            {
                //ADD CACHE OBJECT LOCKING HERE
                conn.DynamicMode = true; //needed for proxy objects dynamic binding 
                CacheMethodSignature mtdSignature = conn.GetMtdSignature(); //used to pass attributed and return values from methods

                mtdSignature.Clear();
                mtdSignature.Add(id, false); //Id = 1, false means dont pass by reference
                mtdSignature.Add(0, false); //exclusive lock
                mtdSignature.Add(0, false); //immediate timeout
                mtdSignature.SetReturnType(conn, ClientTypeId.tStatus);
                CacheObject.RunClassMethod(conn, $"User.{tableName}", "%LockId", mtdSignature);
                CacheStatus status = (CacheStatus)((CacheStatusReturnValue)(mtdSignature.ReturnValue)).Value;

                if (status.IsOK)
                {
                    Console.WriteLine("Lock acquired");

                    Console.ReadLine();

                    //mtdSignature.Clear();
                    //mtdSignature.Add(id, false); //id
                    //mtdSignature.SetReturnType(conn, ClientTypeId.tStatus);
                    //CacheObject.RunClassMethod(conn, "Sample.Person", "%UnlockId", mtdSignature);
                    //Console.WriteLine("Unlocked");
                }
                else
                {
                    Console.WriteLine("Already Locked");
                }

                return (CacheObject)((CacheObjReturnValue)(mtdSignature.ReturnValue)).Value;
               
            }
            catch (Exception ee)
            {

                throw new Exception($"Error in DbConn.Lock(tableName, id)", ee);
            }
        } 
    }
}
