using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(TimerManager))]
public class TileWorldManager : MonoBehaviour 
{
    public const int TEAM_NUMBER_PLAYER = 1;

    // singleton istance
    public static TileWorldManager instance { get; private set; }

    [SerializeField]
    private float tileSize = 1.0f;

    [SerializeField]
    private Vector2 worldSize;

    [SerializeField]
    private GameObject playerPrototype;

    [SerializeField]
    private DroppedItem droppedItemPrototype;

    // TODO: Make door procedural with the floor layout so they are spcially sensical
    [SerializeField]
    private DungeonDoor exitDoor;

    private float tileHalfSize;

    private PersistentDungeon persistentDungeon;

    // Actors in this world. Generally not very many, like 10 or so at most
    private List<GameObject> entities = new List<GameObject>();

    // The entity that is the player avatar
    private Actor currentPlayer;

    // Re-use enitites
    private ObjectPool entityPool;

    // keeps exits closed until all enemies are killed
    private bool exitsUnlocked;


    public float GetTileSize() { return tileSize; }
    public Vector2 GetWorldSize() { return worldSize; }

    public Actor GetLocalPlayer() { return currentPlayer; }


    public TileWorldManager()
    {
        instance = this;

        tileHalfSize = tileSize / 2.0f;
        entityPool = ObjectPool.intance;
    }
    
    protected void Start()
    {
        persistentDungeon = FindObjectOfType<PersistentDungeon>();
        persistentDungeon.EnteringRoom();

        GameObject obj = SpawnEntity(playerPrototype, new Vector2(3, 2));
        currentPlayer = obj.GetComponent<Actor>();
    }

    /// <summary>
    /// Transform the world position into tile position, truncating to int
    /// </summary>
    public Vector2 GetTilePosition(Vector3 worldPosition)
    {
        int tileX = Mathf.FloorToInt(worldPosition.x / tileSize);
        int tileY = Mathf.FloorToInt(worldPosition.y / tileSize);
        return new Vector2(tileX, tileY);
    }

    /// <summary>
    /// Transform the tile position into a world position (at z = 0.0)
    /// </summary>
    public Vector3 GetWorldPosition(Vector2 tilePosition)
    {
        return new Vector3(tilePosition.x * tileSize, tilePosition.y * tileSize, 0.0f);
    }

    /// <summary>
    /// Gets world space position of the center of the input tile location
    /// </summary>
    public Vector3 GetTileCenter(Vector2 tile)
    {
        Vector3 worldPosition = GetWorldPosition(tile);
        return worldPosition + new Vector3(tileHalfSize, tileHalfSize, 0.0f);
    }

    // Spawns the Gameobject as an entity on the tile
    public GameObject SpawnEntity(GameObject prototype, Vector2 tile)
    {
        GameObject obj = ObjectPool.GetActiveInitialized(prototype, tile * tileSize + new Vector2(tileHalfSize, tileHalfSize), Quaternion.identity);
        entities.Add(obj);

        return obj;
    }

    /// <summary>
    /// Removes the entity from teh ones to be considiered in the this world space
    /// </summary>
    public void RemoveEntitiy(GameObject entity)
    {
        entities.Remove(entity);

        // Check if all enemies are dead
        if(!exitsUnlocked)
        {
            bool allEnemiesDead = true;
            foreach(GameObject obj in entities)
            {
                Actor a = obj.GetComponent<Actor>();
                if(a != null && a.teamNumber > TEAM_NUMBER_PLAYER)
                {
                    allEnemiesDead = false;
                }
            }

            if(allEnemiesDead)
            {
                exitsUnlocked = true;
                OpenRoomExits();
            }
        }
    }

    /// <summary>
    /// Returns the entity at the tile position
    /// </summary>
    public GameObject GetEntityAtTile(Vector2 tilePos)
    {
        for(int i = 0; i < entities.Count; i++)
        {
            if((GetTilePosition(entities[i].transform.position) - tilePos).sqrMagnitude <= 0.0f)
            {
                return entities[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Crates a dropped item object from the input item at the location
    /// </summary>
    public void DropItemAtLocation(Item itemToDrop, Vector3 location)
    {
        DroppedItem worldDrop = Instantiate(droppedItemPrototype, location, Quaternion.identity);
        if(worldDrop != null)
        {
            worldDrop.DropItem(itemToDrop);
        }
    }

    // Called when all enemies spawned are killed
    public void OpenRoomExits()
    {
        Debug.Log("Room cleared!");
        // Enable the exit colliders
        exitDoor.UnlockDoor();
    }


    // TOOD: LoadNewRoom in a specific direction from current room reference: Some sort of global map
    public void LoadNewRoom(DungeonDoor triggeredDoor)
    {
        persistentDungeon.LeavingRoom();
    }

    /// <summary>
    /// Iterate over all entities, except the ignored one, if provided
    /// </summary>
    public IEnumerable<GameObject> AllEntities(GameObject ignore = null)
    {
        for(int i = 0; i < entities.Count; i++)
        {
            if(ignore == entities[i])
            {
                continue;
            }

            yield return entities[i];
        }
    }



}
