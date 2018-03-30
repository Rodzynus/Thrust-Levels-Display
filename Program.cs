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
        * Group tag has to be at the beginning of the line.
        * Ex. Up:Forward:Left:Right
        * Ex. group:Special Thrusters:Forward:Up

        * === Configuration: == */
       
        // Tag which scripts looks for when finding LCDs to write to.
        const string lcdTag = "[thrusters]";

        /* === Script Body: === */
        public enum Directions : byte { Up, Down, Forward, Backward, Left, Right };

        Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main()
        {
            List<IMyTextPanel> displays = new List<IMyTextPanel>();
            try { GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(displays); }
            catch (Exception) { Echo("No displays detected."); return; }

            List<IMyThrust> allThrusters = new List<IMyThrust>();
            try { GridTerminalSystem.GetBlocksOfType<IMyThrust>(allThrusters); }
            catch (Exception) { Echo("No thrusters detected."); return; }

            foreach (IMyTextPanel display in displays)
            {
                // If display doesn't contain the tag, skip it.
                if (!display.CustomName.Contains(lcdTag)) continue;

                // StringBuilder for writing on the display.
                StringBuilder displayLines = new StringBuilder();

                // Iterate through Custom Data to grab arguments and build the displayLines.
                string[] customLines = display.CustomData.Split('\n');
                foreach (string line in customLines)
                {
                    GenerateLCDText(ref displayLines, allThrusters, line);
                }

                // Write on the display.
                display.WritePublicText(displayLines, false);
                display.ShowPublicTextOnScreen();
            }
        }

        public void GenerateLCDText(ref StringBuilder displayLines, List<IMyThrust> allThrusters, string line)
        {
            ParseArguments arguments = new ParseArguments(line);
            List<DirectionalThrusters> thrusters = new List<DirectionalThrusters>();

            if (!string.IsNullOrEmpty(arguments.Group))
            {
                displayLines.Append($" {arguments.Group}:\n");

                try { GridTerminalSystem.GetBlockGroupWithName(arguments.Group).GetBlocksOfType<IMyThrust>(allThrusters); }
                catch (Exception) { Echo($"No thrusters detected in {arguments.Group} group or the group doesn't exist."); return; }   
            }
            FillDirectionThrusterList(ref thrusters, allThrusters, arguments);

            foreach (DirectionalThrusters directionalThruster in thrusters)
            {
                displayLines.Append($" {directionalThruster.Direction.ToString()} {ToSI(directionalThruster.CurrentThrust, "n0")}N/{ToSI(directionalThruster.MaxEffectiveThrust, "n0")}N\n");
                displayLines.Append($" {directionalThruster.Percentage().ToString("n1")}%\n");
            }
        }

        private void FillDirectionThrusterList(ref List<DirectionalThrusters> directionalThrusters, List<IMyThrust> thrusters, ParseArguments arguments)
        {
            // Build the list of directions to show.
            if (!arguments.directions.Any())
            {
                directionalThrusters.Add(new DirectionalThrusters(Directions.Up));
                directionalThrusters.Add(new DirectionalThrusters(Directions.Down));
                directionalThrusters.Add(new DirectionalThrusters(Directions.Forward));
                directionalThrusters.Add(new DirectionalThrusters(Directions.Backward));
                directionalThrusters.Add(new DirectionalThrusters(Directions.Left));
                directionalThrusters.Add(new DirectionalThrusters(Directions.Right));
            }
            else
            {
                foreach (Directions direction in arguments.directions)
                    directionalThrusters.Add(new DirectionalThrusters(direction));
            }
            DirectionalThrusters.GetThrust(thrusters, ref directionalThrusters);
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
