using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Modifiers/BaseModifier")]
public class BaseModifier : ScriptableObject
{

    [SerializeField]
    public float value;

    [SerializeField]
    public float lifetime = -1;

    [SerializeField]
    public string tag;

    [SerializeField]
    public ModifierTarget target;
   
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public ModifierContainer.Modifier getModifier()
    {
        return ModifierContainer.MakeModifier(tag, value, lifetime);
    }
}
