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
            public Directions direction;
            public float CurrentThrust;
            public float MaxEffectiveThrust;

            public DirectionalThrusters(Directions name)
            {
                direction = name;
                CurrentThrust = 0;
                MaxEffectiveThrust = 0;
            }

            public float Percentage()
            {
                return (CurrentThrust / MaxEffectiveThrust) * 100;
            }
        }
    }
}
