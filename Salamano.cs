using Intf;
using System;

public class Salamano
{
    public static Action Roam(int instance, Room room)
    {
        Func<string> Look = () => {return looks[instance];};

        return () => {
            GameObject salamano = room.AddObject<GameObject>("salamano");
            salamano.conditions.Add("firstThreeTalk", false);

            Func<string> Talk = (instance == 3)? LastTalk(salamano) : DefaultTalk;

            salamano.SetTransitiveCommand("talk", Talk);
            salamano.SetTransitiveCommand("look", Look);
            salamano.SetTransitiveCommand("who", Who);
        };
    }

    static string Who()
    {
        return "Old Salamano is your neighbor across the landing. He has reddish scabs on his face and wispy yellow hair. His dog has taken on his master's stooped look. It has mange, and is covered with brown sores and scabs. The two of them have been inseperable for eight years.";
    }

    static string DefaultTalk()
    {
        return "'Filthy, stinking bastard!'";
    }

    static Func<string> LastTalk(GameObject sal)
    {
        return () => {
            if (!sal.conditions["firstThreeTalk"])
            {
                sal.conditions["firstThreeTalk"] = true;
                return "'He was a good dog. They say he's not at the pound. You should have seen him before he got sick. His coat was the best thing about him. We'd have a run-in every now and then, but he was a good dog just the same.'";
            }
            else
            {                
               return "'He was a good dog just the same.'";
            }
        };
    }

    static string[] looks = new string[]
    {
        "SALAMANO is with his dog. The mangy thing whimpers as his master curses at it.",
        "SALAMANO's dog is dragging him down the street. He stares at the spaniel in hatred.",
        "SALAMANO is beating his dog and swearing at it for making him stumble.",
        "SALAMANO stands alone in front of his door, muttering to himself."
    };
}