using InterSystems.Data.CacheClient;
using InterSystems.Data.CacheTypes;
using System;
using System.Data;

namespace CacheLockingExample
{
    class LockQuery
    {
        static void Main(string[] args)
        {
            CacheConnection CacheConnect = new CacheConnection();
            CacheConnect.ConnectionString = "Server=192.168.1.10;Port=5641;User=_SYSTEM;Password=SYS;Namespace=company2;Pooling=false;";
            CacheConnect.Open();

            //Lock Query List Example
            Console.WriteLine("Lock Query List Example");
            CacheDataAdapter cadpt = new CacheDataAdapter();
            DataSet ds = new DataSet();
            CacheCommand cmdCache = new CacheCommand("%SYS.LockQuery_List", CacheConnect);
            cmdCache.CommandType = CommandType.StoredProcedure;
            cadpt.SelectCommand = cmdCache;
            cadpt.Fill(ds, "Locks");
            bool locked = false;
            int id = 13395;
            string table = "Invoice";
            foreach (DataRow row in ds.Tables["Locks"].Rows) //loop through locks
            {
                Console.WriteLine(row["LockString"]);
                if (row["LockString"].Equals("^oo" + table + "D(" + id + ")")) //depends on class storage definition data location
                {
                    locked = true;
                    break;
                }
            }
            if (locked)
            {
                Console.WriteLine("Record is locked");
            }
            else
            {
                Console.WriteLine("Record is unlocked");
            }
            Console.WriteLine();
            //Process Query Example
            Console.WriteLine("Process Query Example");
            CacheCommand cmdCache2 = new CacheCommand("%SYS.ProcessQuery_CONTROLPANEL", CacheConnect);
            cmdCache2.CommandType = CommandType.StoredProcedure;
            CacheParameter JNparam = new CacheParameter("JobNumber", CacheDbType.Int);
            JNparam.Value = 1;
            cmdCache2.Parameters.Add(JNparam);
            cadpt.SelectCommand = cmdCache2;
            cadpt.Fill(ds, "Processes");
            foreach (DataRow row in ds.Tables["Processes"].Rows) //loop through processes
            {
                Console.WriteLine("Job Number: " + row["Job#"] + ", Namespace: " + row["Nspace"]);
            }
        }
    }
}
