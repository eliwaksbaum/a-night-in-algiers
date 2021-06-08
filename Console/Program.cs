using System;
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
                string response = "";
                Console.WriteLine("");
                if (world.state == "play")
                {
                    response = Parser.Parse(Console.ReadLine(), world);
                }
                else if (world.state == "quit")
                {
                    string answer = Console.ReadLine();
                    if (answer == "y" || answer == "yes")
                    {
                        world.done = true;
                        response = "You decide nothing is really worth doing today. You make your way to your balcony and look out onto the city, watching life pass by. It is a beautiful night in Algiers." + Environment.NewLine;
                    }
                    else if (answer == "n" || answer == "no")
                    {
                        world.state = "play";
                        response = "You seem to have lost your train of thought. Were you in the middle of something?";
                    }
                    else
                    {
                        response = "Are you sure you want to give up? (yes/no)";
                    }
                }
                Console.WriteLine("");
                Console.WriteLine(response);
            }
        }
    }
}
