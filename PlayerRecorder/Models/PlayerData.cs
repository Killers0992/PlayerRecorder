using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Models
{
    public class PlayerData : InvokeEvent
    {
        public string UserName { get; set; }
        public string UserID { get; set; }
        public RoleType Role { get; set; } = RoleType.None;
        public Vector2Data Speed { get; set; }
        public int CurAnim { get; set; }
        public PlayerMovementState MoveState { get; set; } = PlayerMovementState.Walking;
        public ItemType HoldingItem { get; set; } = ItemType.None;
        public Vector3Data Position { get; set; }
        public Vector2Data Rotation { get; set; }
    }
}
