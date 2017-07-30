using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct PersistentData
{
    public float playerHealth;
}



/// <summary>
/// Does not get destroyed on load, tracks what floor its on
/// </summary>
public class PersistentDungeon : MonoBehaviour
{

    [SerializeField]
    private string dungeonRoomSceneName;


    private PersistentData persistentData;

    private int currentFloor;

	protected void Awake()
	{
        DontDestroyOnLoad(this.gameObject);
	}

	protected void Update()
	{
		
	}


    // Called by world manager when floor is exited
    public void LeavingRoom()
    {
        currentFloor += 1;
        
        ObjectPool.Destroy();
        
        // TODO: set up info that will be used to init the scene in the next room
        SceneManager.LoadScene(dungeonRoomSceneName, LoadSceneMode.Single);
    }

    // Called from world manager on entering new room
    public void EnteringRoom()
    {
        // TODO: setup player health and stuff from persistent data
    }

}