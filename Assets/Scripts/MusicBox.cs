using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicBox : Interactor
{
    public SoundName songA;
    public SoundName songB;
    public override void Interact()
    {
        AudioManager.Instance.PlaySong(songA, songB);
    }


    // Update is called once per frame
    private void Update()
    {
        if (isInteractable)
        {
            highlightFX.gameObject.transform.Rotate(Vector3.forward, 150 * Time.deltaTime);
        }
    }
}