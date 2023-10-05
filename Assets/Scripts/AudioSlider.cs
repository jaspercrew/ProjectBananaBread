using UnityEngine;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour
{
    public static AudioSlider instance;
    public Slider slider;

    // Start is called before the first frame update
    private void Awake()
    {
        slider = GetComponent<Slider>();
        instance = this;
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void UpdateVolume()
    {
        AudioManager.instance.UpdateVolume(slider.value);
    }
}