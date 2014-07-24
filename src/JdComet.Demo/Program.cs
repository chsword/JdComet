using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JdComet.Demo.Listeners;

namespace JdComet.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            MyCometMessageClient.Start("appkey",
                "secret");
            Console.ReadKey();
            
        }
    }
}
