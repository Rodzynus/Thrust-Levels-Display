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
        * To filter direction you want to display, type directions separated by a colon (case sensitive).
        * Ex. Up:Forward:Left:Right
        * Ex. group:Special Thrusters:Forward:Up

        * === Configuration: == */
       
        // Tag which scripts looks for when finding LCDs to write to.
        const string lcdTag = "[thrusters]";

        Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
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
                    // List containing all lines to be passed as arguments to the GetThrust method.
                    List<string> arguments = new List<string>();
                    if (display.CustomData.Length > 0)
                    {
                        string[] customLines = display.CustomData.Split('\n');
                        foreach (string line in customLines)
                        {
                            arguments.Add(line);
                        }
                    } else
                    {
                        arguments.Add("");
                    }
                    string displayLines = "";

                    foreach (string argument in arguments)
                    {
                        List<GroupedThrusters> groupedThrusters = GetThrust(argument);

                        foreach(GroupedThrusters groupedThruster in groupedThrusters)
                        {
                            float thrustPercent = (groupedThruster.currentThrust / groupedThruster.maxEffectiveThrust) * 100;
                            displayLines += " " + groupedThruster.directionName + ": " + ToSI(groupedThruster.currentThrust, "n0") + "N/" + ToSI(groupedThruster.maxEffectiveThrust, "n0") + "N\n";
                            displayLines += " " + thrustPercent.ToString("n1") + "%\n";
                        }
                    }
                    // Write on the display.
                    display.WritePublicText(displayLines, false);
                    display.ShowPublicTextOnScreen();
                }
            }
        }

        // Fill up the array with thrust values.
        List<GroupedThrusters> GetThrust(string arguments)
        {
            string[] argumentsSplit = arguments.Split(':');

            string groupName = "";

            if (argumentsSplit[0].ToLower() == "group")
            {
                groupName = argumentsSplit[1];
            }

            // Compare list of arguments against matching directions and add them to list of GroupedThrusters.
            List<GroupedThrusters> groupedThrusters = new List<GroupedThrusters>();
            foreach (string argument in argumentsSplit)
            {
                if (argument == "Up" || argument == "Down" || argument == "Forward" || argument == "Backward" || argument == "Left" || argument == "Right")
                {
                    groupedThrusters.Add(new GroupedThrusters(argument));
                }
            }

            // If no valid arguments were found, add all thrusters.
            if (groupedThrusters.Count == 0)
            {
                groupedThrusters.Add(new GroupedThrusters("Up"));
                groupedThrusters.Add(new GroupedThrusters("Down"));
                groupedThrusters.Add(new GroupedThrusters("Forward"));
                groupedThrusters.Add(new GroupedThrusters("Backward"));
                groupedThrusters.Add(new GroupedThrusters("Left"));
                groupedThrusters.Add(new GroupedThrusters("Right"));
            }

            List<IMyThrust> thrusters = new List<IMyThrust>();
            if (!string.IsNullOrWhiteSpace(groupName) )
            {
                try
                { GridTerminalSystem.GetBlockGroupWithName(groupName).GetBlocksOfType<IMyThrust>(thrusters);
                } catch (Exception)
                { Echo($"Group {groupName} doesn't exist."); }
            } else
            {
                GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);
            }

            if (thrusters.Count == 0) { return groupedThrusters; }

            foreach (IMyThrust thruster in thrusters)
            {
                // This returns direction of the nozzle. If statements are inverted to get direction of thrust instead.
                string direction = VRageMath.Vector3I.GetDominantDirection(thruster.GridThrustDirection).ToString();
                switch (direction)
                {
                    case "Up":
                        direction = "Down"; break;
                    case "Down":
                        direction = "Up"; break;
                    case "Forward":
                        direction = "Backward"; break;
                    case "Backward":
                        direction = "Forward"; break;
                    case "Left":
                        direction = "Right"; break;
                    case "Right":
                        direction = "Left"; break;
                }

                foreach (GroupedThrusters groupedThruster in groupedThrusters)
                {
                    if (groupedThruster.directionName == direction)
                    {
                        groupedThruster.currentThrust += thruster.CurrentThrust;
                        groupedThruster.maxEffectiveThrust += thruster.MaxEffectiveThrust;
                    }
                }
            }
            return groupedThrusters;
        }

        // Not my own piece of code, method found here: https://stackoverflow.com/questions/12181024/formatting-a-number-with-a-metric-prefix
        public string ToSI(float f, string format = null)
        {
            if (f == 0)
            {
                return "0";
            }
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

        // Class for grouping thrusters for each direction.
        private class GroupedThrusters
        {
            public string directionName;
            public float currentThrust;
            public float maxEffectiveThrust;

            public GroupedThrusters (string name)
            {
                directionName = name;
                currentThrust = 0;
                maxEffectiveThrust = 0;
            }
        }
    }
}
