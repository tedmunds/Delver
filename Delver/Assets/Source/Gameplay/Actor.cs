using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
public class Actor : MonoBehaviour 
{
    public delegate void OnDamageCallback(float damageAmount);

    public enum Facing
    {
        Right,
        Left
    }

    [SerializeField] // human readeable name that is game applicable
    public string gameName;

    [SerializeField]
    public float baseHealth = 100.0f;
    
    [SerializeField]
    public int teamNumber = 0;

    [HideInInspector]
    public OnDamageCallback damageCallback;
    
    private Facing facing;
    
    protected float currentHealth;

    protected bool invincible;
    protected bool isDead;

    protected SpriteRenderer spriteRenderer;
    private TileWorldManager world;

    // Get / Set
    public bool IsInvincible() { return invincible; }
    public bool IsDead() { return isDead; }
    public void SetInvincible(bool newInvincible) { invincible = newInvincible; }
    public float GetHealthPercent() { return currentHealth / baseHealth; }
    public float GetCurrentHealth() { return currentHealth; }
    public float GetBaseHealth() { return baseHealth; }    

    public Facing GetFacing() { return facing; }


    protected virtual void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        invincible = false;
        currentHealth = baseHealth;
    }
    
    protected void Start()
	{
        world = TileWorldManager.instance;

    }
	
	protected virtual void Update()
	{
		
	}


    // Gets the offset vector from actor position to the apparent bottom of the sprite
    public Vector3 GetBaseOffset()
    {
        return new Vector3(0.0f, spriteRenderer.bounds.size.y / 2.0f, 0.0f);
    }


    // Apply the damage to the actor generical,y calling Died() when health hits 0
    public void TakeDamage(float baseDamage)
    {
        if(invincible || isDead)
        {
            return;
        }

        currentHealth = Mathf.Max(currentHealth - baseDamage, 0.0f);

        if(currentHealth <= 0.0f)
        {
            Died();
        }

        if(damageCallback != null)
        {
            damageCallback(baseDamage);
        }
    }

    public void Heal(float healAmount)
    {
        currentHealth = Mathf.Min(currentHealth + healAmount, GetBaseHealth());
    }


    protected void Died()
    {
        // for now just diable it
        DeactivateDelayed(0.0f);
        isDead = true;

        world.RemoveEntitiy(gameObject);
    }


    /**
     * Deactivates the actor gameobject after a certain amount of time
     */
    public void DeactivateDelayed(float delay)
    {
        StartCoroutine(Co_DeactivateDelayed(delay));
    }

    private IEnumerator Co_DeactivateDelayed(float delay)
    {
        for(float initiated = Time.time; Time.time - initiated < delay;)
        {
            yield return null;
        }

        gameObject.SetActive(false);
    }


    public void SetFacing(Facing newFacing)
    {
        if(newFacing != facing)
        {
            facing = newFacing;
            spriteRenderer.flipX = (facing == Facing.Left);
        }
    }

    public virtual void Reflect(Actor Reflector) { }

}
