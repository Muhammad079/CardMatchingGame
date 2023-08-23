using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Card", menuName ="New Card")]
public class Card : ScriptableObject
{
    public string CardName;
    public Sprite FaceUp;
    public Sprite FaceDown;

}
