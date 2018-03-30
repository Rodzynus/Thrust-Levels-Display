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
        // Class for grouping thrusters for each direction.
        public class DirectionalThrusters
        {
            public Directions Direction { get; private set; }
            public float CurrentThrust { get; private set; }
            public float MaxEffectiveThrust { get; private set; }

            public DirectionalThrusters(Directions name)
            {
                Direction = name;
                CurrentThrust = 0;
                MaxEffectiveThrust = 0;
            }

            public float Percentage()
            {
                return (CurrentThrust / MaxEffectiveThrust) * 100;
            }

            // Iterates through all thrusters in list and fills up list of directions with values.
            public static void GetThrust(List<IMyThrust> thrusters, ref List<DirectionalThrusters> directionalThrusters)
            {
                foreach (IMyThrust thruster in thrusters)
                {
                    // This returns direction of the nozzle. If statements are inverted to get direction of thrust instead.
                    Directions thrusterDirection = Directions.Up;   // Defaults to Up just to satisfy the compiler. Will be overritten with actual value.
                    string nozzleDirection = VRageMath.Vector3I.GetDominantDirection(thruster.GridThrustDirection).ToString();
                    switch (nozzleDirection)
                    {
                        case "Up":
                            thrusterDirection = Directions.Down; break;
                        case "Down":
                            thrusterDirection = Directions.Up; break;
                        case "Forward":
                            thrusterDirection = Directions.Backward; break;
                        case "Backward":
                            thrusterDirection = Directions.Forward; break;
                        case "Left":
                            thrusterDirection = Directions.Right; break;
                        case "Right":
                            thrusterDirection = Directions.Left; break;
                    }

                    foreach(DirectionalThrusters directionalThruster in directionalThrusters)
                    if (directionalThruster.Direction.Equals(thrusterDirection))
                    {
                        directionalThruster.CurrentThrust += thruster.CurrentThrust;
                        directionalThruster.MaxEffectiveThrust += thruster.MaxEffectiveThrust;
                    }
                }
            }
        }
    }
}
