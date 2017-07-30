using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TileWorldManager))]
public class EnemySpawner : MonoBehaviour
{

    [SerializeField]
    private EnemyGroup spawnGroup;

    [SerializeField]
    private  Vector2 originPos;

    private TileWorldManager world;

    protected void Start()
	{
        world = GetComponent<TileWorldManager>();
        
        // Spawn the group
        // TODO: spawn group in different orientations, besed on which wall the player spawned on (from door)
        foreach(GroupMember member in spawnGroup.groupMembers)
        {
            Vector2 tilePos = originPos + member.spawnOffset;
            world.SpawnEntity(member.enemyToSpawn.gameObject, tilePos);
        }
    }

	protected void Update()
	{
		
	}

}