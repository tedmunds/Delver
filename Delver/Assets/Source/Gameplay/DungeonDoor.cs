using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DungeonDoor : MonoBehaviour
{

    private bool isOpen = false;
    private bool hasBeenEntered = false;


    public void UnlockDoor()
    {
        isOpen = true;

        // TODO: animation 
        GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 1);
    }



    public void OnTriggerEnter2D(Collider2D other)
    {
        if(isOpen && !hasBeenEntered)
        {
            hasBeenEntered = true;

            // TODO: open a button promt? or just open the corresponding new room
            TileWorldManager world = FindObjectOfType<TileWorldManager>();
            if(world != null)
            {
                world.LoadNewRoom(this);
            }
        }
    }

}