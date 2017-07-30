//#define DEBUG_DRAW

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TileMover : MonoBehaviour
{
    private const float snapTolerance = 0.01f;

    // Cache reference to the tile world for easy access
    private TileWorldManager world;

    // The tile the mover is moving to / standing on
    private Vector2 targetTile;

    //private Vector3 velocity;
    private float moveSpeed;

    // Indicates that it is not lerping to a target tile
    private bool isAtRest;


    public TileWorldManager GetWorld()
    {
        return world;
    }


    protected void Start()
	{
        world = FindObjectOfType<TileWorldManager>();

        // Start by jumping to the nearest tile
        targetTile = world.GetTilePosition(transform.position);
        moveSpeed = 10.0f;
    }
	
	protected void Update()
	{
		if(!IsAtTargetTile())
        {
            isAtRest = false;
            // Lerp towards the target at the current speed
            Vector3 currentPos = transform.position;
            Vector3 targetPos = world.GetTileCenter(targetTile);

            Vector3 toTarget = (targetPos - currentPos).normalized;

            Vector3 delta = (toTarget * moveSpeed) * Time.deltaTime;
            Vector3 resolvedPos = currentPos + delta;

            Vector3 resolvedXComponent = currentPos + new Vector3(delta.x, 0.0f, 0.0f);
            Vector3 resolvedYComponent = currentPos + new Vector3(0.0f, delta.y, 0.0f);

            Vector3 targetXComponent = currentPos + new Vector3((targetPos - currentPos).x, 0.0f, 0.0f);
            Vector3 targetYComponent = currentPos + new Vector3(0.0f, (targetPos - currentPos).y, 0.0f);
            
            // Check if the resolved pos is past the target (ie. the direction to the target has flipped after adding delta)
            float resolvedChangeIndirection = Vector3.Dot((targetXComponent - currentPos).normalized, (targetXComponent - resolvedXComponent).normalized);
            if(resolvedChangeIndirection < 0.0f)
            {
                resolvedPos.x = targetPos.x;
            }

            // Do the same for the y component
            resolvedChangeIndirection = Vector3.Dot((targetYComponent - currentPos).normalized, (targetYComponent - resolvedYComponent).normalized);
            if(resolvedChangeIndirection < 0.0f)
            {
                resolvedPos.y = targetPos.y;
            }

            transform.position = resolvedPos;
        }
        else
        {
            // need to snap to the tile, since we are within snapping tolerence but are not at rest yet
            if(!isAtRest)
            {
                SnapToTargetTile(targetTile);
            }
        }

#if DEBUG_DRAW
        Debug.DrawLine(transform.position, world.GetTileCenter(targetTile), Color.green);
        Debug.DrawLine(world.GetWorldPosition(targetTile), world.GetTileCenter(targetTile), Color.blue);
#endif
    }

    /// <summary>
    /// Move the specified int number of tiles (starts a tween to the target)
    /// </summary>
    public void Move(Vector2 delta, float speed, bool forceMaxDistance = false)
    {
        moveSpeed = speed;

        // Do not change the tile if its no past the center of the current tile
        Vector3 toCenter = world.GetTileCenter(TilePosition()) - transform.position;
        if(toCenter.sqrMagnitude > 0.0f && !forceMaxDistance)
        {
            float pastCenter = Vector3.Dot(toCenter.normalized, delta.normalized);
            if(pastCenter > 0.0f)
            {
                delta = Vector2.zero;
            }
        }

        Vector3 toTarget = targetTile - TilePosition();
        float deltaX = GetTileDelta(delta.x);
        float deltaY = GetTileDelta(delta.y);

        // dont let a lack of input override a previously desired input
        deltaX = Mathf.Abs(deltaX) > 0.0f ? deltaX : toTarget.x;
        deltaY = Mathf.Abs(deltaY) > 0.0f ? deltaY : toTarget.y;
        
        Vector2 candidateTile = TilePosition() + new Vector2(deltaX, deltaY);
        GameObject obj = world.GetEntityAtTile(candidateTile);
        targetTile = (obj == null) ? candidateTile : targetTile;
    }
	
    private float GetTileDelta(float worldInputVal)
    {
        if(Mathf.Abs(worldInputVal) > 0.3f)
        {
            return Mathf.Sign(worldInputVal) * Mathf.CeilToInt(Mathf.Abs(worldInputVal));
        }

        return 0.0f;
    }

    /// <summary>
    /// This actors position in tile space
    /// </summary>
    public Vector2 TilePosition()
    {
        return world.GetTilePosition(transform.position);
    }

    /// <summary>
    /// Get sthe target tile this mover is either moving towards or standing on
    /// </summary>
    public Vector2 GetMoveTo()
    {
        return targetTile;
    }
    
    /// <summary>
    /// Returns true if the mover has arrived its destination (within the snap tolerence)
    /// </summary>
    protected bool IsAtTargetTile()
    {
        return (transform.position - world.GetTileCenter(targetTile)).sqrMagnitude < (snapTolerance * snapTolerance);
    }

    /// <summary>
    /// Sets the mover at rest, and places them in the center of the input tile
    /// </summary>
    public void SnapToTargetTile(Vector2 tile)
    {
        transform.position = world.GetTileCenter(tile);
        isAtRest = true;
    }

    /// <summary>
    /// returns a cardinal vector in the dominant axis of the input direction. Returns 0 if input is 0
    /// </summary>
    public Vector2 GetDominantDirection(Vector2 direction)
    {
        if(direction.sqrMagnitude == 0.0f)
        {
            return Vector2.zero;
        }
        
        if(Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return new Vector2(Mathf.Sign(direction.x), 0.0f);
        }
        else
        {
            return new Vector2(0.0f, Mathf.Sign(direction.y));
        }
    }

}