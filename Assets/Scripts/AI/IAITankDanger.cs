using UnityEngine;

public interface IAITankDanger
{
    Vector3 Position { get; }
    int Team { get; }
}

// Disabled duplicate interface from TanksRebirth
// #if false
// using Microsoft.Xna.Framework;
// namespace TanksRebirth.GameContent.Systems.AI;
// public interface IAITankDanger {
//     Vector2 Position { get; set; }
//     int Team { get; }
// }
// #endif