using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    private PlayerControllerSmooth player;
    private Actor playerActor;

    [SerializeField]
    public Image healthProgressBar;

    [SerializeField]
    public Image staminaProgressBar;
    
    private RectTransform canvasTransform;
    
    protected void Start()
    {
        canvasTransform = (RectTransform)transform;
    }


	protected void Update ()
    {
		if(player == null)
        {
            // TODO: probably need a better way to get this, since the player will be spawned
            player = FindObjectOfType<PlayerControllerSmooth>();

            if(player != null)
            {
                playerActor = player.gameObject.GetComponent<Actor>();
            }
        }

        if(player != null && playerActor != null)
        {
            staminaProgressBar.fillAmount = player.GetEnergyPercent();
            healthProgressBar.fillAmount = playerActor.GetHealthPercent();
        }       
	}
    
}
