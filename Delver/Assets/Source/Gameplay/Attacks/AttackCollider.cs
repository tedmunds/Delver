using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class AttackCollider : MonoBehaviour
{
    public delegate void BeginOverlapDetect(Actor actor);
    public delegate bool CanHitTarget(Actor target);

    private BoxCollider2D boxDefinition;
    private BeginOverlapDetect onOverlap;
    private CanHitTarget validateTarget;

    [SerializeField]
    private LayerMask detectLayers;

    private List<Actor> hitsThisLifetime = new List<Actor>();
    private Actor owner;

    protected void Start()
	{
        boxDefinition = GetComponent<BoxCollider2D>();
    }

    protected void OnEnable()
    {
        hitsThisLifetime.Clear();
    }


	protected void Update()
	{
        float angle = transform.rotation.eulerAngles.z;
        Collider2D[] overlaps = Physics2D.OverlapBoxAll(transform.position, boxDefinition.size, angle, detectLayers);
        foreach(Collider2D overlap in overlaps)
        {
            // Only hit each thin once
            Actor hit = overlap.gameObject.GetComponent<Actor>();
            if (hit != null && 
                hit != owner && 
                !hitsThisLifetime.Contains(hit) &&
                OverlapTarget(hit))
            {
                hitsThisLifetime.Add(hit);
                onOverlap(hit);
            }
        }
	}

    /// <summary>
    /// Call to register a callback to receive overlap events to a specific function
    /// </summary>
    public void InitFromAttack(Actor owner, BeginOverlapDetect callback, CanHitTarget targetValidation)
    {
        onOverlap = callback;
        validateTarget = targetValidation;
        this.owner = owner;
    }


    private bool OverlapTarget(Actor target)
    {
        if(validateTarget != null)
        {
            return validateTarget(target);
        }

        return true;
    }

}