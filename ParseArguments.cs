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
            public string[] Arguments;

            public ParseArguments(string args)
            {
                if (args.ToLower().StartsWith("group"))
                {
                    string[] argumentsSplit = args.Split(':');
                    Group = argumentsSplit[1];
                    Arguments = new string[argumentsSplit.Length - 2];
                    Array.ConstrainedCopy(argumentsSplit, 2, Arguments, 0, argumentsSplit.Length - 2);
                }
                else
                {
                    Group = string.Empty;
                    Arguments = args.Split(':');
                }
            }
        }
    }
}
