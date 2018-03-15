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
        const string lcdTag = "[thrusters]";

        Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Main()
        {
            List<IMyThrust> thrusters = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);
            if (thrusters.Count == 0)
            {
                Echo("No thrusters detected.");
                return;
            }

            List<IMyTextPanel> displays = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(displays);
            if (displays.Count == 0)
            {
                Echo("No valid displays detected.");
                return;
            }

            GroupedThrusters[] groupedThrusters = {
                new GroupedThrusters("Up"),
                new GroupedThrusters("Down"),
                new GroupedThrusters("Forward"),
                new GroupedThrusters("Backward"),
                new GroupedThrusters("Left"),
                new GroupedThrusters("Right"),
            };

            GetThrust(ref thrusters, ref groupedThrusters);

            foreach (IMyTextPanel display in displays)
            {
                if (display.CustomName.Contains(lcdTag))
                {
                    for (int i = 0; i < groupedThrusters.Length; i++)
                    {
                        float thrustPercent = (groupedThrusters[i].currentThrust / groupedThrusters[i].maxEffectiveThrust) * 100;
                        string displayLine = groupedThrusters[i].directionName + ToSI(groupedThrusters[i].currentThrust, "n0") + "N/" + ToSI(groupedThrusters[i].maxEffectiveThrust, "n0") + "N\n";
                        displayLine += " " + thrustPercent.ToString("n1") + "%\n";

                        // Write on the display.
                        if (i == 0) { display.WritePublicText(displayLine, false); }
                        else { display.WritePublicText(displayLine, true); }
                    }
                    display.ShowPublicTextOnScreen();
                }
            }
        }

        // Fill up the array with thrust values.
        void GetThrust(ref List<IMyThrust> thrusters, ref GroupedThrusters[] groupedThrusters)
        {
            foreach (IMyThrust thruster in thrusters)
            {
                // This returns direction of the nozzle. If statements are inverted to get direction of thrust instead.
                Vector3I direction = thruster.GridThrustDirection;
                if (direction == VRageMath.Vector3I.Down)
                {
                    groupedThrusters[0].currentThrust += thruster.CurrentThrust;
                    groupedThrusters[0].maxEffectiveThrust += thruster.MaxEffectiveThrust;
                }
                else if (direction == VRageMath.Vector3I.Up)
                {
                    groupedThrusters[1].currentThrust += thruster.CurrentThrust;
                    groupedThrusters[1].maxEffectiveThrust += thruster.MaxEffectiveThrust;
                }
                else if (direction == VRageMath.Vector3I.Backward)
                {
                    groupedThrusters[2].currentThrust += thruster.CurrentThrust;
                    groupedThrusters[2].maxEffectiveThrust += thruster.MaxEffectiveThrust;
                }
                else if (direction == VRageMath.Vector3I.Forward)
                {
                    groupedThrusters[3].currentThrust += thruster.CurrentThrust;
                    groupedThrusters[3].maxEffectiveThrust += thruster.MaxEffectiveThrust;
                }
                else if (direction == VRageMath.Vector3I.Right)
                {
                    groupedThrusters[4].currentThrust += thruster.CurrentThrust;
                    groupedThrusters[4].maxEffectiveThrust += thruster.MaxEffectiveThrust;
                }
                else if (direction == VRageMath.Vector3I.Left)
                {
                    groupedThrusters[5].currentThrust += thruster.CurrentThrust;
                    groupedThrusters[5].maxEffectiveThrust += thruster.MaxEffectiveThrust;
                }
            }
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

        // Struct for grouping thrusters for each direction.
        private struct GroupedThrusters
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