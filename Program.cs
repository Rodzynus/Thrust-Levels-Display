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
    partial class Program : MyGridProgram
    {
        /* Thrust Levels Display by Rodzyn *
        *  Displays current thrust for each direction. *
        *  Written using MDK for SE: https://github.com/malware-dev/MDK-SE *

        * ======= Usage: ====== *
        * Set the tag you want to use to mark LCDs used by the script.
        * The script will now show you values for all thrusters.
        * 
        * To show only specific group add 'group:GroupName' to Custom Data of an LCD.
        * Ex. group:Special Thrusters
        *
        * To filter direction you want to display, type directions separated by a colon.
        * Ex. Up:Forward:Left:Right
        * Ex. group:Special Thrusters:Forward:Up

        * === Configuration: == */
       
        // Tag which scripts looks for when finding LCDs to write to.
        const string lcdTag = "[thrusters]";

        Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main()
        {
            List<IMyTextPanel> displays = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(displays);
            if (displays.Count == 0)
            {
                Echo("No valid displays detected.");
                return;
            }

            foreach (IMyTextPanel display in displays)
            {
                if (display.CustomName.Contains(lcdTag))
                {
                    List<ParseArguments> arguments = new List<ParseArguments>();
                    if (display.CustomData.Length > 0)
                    {
                        string[] customLines = display.CustomData.Split('\n');
                        foreach (string line in customLines) arguments.Add(new ParseArguments(line));
                    }
                    
                    List<GroupOfThrusters> groupsOfThrusters = new List<GroupOfThrusters>();

                    if (arguments.Count > 0)
                    {
                        foreach (ParseArguments argument in arguments)
                        {
                            groupsOfThrusters.Add(new GroupOfThrusters(this, argument));
                        }
                    }
                    else groupsOfThrusters.Add(new GroupOfThrusters(this));
                    
                    string displayLines = "";

                    foreach (GroupOfThrusters groupOfThrusters in groupsOfThrusters)
                    {
                        if (!string.IsNullOrEmpty(groupOfThrusters.Group))
                        {
                            displayLines += $" {groupOfThrusters.Group}:\n";
                        }

                        foreach (DirectionalThrusters thruster in groupOfThrusters.Thrusters)
                        {
                            displayLines += $" {thruster.DirectionName} {ToSI(thruster.CurrentThrust, "n0")}N/{ToSI(thruster.MaxEffectiveThrust, "n0")}N\n";
                            displayLines += $" {thruster.Percentage().ToString("n1")}%\n";
                        }
                    }

                    // Write on the display.
                    display.WritePublicText(displayLines, false);
                    display.ShowPublicTextOnScreen();
                }
            }
        }

        // Not my own piece of code, method found here: https://stackoverflow.com/questions/12181024/formatting-a-number-with-a-metric-prefix
        public static string ToSI(float f, string format = null)
        {
            if (f == 0) return "0";

            char[] incPrefixes = new[] { 'k', 'M', 'G', 'T', 'P', 'E', 'Z', 'Y' };
            char[] decPrefixes = new[] { 'm', '\u03bc', 'n', 'p', 'f', 'a', 'z', 'y' };

            int degree = (int)Math.Floor(Math.Log10(Math.Abs(f)) / 3);
            double scaled = f * Math.Pow(1000, -degree);

            char? prefix = null;
            switch (Math.Sign(degree))
            {
                case 1: prefix = incPrefixes[degree - 1]; break;
                case -1: prefix = decPrefixes[-degree - 1]; break;
            }

            return scaled.ToString(format) + prefix;
        }

        // Makes first letter of a string upper case and the rest lower case.
        public static string FirstLetterCapital(string s)
        {
            if (string.IsNullOrEmpty(s))  return "";

            if (s.Length == 1) return s.ToUpper();

            char[] c = s.ToLower().ToCharArray();
            c[0] = char.ToUpper(c[0]);
            return new string (c);
        }
    }
}
