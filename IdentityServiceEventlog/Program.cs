using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace IdentityServiceEventlog
{
    class Program
    {
        const string _eventlogsource = "ActiveDirectory Identity Service";
        static void Main(string[] args)
        {
            try
            {
                if (!EventLog.SourceExists(_eventlogsource))
                    EventLog.CreateEventSource(_eventlogsource, "Application");
                Console.WriteLine(_eventlogsource+" EventLogs Status OK !");
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }
            Console.WriteLine();
            Console.WriteLine("Strike any Key to Exit");
            Console.ReadKey(false);
        }
    }
}
