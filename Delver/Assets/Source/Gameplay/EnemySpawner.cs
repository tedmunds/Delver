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
    
    [SerializeField]
    private HealthBar healthBarPrototype;

    private TileWorldManager world;

    protected void Start()
	{
        world = GetComponent<TileWorldManager>();
        
        // Spawn the group
        // TODO: spawn group in different orientations, besed on which wall the player spawned on (from door)
        foreach(GroupMember member in spawnGroup.groupMembers)
        {
            Vector2 tilePos = originPos + member.spawnOffset;
            GameObject enemy = world.SpawnEntity(member.enemyToSpawn.gameObject, tilePos);
            Actor spawnedActor = enemy.GetComponent<Actor>();
            if(spawnedActor != null)
            {
                HealthBar healthBar = Instantiate(healthBarPrototype);
                healthBar.AttachToTarget(spawnedActor);
            }
        }
    }

	protected void Update()
	{
		
	}

}