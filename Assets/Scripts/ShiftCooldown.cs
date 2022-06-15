using UnityEngine;
using UnityEngine.UI;

public class ShiftCooldown : MonoBehaviour
{
    public Image image;
    private float cooldown;
    // Start is called before the first frame update
    private void Start()
    {
        image = transform.Find("Canvas").GetComponent<Image>();
        cooldown = CharController.ShiftCooldown;
        image.fillAmount = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        if (image.fillAmount > 0)
        {
            image.fillAmount -= Time.fixedDeltaTime / cooldown;
            if (image.fillAmount < 0)
            {
                image.fillAmount = 0;
            }
        }
        
    }
}
