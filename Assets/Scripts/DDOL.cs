using UnityEngine;

// ReSharper disable once InconsistentNaming
public class DDOL : MonoBehaviour
{
    private static DDOL instance;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
