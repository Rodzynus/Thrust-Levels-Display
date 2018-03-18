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
        public class GroupOfThrusters
        {
            private List<DirectionalThrusters> directionalThrusters;
            private string group;
            private Program myGP;

            public List<DirectionalThrusters> Thrusters { get { return directionalThrusters; } }
            public string Group { get { return group; } }

            public GroupOfThrusters(Program MyGP)
            {
                directionalThrusters = new List<DirectionalThrusters>();
                myGP = MyGP;
                DefaultFill();
                GetThrust();
            }

            public GroupOfThrusters(Program MyGP, ParseArguments arguments)
            {
                directionalThrusters = new List<DirectionalThrusters>();
                myGP = MyGP;
                group = arguments.Group;
                if (arguments.Arguments.Count() > 0)
                {
                    foreach (string argument in arguments.Arguments)
                    {
                        string formArgument = FirstLetterCapital(argument);
                        if (formArgument == "Up" || formArgument == "Down" || formArgument == "Forward" || formArgument == "Backward" || formArgument == "Left" || formArgument == "Right")
                            directionalThrusters.Add(new DirectionalThrusters(formArgument));
                    }
                }
                else DefaultFill();
                GetThrust();
            }

            private void DefaultFill()
            {
                directionalThrusters.Add(new DirectionalThrusters("Up"));
                directionalThrusters.Add(new DirectionalThrusters("Down"));
                directionalThrusters.Add(new DirectionalThrusters("Forward"));
                directionalThrusters.Add(new DirectionalThrusters("Backward"));
                directionalThrusters.Add(new DirectionalThrusters("Left"));
                directionalThrusters.Add(new DirectionalThrusters("Right"));
            }

            private void GetThrust()
            {
                List<IMyThrust> thrusters = new List<IMyThrust>();
                if (!string.IsNullOrWhiteSpace(group))
                {
                    try
                    {
                        myGP.GridTerminalSystem.GetBlockGroupWithName(group).GetBlocksOfType<IMyThrust>(thrusters);

                    }
                    catch (Exception)
                    { myGP.Echo($"Group {group} doesn't exist."); }
                }
                else
                {
                    try { myGP.GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters); }
                    catch (Exception) { myGP.Echo("No thrusters detected."); }
                }

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

                    foreach (DirectionalThrusters directionalThruster in directionalThrusters)
                    {
                        if (directionalThruster.DirectionName == direction)
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
