using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SimpleVolumeManager : MonoBehaviour
{
    public static SimpleVolumeManager instance;
    public Volume volume;

    void Awake()
    {
        instance = this;
        volume = GetComponent<Volume>();
    }

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }
}
