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

            /* Two dimensional array holding current and max thrust.
             * First dimension:
             * 0 - upwards      3 - backwards
             * 1 - downwards    4 - left
             * 2 - forwards     5 - right
             * Second dimension:
             * 0 - current thrust   1 - max thrust */
            float[,] thrustValues = new float[6, 2];
            GetThrust(ref thrusters, ref thrustValues);

            string[] displayText = new string[6] { " Up: ", " Down: ", " Forward: ", " Backward: ", " Left: ", " Right: " };

            foreach (IMyTextPanel display in displays)
            {
                if (display.CustomName.Contains(lcdTag))
                {
                    for (int i = 0; i < displayText.Length; i++)
                    {
                        float thrustPercent = (thrustValues[i, 0] / thrustValues[i, 1]) * 100;
                        string displayLine = displayText[i] + ToSI(thrustValues[i, 0], "n0") + "N/" + ToSI(thrustValues[i, 1], "n0") + "N\n";
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
        void GetThrust(ref List<IMyThrust> thrusters, ref float[,] thrustValues)
        {
            foreach (IMyThrust thruster in thrusters)
            {
                Vector3I direction = thruster.GridThrustDirection;
                //Up and down thrusters seem to be inverted. Need to test on more cockpits.
                if (direction == VRageMath.Vector3I.Down)
                {
                    thrustValues[0, 0] += thruster.CurrentThrust;
                    thrustValues[0, 1] += thruster.MaxEffectiveThrust;
                }
                else if (direction == VRageMath.Vector3I.Up)
                {
                    thrustValues[1, 0] += thruster.CurrentThrust;
                    thrustValues[1, 1] += thruster.MaxEffectiveThrust;
                }
                else if (direction == VRageMath.Vector3I.Forward)
                {
                    thrustValues[2, 0] += thruster.CurrentThrust;
                    thrustValues[2, 1] += thruster.MaxEffectiveThrust;
                }
                else if (direction == VRageMath.Vector3I.Backward)
                {
                    thrustValues[3, 0] += thruster.CurrentThrust;
                    thrustValues[3, 1] += thruster.MaxEffectiveThrust;
                }
                else if (direction == VRageMath.Vector3I.Left)
                {
                    thrustValues[4, 0] += thruster.CurrentThrust;
                    thrustValues[4, 1] += thruster.MaxEffectiveThrust;
                }
                else if (direction == VRageMath.Vector3I.Right)
                {
                    thrustValues[5, 0] += thruster.CurrentThrust;
                    thrustValues[5, 1] += thruster.MaxEffectiveThrust;
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
    }
}