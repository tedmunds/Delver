using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Update_Abilities/Homing")]
public class Homing_Ability : Update_Ability
{
    [SerializeField]
    public float minHomingDistance;

    [SerializeField]
    public float homingIntensity;

    private Vector3 homingDirection;

    private Actor closestActor = null;

    public override void UpdateAbility(Actor user, AttackCollider currentCollider)
    {
        if (currentCollider != null)
        {
            UpdateClosestHomingTarget(user, currentCollider);
            if (closestActor != null)
            {

                Home(currentCollider);
            }
        }
    }

    /// <summary>
    /// Move the attack collider closer to the closest valid target based on the homingIntensity
    /// </summary>
    private void Home(AttackCollider currentCollider)
    {
        if (Vector3.Distance(closestActor.transform.position, currentCollider.transform.position) <= minHomingDistance)
        {
          float step = homingIntensity * Time.deltaTime;
          currentCollider.transform.position = Vector3.MoveTowards(currentCollider.transform.position, closestActor.transform.position, step);
        }
    }

    /// <summary>
    /// Find the closest enemy in range (if any) to home to
    /// </summary>
    private void UpdateClosestHomingTarget(Actor user, AttackCollider currentCollider)
    {
        closestActor = null;
        List<GameObject> gameObjectList = TileWorldManager.instance.GetEntities();
        foreach (GameObject gObject in gameObjectList)
        {
            Actor actor = gObject.GetComponent("Actor") as Actor;
            if (actor != null)
            {
                if (actor.teamNumber != user.teamNumber && !(actor is AttackCollider))
                {
                    if (actor.IsDead() == false)
                    {
                        if (closestActor == null)
                        {
                            closestActor = actor;
                        }
                        else
                        {
                            if (Vector3.Distance(closestActor.transform.position, currentCollider.transform.position) > Vector3.Distance(actor.transform.position, currentCollider.transform.position))
                            {
                                closestActor = actor;
                            }
                        }
                    }
                }

            }

        }
        Debug.Log(closestActor);
    }

}
