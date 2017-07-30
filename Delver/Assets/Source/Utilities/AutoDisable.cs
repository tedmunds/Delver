using System.Collections;
using UnityEngine;

public class AutoDisable : MonoBehaviour {

    [SerializeField]
    public float lifeTime = 0.0f;

	protected void OnEnable() {
        StartCoroutine(Timer());
	}
    
    public IEnumerator Timer() {
        float startTime = Time.time;
        for(; Time.time - startTime < lifeTime;) {
            yield return new WaitForSeconds(lifeTime);
        }

        gameObject.SetActive(false);
    }

}