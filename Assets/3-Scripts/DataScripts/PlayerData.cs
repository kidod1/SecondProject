using UnityEngine;

[CreateAssetMenu]
public class PlayerData : ScriptableObject
{
    public float playerSpeed = 5;
    public float knockbackDuration = 0.1f;
    public float knockbackSpeed = 5.0f;
    public float projectileSpeed = 10;
    public float projectileRange = 2;
    public int projectileType = 0;
    public int MaxHP = 20;
}
