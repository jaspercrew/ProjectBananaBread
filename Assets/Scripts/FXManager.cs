using UnityEngine;

public class FXManager : MonoBehaviour
{
    public static FXManager instance;

    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = this;
    }
}