using UnityEngine;

public class BulletDanger : MonoBehaviour, IAITankDanger
{
    [SerializeField] private int team = 1; // 1=玩家, 0=敵人
    
    public Vector3 Position => transform.position;
    public int Team => team;
}