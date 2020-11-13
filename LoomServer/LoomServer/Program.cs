using System;

namespace LoomServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers
            Server.Start(50, 26950);

            Console.ReadKey();
        }
    }
}
