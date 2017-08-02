using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AttackPatterns/AttackPattern_Cross")]
public class AttackPattern_Cross : AttackPattern
{
    public override Vector3[] GetColliderSpawnLocations(Ability user, Vector3 colliderBounds, int layer)
    {
        Vector3[] directions = new Vector3[] {
            new Vector3( 1.0f,  0.0f, 0.0f),
            new Vector3(-1.0f,  0.0f, 0.0f),
            new Vector3( 0.0f,  1.0f, 0.0f),
            new Vector3( 0.0f, -1.0f, 0.0f),
        };

        Vector3 origin = user.activatedPosition;

        Vector3[] outPositions = new Vector3[directions.Length];

        for(int i = 0; i < directions.Length; i++)
        {
            Vector3 dir = directions[i];

            float delta = Mathf.Abs(dir.x) > Mathf.Abs(dir.y) ? colliderBounds.x : colliderBounds.y;
            outPositions[i] = origin + (dir * (delta * (layer + 1)));
        }
        
        return outPositions;
    }
}