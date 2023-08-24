using System;
using UnityEngine;

[CreateAssetMenu(fileName ="Card", menuName ="New Card")]
[Serializable]
public class Card : ScriptableObject
{
    public string CardID;
    public Sprite FaceUp;
    public Sprite FaceDown;

    public AudioClip FlipAudio;
}
