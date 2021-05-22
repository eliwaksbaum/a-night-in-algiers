using System;
using System.Collections.Generic;
using Intf;

namespace Algiers
{
    class Program
    {
        static void Main(string[] args)
        {
            World world = SetWorld();
            Console.WriteLine(world.Start());
            while (!world.done)
            {
                Console.WriteLine("");
                string response = Parser.Parse(Console.ReadLine(), world);
                Console.WriteLine("");
                Console.WriteLine(response);
            }
        }

        static World SetWorld()
        {
            World world = new World();

            //CHAMBRE
            Room chambre = world.AddRoom("chambre");
            chambre.description = "This is the bedroom.";
            chambre.exits = new Dictionary<string, string>
            {
                {"left", "balcony"},
                {"right", "antechambre"}
            };

            //CHAIR
            Item chair = chambre.AddItem("chair");
            chair.OnTake = () =>
            {
                return "You can't take the chair.";
            };
            chair.Done();

            world.Done();
            return world;
        }
    }
}
