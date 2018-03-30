using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        // Class used for parsing arguments passed through LCD Custom Data.
        // Not much here for now but done this for practice, readability and potential future expansion.
        public class ParseArguments
        {
            public string Group;
            public List<Directions> directions;

            public ParseArguments(string args)
            {
                directions = new List<Directions>();
                if (string.IsNullOrEmpty(args))
                {  
                    Group = string.Empty;
                    return;
                }
                string[] argumentsSplit = args.Split(':');
                GetDirections(ref argumentsSplit);
            }

            private void GetDirections(ref string[] argumentsSplit)
            {
                for (int i = 0; i < argumentsSplit.Length; i++)
                {
                    if (argumentsSplit[i].Equals("group", StringComparison.OrdinalIgnoreCase))
                    {
                        Group = argumentsSplit[i + 1];
                        continue;
                    }
                    Directions direction;
                    if (Enum.TryParse(FirstLetterCapital(argumentsSplit[i]), out direction))
                    {
                        directions.Add(direction);
                    }
                }
            }
        }
    }
}
