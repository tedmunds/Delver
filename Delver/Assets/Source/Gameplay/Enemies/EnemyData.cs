using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "EnemyData")]
public class EnemyData : ScriptableObject
{
    [SerializeField]
    public float baseDamage;

    [SerializeField]
    public float engageRange;

    [SerializeField]
    public float telegraphTime;

}