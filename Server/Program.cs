using Server;
using System;

namespace ServerClientUDP
{
    class Program
    {
       
        static void Main(string[] args)
        {
            BasicServer bs = new BasicServer();
            
            Console.WriteLine("started up Server.. Listening....");
            Console.ReadLine();
           
        }
    }
}
