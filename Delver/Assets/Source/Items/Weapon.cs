using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Weapon")]
public class Weapon : Item
{
    [SerializeField]
    public Ability[] baseAttackCombo;

    [SerializeField]
    public Ability specialAttack;

    // TODO: various modifiers: attack speed, stamina regen, max health etc.
}