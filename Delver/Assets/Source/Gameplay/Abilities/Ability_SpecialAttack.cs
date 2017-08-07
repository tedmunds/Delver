using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base for attacks that place peristent tiles in the world
/// </summary>
[CreateAssetMenu(menuName = "Abilities/SpecialAttack")]
public class Ability_SpecialAttack : Ability
{
    protected struct ColliderEntry
    {
        public WorldEffectCollider spawnedCollider;
        public float addedTime;
    }

    
    [SerializeField]
    protected WorldEffectCollider colliderType;

    /// <summary>
    /// Will spawn the colliders in groups moveing out from attack origin, based on this attacks specific pattern
    /// </summary>
    [SerializeField]
    protected int numLayers;

    [SerializeField]
    protected float dalayBetweenLayers;

    /// <summary>
    /// How long to maintain each collider
    /// </summary>
    [SerializeField]
    protected float colliderLifetime;

    [SerializeField]
    protected AttackPattern attackPattern;

    protected List<ColliderEntry> colliders = new List<ColliderEntry>();

    protected float lastLayerTime;
    protected int currentLayerNum;
    
    public override void AbilityStarted(Actor attacker, Vector3 position, Vector3 direction)
    {
        base.AbilityStarted(attacker, position, direction);

        // Spawn first layer immediately
        SpawnColliderLayer(0);
        lastLayerTime = Time.time;

        // Spawn the rest immediatly too
        if(dalayBetweenLayers <= 0.0f)
        {
            for(int i = 1; i < numLayers; i++)
            {
                SpawnColliderLayer(i);
            }
        }
    }

    public override void UpdateAbility(Actor attacker)
    {
        base.UpdateAbility(attacker);
        
        // Check if a new layer of colliders should be spawned
        if(dalayBetweenLayers > 0.0f && Time.time - lastLayerTime > dalayBetweenLayers && currentLayerNum < numLayers - 1)
        {
            lastLayerTime = Time.time;
            currentLayerNum += 1;

            SpawnColliderLayer(currentLayerNum);
        }

        // Remove stale colliders, unless lifetime is <= 0.0, in which case they last until this ability ends
        if(colliderLifetime > 0.0f)
        {
            for(int i = colliders.Count - 1; i >= 0; i--)
            {
                ColliderEntry entry = colliders[i];
                if(Time.time - entry.addedTime > colliderLifetime)
                {
                    entry.spawnedCollider.gameObject.SetActive(false);
                    colliders.RemoveAt(i);
                }
            }
        }
    }

    public override void AbilityEnded(Actor attacker)
    {
        base.AbilityEnded(attacker);
        
        foreach(ColliderEntry entry in colliders)
        {
            entry.spawnedCollider.gameObject.SetActive(false);
        }

        colliders.Clear();
    }


    protected void SpawnColliderLayer(int layer)
    {
        // Grab all the locations to use, determined by subclasses
        Vector3[] spawnLocations = attackPattern.GetColliderSpawnLocations(this, colliderType.GetComponent<BoxCollider2D>().size, layer);
        foreach(Vector3 loc in spawnLocations)
        {
            ColliderEntry newEntry = new ColliderEntry()
            {
                spawnedCollider = ObjectPool.GetActive(colliderType, loc, Quaternion.identity),
                addedTime = Time.time
            };

            newEntry.spawnedCollider.InitFromAttack(abilityUser, OnColliderEnter, OnColliderExit, ValidateHit, activatedDirection, colliderSpeed);
            colliders.Add(newEntry);
        }
    }

   

    protected virtual void OnColliderEnter(Actor hit)
    {

    }

    protected virtual void OnColliderExit(Actor hit)
    {

    }

    protected virtual bool ValidateHit(Actor hit)
    {
        return abilityUser.teamNumber != hit.teamNumber;
    }
}