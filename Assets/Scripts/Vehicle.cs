using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{

    public GameObject destroyedGFX;
    public GameObject normalGFX;
    public bool explode;
    bool routineStarted;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (explode == true) 
		{
			if (routineStarted == false) 
			{
				//Start the explode coroutine
				//StartCoroutine(Explode());
				routineStarted = true;
			} 
		}
    }

    // IEnumerator Explode ()
    // {
    //     yield return new WaitForSeconds(.02f);
    //     destroyedGFX.SetActive(true);
    //     normalGFX.SetActive(false);
        
        
    // }
}
