using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Storage container for players items, with some specifics about weapons and health potions etc.
 */
[RequireComponent(typeof(PlayerControllerSmooth))]
public class Inventory : MonoBehaviour
{
    // The weapon that the inventory will start equipped with
    [SerializeField]
    private Weapon defaultWeapon;

    // Cache ref to the player this inventory is associated with
    private PlayerControllerSmooth playerOwner;

    public Weapon equippedWeapon { get; private set; }
    
    protected void Start()
    {
        playerOwner = GetComponent<PlayerControllerSmooth>();

        // Equip the weapon through normal channel so proper modifiers get applied etc.
        EquipWeapon(defaultWeapon);
    }

    /// <summary>
    /// Serts the wepaon as the main weapon
    /// </summary>
    public void EquipWeapon(Weapon newWeapon)
    {
        if(equippedWeapon != null)
        {
            // For now just drop the old weapon
            playerOwner.WeaponUnEquipped(equippedWeapon);

            playerOwner.world.DropItemAtLocation(equippedWeapon, transform.position);
        }

        equippedWeapon = newWeapon;
        playerOwner.WeaponEquipped(equippedWeapon);
    }

    /// <summary>
    /// Gets the attack at the index from the weapons base attack list, returns null if the weapon doesnt have the combo index
    /// </summary>
    public Ability GetBaseAttack(int comboIndex = 0)
    {
        if(equippedWeapon != null && equippedWeapon.baseAttackCombo.Length > 0 && comboIndex < equippedWeapon.baseAttackCombo.Length)
        {
            return equippedWeapon.baseAttackCombo[comboIndex];
        }

        return null;
    }
    
    /// <summary>
    /// Equipped weapons special attack
    /// </summary>
    public Ability GetSpecialAttack()
    {
        if(equippedWeapon != null)
        {
            return equippedWeapon.specialAttack;
        }

        return null;
    }

    /// <summary>
    /// The nubmer of abilities in the equipped weapons combo list
    /// </summary>
    public int WeaponComboLength()
    {
        if(equippedWeapon != null)
        {
            return equippedWeapon.baseAttackCombo.Length;
        }

        return -1;
    }

}