// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// public class BeatBoost : BeatEntity
// {
//     public float boostForce = 20f;
//     private float horizontalMultiplier = .3f;
//     protected override void MicroBeatAction()
//     {
//         Boost();
//         base.MicroBeatAction();
//     }
//
//     private void Boost()
//     {
//         Vector2 boostVector = Vector2.zero;
//         if (Input.GetKey(KeyCode.A))
//         {
//             boostVector += Vector2.left * horizontalMultiplier;
//         }
//         if (Input.GetKey(KeyCode.S))
//         {
//             boostVector += Vector2.down;
//         }
//         if (Input.GetKey(KeyCode.D))
//         {
//             boostVector += Vector2.right * horizontalMultiplier;
//         }
//         if (Input.GetKey(KeyCode.W))
//         {
//             boostVector += Vector2.up;
//         }
//         boostVector *= boostForce;
//         print(boostVector);
//         GetComponent<Rigidbody2D>().AddForce(boostVector, ForceMode2D.Impulse);
//         CharController.Instance.recentlyImpulsed = true;
//
//     }
//
//     // Update is called once per frame
//     void Update()
//     {
//         
//     }
// }
