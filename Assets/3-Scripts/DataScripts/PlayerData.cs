using UnityEngine;

[CreateAssetMenu]
public class PlayerData : ScriptableObject
{
    public float playerSpeed = 5;
    public float projectileSpeed = 10;
    public float projectileRange = 2;
    public int projectileType = 0;
}
