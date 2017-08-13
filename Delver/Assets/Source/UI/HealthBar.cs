using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private Vector3 offset;

    /// <summary>
    /// How long to stay visible when owner takes damage
    /// </summary>
    [SerializeField]
    private float onDamageVisiblity = 1.0f;

    // The actor this health bar is attached to
    private Actor owner;
    private float lastKnownHealth;

    private SpriteRenderer sprite;
    private Color baseColor;
    private float fullWidth;

    public void AttachToTarget(Actor owner)
    {
        this.owner = owner;
        lastKnownHealth = owner.GetCurrentHealth();
        sprite = GetComponent<SpriteRenderer>();
        baseColor = sprite.color;
        sprite.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.0f);
        fullWidth = sprite.size.x;
    }


    protected void Update()
    {
        if(owner != null && !owner.IsDead())
        {
            // Put the health bar into position for the target actor
            transform.position = owner.transform.position + offset;

            float currentHealth = owner.GetCurrentHealth();
            if(lastKnownHealth != currentHealth)
            {
                lastKnownHealth = currentHealth;
                float baseHealth = owner.GetBaseHealth();

                // Assign the new health value
                float healthVal = Mathf.Clamp(currentHealth / baseHealth, 0.0f, 1.0f);

                sprite.size = new Vector2(fullWidth * healthVal, sprite.size.y);

                // Draw and fade out
                OnHealthChanged();
            }
        }
        else
        {
            // If the enemy has died
            Destroy(gameObject);
        }
    }

    protected void OnHealthChanged()
    {
        sprite.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1.0f);

        System.Action resetColor = delegate ()
        {
            if(sprite != null)
            {
                sprite.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.0f);
            }
        };

        TimerManager.SetTimer(resetColor, onDamageVisiblity);
    }
}