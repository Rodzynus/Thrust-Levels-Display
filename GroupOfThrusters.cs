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
        // Class keeping whole groups of thrusters in all directions.
        public class GroupOfThrusters
        {
            public List<DirectionalThrusters> directionalThrusters;
            private string group;
            private Program myGP;

            public string Group { get { return group; } }

            public GroupOfThrusters(Program MyGP, ParseArguments arguments, ref List<IMyThrust> AllThrusters)
            {
                directionalThrusters = new List<DirectionalThrusters>();
                myGP = MyGP;

                if (!arguments.directions.Any()) DefaultFill();
                else
                    foreach (Directions direction in arguments.directions)
                        directionalThrusters.Add(new DirectionalThrusters(direction));

                if (!String.IsNullOrEmpty(arguments.Group))
                {
                    group = arguments.Group;
                    List<IMyThrust> thrusters = new List<IMyThrust>();
                    try { myGP.GridTerminalSystem.GetBlockGroupWithName(group).GetBlocksOfType<IMyThrust>(thrusters); }
                    catch (Exception) { myGP.Echo($"No thrusters detected in {group} group or the group doesn't exist."); return; }
                    GetThrust(ref thrusters);
                }
                else GetThrust(ref AllThrusters);
            }

            private void DefaultFill()
            {
                directionalThrusters.Add(new DirectionalThrusters(Directions.Up));
                directionalThrusters.Add(new DirectionalThrusters(Directions.Down));
                directionalThrusters.Add(new DirectionalThrusters(Directions.Forward));
                directionalThrusters.Add(new DirectionalThrusters(Directions.Backward));
                directionalThrusters.Add(new DirectionalThrusters(Directions.Left));
                directionalThrusters.Add(new DirectionalThrusters(Directions.Right));
            }

            private void GetThrust(ref List<IMyThrust> thrusters)
            {
                foreach (IMyThrust thruster in thrusters)
                {
                    // This returns direction of the nozzle. If statements are inverted to get direction of thrust instead.
                    Directions direction = Directions.Up;   // Defaults to Up just to satisfy the compiler. Will be overritten with actual value.
                    string nozzleDirection = VRageMath.Vector3I.GetDominantDirection(thruster.GridThrustDirection).ToString();
                    switch (nozzleDirection)
                    {
                        case "Up":
                            direction = Directions.Down; break;
                        case "Down":
                            direction = Directions.Up; break;
                        case "Forward":
                            direction = Directions.Backward; break;
                        case "Backward":
                            direction = Directions.Forward; break;
                        case "Left":
                            direction = Directions.Right; break;
                        case "Right":
                            direction = Directions.Left; break;
                    }

                    foreach (DirectionalThrusters directionalThruster in directionalThrusters)
                    {
                        if (directionalThruster.direction == direction)
                        {
                            directionalThruster.CurrentThrust += thruster.CurrentThrust;
                            directionalThruster.MaxEffectiveThrust += thruster.MaxEffectiveThrust;
                        }
                    }
                }
            }
        }
    }
}
