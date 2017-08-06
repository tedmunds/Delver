using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Represents an item in the map that can be picked up and added to an inventory
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class DroppedItem : MonoBehaviour
{
    [SerializeField]
    public float interactRange = 1.0f;

    [SerializeField]
    public LayerMask detectLayers;

    public Item item { get; private set; }

    private float droppedTime;

    private List<Inventory> overlappingInv = new List<Inventory>();

    private SpriteRenderer spriteComponent;
    
    /// <summary>
    /// Call when this gameobject is instantiated to init this dropped item as storing the input item
    /// </summary>
    public void DropItem(Item droppedItem)
    {
        item = droppedItem;
        droppedTime = Time.time;
        overlappingInv.Clear();

        if(spriteComponent == null)
        {
            spriteComponent = GetComponent<SpriteRenderer>();
        }

        // TODO: set sprite image to the one specified by the item?
    }


    public void PickUpItem(Inventory pickedUpBy)
    {
        // TODO: make a more generic way of pickup, for now its only for weapons
        if(item is Weapon)
        {
            Weapon weap = (Weapon)item;
            pickedUpBy.EquipWeapon(weap);
        }

        // And kill after pickup
        Destroy(gameObject);
    }


    protected void Update()
    {
        Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, interactRange, detectLayers);
        foreach(Collider2D overlap in overlaps)
        {
            // Only track each interactor once
            Inventory interactor = overlap.gameObject.GetComponent<Inventory>();
            if(!overlappingInv.Contains(interactor))
            {
                overlappingInv.Add(interactor);

                // TODO: notify inv on overlap start / end so it can hide show pickup UI
            }
        }

        for(int i = overlappingInv.Count - 1; i >= 0; i--)
        {
            Inventory interactor = overlappingInv[i];

            bool stillOverlappingActor = false;
            for(int j = 0; j < overlaps.Length; j++)
            {
                if(overlaps[j].gameObject == interactor.gameObject)
                {
                    stillOverlappingActor = true;
                    break;
                }
            }

            if(!stillOverlappingActor)
            {
                // TODO: notify inv on end interaction rannge
                overlappingInv.RemoveAt(i);
            }
        }
    }


}