using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class AttackCollider : MonoBehaviour
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
    

    protected void OnEnable()
    {
        hitsThisLifetime.Clear();
        overlapsThisFrame = null;
    }


	protected virtual void Update()
	{
        float angle = transform.rotation.eulerAngles.z;
        overlapsThisFrame = Physics2D.OverlapBoxAll(transform.position, boxDefinition.size, angle, detectLayers);
        foreach(Collider2D overlap in overlapsThisFrame)
        {
            // Only hit each thin once
            Actor hit = overlap.gameObject.GetComponent<Actor>();
            if (hit != null && 
                hit != owner && 
                !hitsThisLifetime.Contains(hit) &&
                ShouldOverlapTarget(hit))
            {
                hitsThisLifetime.Add(hit);
                onOverlap(hit);
            }
        }
	}

    /// <summary>
    /// Call to register a callback to receive overlap events to a specific function
    /// </summary>
    public void InitFromAttack(Actor owner, OverlapEvent callback, CanHitTarget targetValidation)
    {
        boxDefinition = GetComponent<BoxCollider2D>();

        onOverlap = callback;
        validateTarget = targetValidation;
        this.owner = owner;
    }


    private bool ShouldOverlapTarget(Actor target)
    {
        if(validateTarget != null)
        {
            return validateTarget(target);
        }

        return true;
    }

}