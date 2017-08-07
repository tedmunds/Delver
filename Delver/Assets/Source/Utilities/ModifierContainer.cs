using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Represents the different values a modifier can target
/// </summary>
public enum ModifierTarget
{
    Empty,
    Health,
    MoveSpeed,
}

public class ModifierContainer
{
    
    private enum ModifierCombinationMode
    {
        Add,
        Multiply
    }

    // Tie different modifier targets to how they should be combined
    private static Dictionary<ModifierTarget, ModifierCombinationMode> combinationModes = new Dictionary<ModifierTarget, ModifierCombinationMode>()
    {
        { ModifierTarget.Empty,         ModifierCombinationMode.Add },
        { ModifierTarget.Health,        ModifierCombinationMode.Add },
        { ModifierTarget.MoveSpeed,     ModifierCombinationMode.Multiply },
    };



    /// <summary>
    /// Info representing a modifier to some property in game. Lifetime pf -1 means it must be manually removed (default setting)
    /// </summary>
    public struct Modifier
    {
        // The value to combine with other modfiers to get the total value
        public float modifierValue;

        // How long after it was added should it be automatically removed
        public float lifeTime;

        // When this modifier was added, used weith lifetime to auto remove it
        public float addedTime;

        // Used to easily compare modifiers, instead of full strings
        private int tag;

        // Init the modifier, with Time.time to indicate when it was initialy created
        public Modifier(float value, string modifierTag, float currentTime, float lifeTime = -1.0f)
        {
            modifierValue = value;
            tag = modifierTag.GetHashCode();
            addedTime = currentTime;
            this.lifeTime = lifeTime;           
        }
        
        public bool HasTag(int otherTag)
        {
            return tag == otherTag;
        }
    }

    // The set of modifiers, tied to their corresponding target
    private Dictionary<ModifierTarget, List<Modifier>> modifierMap = new Dictionary<ModifierTarget, List<Modifier>>();

    /// <summary>
    /// Creates a modifier struct with slightly fewer params than making it manually
    /// </summary>
    public static Modifier MakeModifier(string tag, float modifierValue, float lifeTime = -1.0f)
    {
        return new Modifier(modifierValue, tag, Time.time, lifeTime);
    }

    /// <summary>
    /// Call every frame, supplying Time.time, to remove modifiers that have expired their lifetimes
    /// </summary>
    public void UpdateModifiers(float currentTime)
    {
        // Remove expired modifiers
        foreach(var entry in modifierMap)
        {
            for(int i = entry.Value.Count - 1; i >= 0; i--)
            {
                // Past its lifetime
                if(entry.Value[i].lifeTime > 0.0f &&
                    entry.Value[i].addedTime + entry.Value[i].lifeTime < currentTime)
                {
                    entry.Value.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>
    /// Adds the modifier to the set, tied to teh specific target
    /// </summary>
    public void AddModifer(ModifierTarget target, Modifier newModifier)
    {
        List<Modifier> modSet;
        if(modifierMap.TryGetValue(target, out modSet))
        {
            modSet.Add(newModifier);
        }
        else
        {
            modSet = new List<Modifier>();
            modSet.Add(newModifier);
            modifierMap.Add(target, modSet);
        }
    }

    /// <summary>
    /// Removes all the modifers with the given tag
    /// </summary>
    public void RemoveModifier(ModifierTarget target, string modifierTag)
    {
        int targetTag = modifierTag.GetHashCode();
        List<Modifier> modSet;
        if(modifierMap.TryGetValue(target, out modSet))
        {
            for(int i = modSet.Count - 1; i >= 0; i--)
            {
                if(modSet[i].HasTag(targetTag))
                {
                    modSet.RemoveAt(i);
                }
            }
        }
    }


    /// <summary>
    /// Removes all the modifiers under the input target type
    /// </summary>
    public void RemoveAllModifiers(ModifierTarget target)
    {
        List<Modifier> modSet;
        if(modifierMap.TryGetValue(target, out modSet))
        {
            modSet.Clear();
        }
    }


    /// <summary>
    /// Gets the total value of the input modifier
    /// </summary>
    public float GetModifierValue(ModifierTarget target)
    {
        ModifierCombinationMode combineMode = combinationModes[target];

        float outValue = combineMode == ModifierCombinationMode.Add? 0.0f : 1.0f;
        
        List<Modifier> modSet;
        if(modifierMap.TryGetValue(target, out modSet))
        {
            for(int i = 0; i < modSet.Count; i++)
            {
                outValue = (combineMode == ModifierCombinationMode.Add) ? outValue + modSet[i].modifierValue : outValue * modSet[i].modifierValue;
            }
        }

        return outValue;
    }

}