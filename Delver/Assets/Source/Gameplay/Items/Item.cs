using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Common data for all items: Items are not instantiated so they shouldnt contain any instanced state
 */ 
public abstract class Item : ScriptableObject
{
    [SerializeField]
    public string itemName;

}