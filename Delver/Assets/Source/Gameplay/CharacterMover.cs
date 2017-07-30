using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Actor))]
public class CharacterMover : MonoBehaviour
{
	
	[SerializeField]
	protected float skinWidth = 0.2f;
	
	[SerializeField]
	protected float slopLimit = 0.9f;

    [SerializeField]
    protected float movementSpeed;

    [SerializeField]
    protected float acceleration;

    [SerializeField]
    protected LayerMask detectLayers;

    // The actor this mover is on
    protected Actor owner;
	
	protected float bounciness = 0.0f;
	protected float velocityMagIsZeroOnBounce = 0.5f;

	protected CircleCollider2D footCollider;
	protected Rigidbody2D rigid;
	
	protected Vector3 velocity;
	
	protected Vector3 oldPosition;
	
	protected float movementModifier = 1.0f;

    /** Should position be limited in a specific container */
    protected bool bContrainMovementInBorders = false;
    protected float xLeftLimit;
    protected float xRightLimit;
    protected float yTopLimit;
    protected float yBottomLimit;
	
	// game object were standing on
	protected GameObject standingBase;

    // Flag to temporarily disable movment collision
    protected bool bTempDisableCollision;
	
	// DEBUG
	private bool bDebugMove = false;

    public void SetMovementModifier(float n) { movementModifier = n; }
    public float GetMovementModifier() { return movementModifier; }
    public void SetMovemetCollision(bool newFlag) { bTempDisableCollision = !newFlag; }

    public float GetMoveSpeed() { return movementSpeed * movementModifier; }
    public Vector3 GetVelocity() { return velocity; }

	// Use this for initialization
	void Start ()
    {
		footCollider  = GetComponent<CircleCollider2D>();
		rigid = GetComponent<Rigidbody2D>();
		owner = GetComponent<Actor>();

        //WorldManager.instance.GetWorldBounds(ref xLeftLimit, ref xRightLimit, ref yTopLimit, ref yBottomLimit);
	}
	
	// Update is called once per frame
	void Update ()
    {
		standingBase = GetGround();

		Move(velocity);
	}

	public float xVel() { return velocity.x; }
	public float yVel() { return velocity.y; }

	/** Current speed as percentage of max */
	public float NormalizedVel()
    {
		return velocity.magnitude / movementSpeed;
	}

    /// <summary>
    /// Resets velocity and freezes the mover
    /// </summary>
    public void SetVelocityZero()
    {
        velocity = Vector3.zero;
    }

	
	// Does a walk move
	public void SetControlInput(Vector2 amplitude) {
		velocity.x = Mathf.Lerp(velocity.x, 									// from
		                        amplitude.x * movementSpeed * movementModifier, // to	
		                        Time.deltaTime * acceleration) ;				// delta

		velocity.y = Mathf.Lerp(velocity.y, 									// from
		                        amplitude.y * movementSpeed * movementModifier, // to	
		                        Time.deltaTime * acceleration) ;				// delta
	}
	
	
	// Does frame normalized movement
	public void Move(Vector3 vel)
    {
		Vector3 moveDelta = vel * Time.deltaTime;

        if(moveDelta.x != 0.0f && !bTempDisableCollision)
        {
			moveDelta = HorizontalMove(moveDelta);
		}

        if(moveDelta.y != 0.0f && !bTempDisableCollision)
        {
			moveDelta = VerticalMove(moveDelta);
		}

        if(moveDelta.sqrMagnitude > 0.0f)
        {
            transform.Translate(moveDelta);
        }
        
        // contrain within borders of world
        if(bContrainMovementInBorders)
        {
            if(transform.position.x < xLeftLimit)
            {
                transform.position = new Vector3(xLeftLimit, transform.position.y, transform.position.z);
            }
            if(transform.position.x > xRightLimit)
            {
                transform.position = new Vector3(xRightLimit, transform.position.y, transform.position.z);
            }

            if(transform.position.y > yTopLimit)
            {
                transform.position = new Vector3(transform.position.x, yTopLimit, transform.position.z);
            }
            if(transform.position.y - owner.GetBaseOffset().y < yBottomLimit)
            {
                transform.position = new Vector3(transform.position.x, yBottomLimit + owner.GetBaseOffset().y, transform.position.z);
            }
        }
	}


	// Forces some an instantanious force onto the character, forcing them to move
	public void ApplyHorizontalForce(float forceVel)
    {
		velocity.x = forceVel;
	}

