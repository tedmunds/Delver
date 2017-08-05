using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class AttackCollider : Actor
{
    public delegate void OverlapEvent(Actor actor);
    public delegate bool CanHitTarget(Actor target);

    protected BoxCollider2D boxDefinition;
    protected OverlapEvent onOverlap;
    protected CanHitTarget validateTarget;

    [SerializeField]
    protected LayerMask detectLayers;

    protected List<Actor> hitsThisLifetime = new List<Actor>();
    protected Actor owner;

    protected Collider2D[] overlapsThisFrame;

    public Vector3 GetBounds() { return boxDefinition.size; }

    public float speed = 0;
    public Vector3 activatedDirection { get; private set; }

    protected override void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        invincible = true;

        hitsThisLifetime.Clear();
        overlapsThisFrame = null;
    }


	protected override void Update()
	{
        float angle = transform.rotation.eulerAngles.z;
        overlapsThisFrame = Physics2D.OverlapBoxAll(transform.position, boxDefinition.size, angle, detectLayers);
        this.transform.position += this.activatedDirection * this.speed * Time.deltaTime;

        foreach (Collider2D overlap in overlapsThisFrame)
        {
            // Only hit each thing once
            Actor hit = overlap.gameObject.GetComponent<Actor>();
            if (hit != null && 
                hit.teamNumber != this.teamNumber && 
                !hitsThisLifetime.Contains(hit) &&
                ShouldOverlapTarget(hit))
            {
                hitsThisLifetime.Add(hit);
                onOverlap(hit);
            }
        }
	}

    /// <summary>
    /// Call to register a callback to receive overlap events to a specific function, set the collider's initial direction and speed
    /// </summary>
    public void InitFromAttack(Actor owner, OverlapEvent callback, CanHitTarget targetValidation, Vector3 activatedDirection, float speed)
    {
        boxDefinition = GetComponent<BoxCollider2D>();

        onOverlap = callback;
        validateTarget = targetValidation;
        this.owner = owner;
        this.activatedDirection = activatedDirection;
        this.speed = speed;
    }


    private bool ShouldOverlapTarget(Actor target)
    {
        if(validateTarget != null)
        {
            return validateTarget(target);
        }

        return true;
    }


    /// <summary>
    /// Reflect the current activated direction for the collider, give it a new team number for the "reflector" of the attack
    /// </summary>
    public override void Reflect(Actor reflector)
    {
        this.activatedDirection = Vector3.Reflect(this.activatedDirection, this.activatedDirection);
        this.teamNumber = reflector.teamNumber;
    }
}