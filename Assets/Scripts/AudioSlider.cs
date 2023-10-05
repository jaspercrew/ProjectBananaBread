using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour
{
    public Slider slider;
    public static AudioSlider instance;

    // Start is called before the first frame update
    void Awake()
    {
        slider = GetComponent<Slider>();
        instance = this;
    }

    // Update is called once per frame
    void Update() { }

    public void UpdateVolume()
    {
        AudioManager.Instance.UpdateVolume(slider.value);
    }
}
