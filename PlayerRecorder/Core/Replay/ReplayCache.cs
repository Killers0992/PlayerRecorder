using Interactables.Interobjects.DoorUtils;
using MapGeneration.Distributors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder.Core.Replay
{
    public class ReplayCache
    {
        public static Dictionary<string, Lift> ElevatorCache { get; set; } = new Dictionary<string, Lift>();
        public static Dictionary<Vector3, DoorVariant> DoorCache { get; set; } = new Dictionary<Vector3, DoorVariant>();
        public static Dictionary<Vector3, Scp079Generator> GeneratorCache { get; set; } = new Dictionary<Vector3, Scp079Generator>();

        public static void ClearCache()
        {
            ElevatorCache.Clear();
            DoorCache.Clear();
            GeneratorCache.Clear();
        }
    }
}
