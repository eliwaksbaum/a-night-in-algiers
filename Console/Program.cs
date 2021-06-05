using System;
using System.Collections.Generic;
using Intf;

namespace Algiers
{
    class Program
    {
        static void Main(string[] args)
        {
            World world = AlgiersWorld.SetWorld();
            Console.WriteLine("");
            Console.WriteLine(world.start);
            while (!world.done)
            {
                Console.WriteLine("");
                string response = Parser.Parse(Console.ReadLine(), world);
                Console.WriteLine("");
                Console.WriteLine(response);
            }
        }
    }
}
