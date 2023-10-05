using UnityEngine;

// ReSharper disable once InconsistentNaming
public class DDOL : MonoBehaviour
{
    private static DDOL _instance;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }
}