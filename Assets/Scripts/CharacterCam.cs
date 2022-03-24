using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CharacterCam : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.GetComponent<CinemachineVirtualCamera>().Follow = FindObjectOfType<CharController>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
