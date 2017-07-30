using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct GroupMember
{
    public Actor enemyToSpawn;
    public Vector2 spawnOffset;
}

/// <summary>
/// Specifies a group of enemies to spawn together, and where they should spawn relative to a common origin
/// </summary>
[CreateAssetMenu(menuName = "EnemyGroup")]
public class EnemyGroup : ScriptableObject
{
    [SerializeField]
    public string description;

    [SerializeField]
    public GroupMember[] groupMembers; 
}