	// Forces an instant force vertically
	public void ApplyVerticalForce(float forceVel)
    {
		velocity.y = forceVel;
	}
	
	
	// adjusts the input time normalized move to account for horizoontal collisions
	protected Vector3 HorizontalMove(Vector3 moveDelta)
    {
		// Get the top and bottom corners on the side that its moving
		Vector3 top = GetUpperCorner(moveDelta.x < 0.0f);
		Vector3 bottom = GetLowerCorner(moveDelta.x < 0.0f);
		
		Vector3 horzeDelta = moveDelta;
		horzeDelta.y = 0.0f;
		float horzRayCastDistance = horzeDelta.magnitude + skinWidth;
		
		// Check horizontal movement by casting rays in the direction of movement, plus skin width
		List<RaycastHit2D> raysToCheck = new List<RaycastHit2D>();
		raysToCheck.Add(Physics2D.Raycast(top, horzeDelta.normalized, horzRayCastDistance, detectLayers));
		raysToCheck.Add(Physics2D.Raycast(bottom, horzeDelta.normalized, horzRayCastDistance, detectLayers));
		
		if(bDebugMove)
        {
			Debug.DrawLine(top, top +  horzeDelta.normalized*horzRayCastDistance, Color.yellow, 0);
			Debug.DrawLine(bottom, bottom +  horzeDelta.normalized*horzRayCastDistance, Color.yellow, 0);
		}
		
		bool collided = false;
		int successfulHit = 0;
		
		// check the horizontal rays and limit horizontal velocity
		int idx = 0;
		foreach(RaycastHit2D ray in raysToCheck)
        {
			if(ray && !ray.collider.isTrigger)
            {
				moveDelta.x = (new Vector3(ray.point.x, ray.point.y, 0.0f) - bottom).x;

				moveDelta.x -= moveDelta.normalized.x * skinWidth;

				collided = true;
				successfulHit = idx;
			}

			idx++;
		}
		
		// Callback for collision
		if(collided)
        {
			OnHit(ref moveDelta, raysToCheck[successfulHit].normal, raysToCheck[successfulHit].point);
		}
		
		return moveDelta;
	}
	
	
	
	// adjusts the input time normalized move to account for vertical collisions and falling
	protected Vector3 VerticalMove(Vector3 moveDelta)
    {
		// do different cast depending on if were moving up or down
		
		Vector3 vertDelta = moveDelta;
		vertDelta.x = 0.0f;
		float vertRayCastDistance = vertDelta.magnitude + skinWidth;
		Vector3 left;
		Vector3 right;
		
		if(moveDelta.y < 0.0f)
        {
			// Moving down, main case
			left = GetLowerCorner(true);
			right = GetLowerCorner(false);
		}
		else
        {
			left = GetUpperCorner(true);
			right = GetUpperCorner(false);
		}
		
		// Check horizontal movement by casting rays in the direction of movement, plus skin width
		List<RaycastHit2D> raysToCheck = new List<RaycastHit2D>();
		raysToCheck.Add(Physics2D.Raycast(left, vertDelta.normalized, vertRayCastDistance, detectLayers));
		raysToCheck.Add(Physics2D.Raycast(right, vertDelta.normalized, vertRayCastDistance, detectLayers));
		
		if(bDebugMove)
        {
			Debug.DrawLine(left, left +  vertDelta.normalized*vertRayCastDistance, Color.blue, 0);
			Debug.DrawLine(right, right +  vertDelta.normalized*vertRayCastDistance, Color.blue, 0);
		}
		
		bool collided = false;
		int successfulHit = 0;
		
		// check the horizontal rays and limit horizontal velocity
		int idx = 0;
		foreach(RaycastHit2D ray in raysToCheck)
        {
			if(ray && !ray.collider.isTrigger)
            {
				moveDelta.y = (new Vector3(ray.point.x, ray.point.y, 0.0f) - left).y;

				moveDelta.y -= moveDelta.normalized.y * skinWidth;

				collided = true;
				successfulHit = idx;
			}
			idx++;
		}
		
		// Callback for collision
		if(collided)
        {
			OnHit(ref moveDelta, raysToCheck[successfulHit].normal, raysToCheck[successfulHit].point);
		}
		
		return moveDelta;
	}
	
	
	
	protected void OnHit(ref Vector3 vel, Vector3 normal, Vector3 point)
    {
		// Determine how flat the surfae is:
		float flatness = Mathf.Abs(Vector3.Dot(normal, Vector3.up));

		if(flatness > slopLimit)
        {
			// mostly vertical
			velocity.y = 0.0f;
		}
		else
        {
			// mostly horizontal surface
			velocity.x = 0.0f;
		}
		
	}
	

	// Get the standing base, set isOnGround
	protected GameObject GetGround()
    {
		// TODO: figure out what sort of thing its standing on so that you can travel of pltforms / boatts or whatever
		return null;
	}


	
	// Get the location of the top corner (left or right based on move left)
	public Vector3 GetUpperCorner(bool bMoveLeft)
    {
		float offsetDistX = footCollider.radius ;
		float offsetDistY = footCollider.radius;
		
		Vector3 center = transform.position + new Vector3(footCollider.offset.x, footCollider.offset.y, 0.0f);
		Vector3 toCorner = new Vector3(bMoveLeft? -offsetDistX : offsetDistX, offsetDistY, 0.0f);
		return center + toCorner;
	}
	
	// Get the location of the bottom corner (left or right based on move left)
	public Vector3 GetLowerCorner(bool bMoveLeft)
    {
		float offsetDistX = footCollider.radius;
		float offsetDistY = -footCollider.radius;

		Vector3 center = transform.position + new Vector3(footCollider.offset.x, footCollider.offset.y, 0.0f);
		Vector3 toCorner = new Vector3(bMoveLeft? -offsetDistX : offsetDistX, offsetDistY, 0.0f);
		return center + toCorner;
	}


	// Zero out all movement
	public void KillAllMovement()
    {
		velocity = Vector3.zero;
	}


	
}







