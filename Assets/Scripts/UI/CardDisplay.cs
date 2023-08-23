using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    public Card CardDetails;

    private Image _cardImage;
    
    private Sprite _cardFaceDown;
    private Sprite _cardFaceUp;

    private bool _faceUp;
    private void Awake()
    {
        if(_cardImage == null)
            _cardImage = GetComponent<Image>();
        if(CardDetails != null)
        {
            _cardFaceDown = CardDetails.FaceDown;
            _cardFaceUp = CardDetails.FaceUp;
        }
    }

    public void OnClick()
    {
        Debug.LogError("On mouse down");
        _faceUp = !_faceUp;
        StartCoroutine(RotateThisCard(_faceUp));
    }

    private IEnumerator RotateThisCard(bool faceUp)
    {
        for (int i = 0; i <= 90; i += 10)
        {
            transform.rotation = Quaternion.Euler(0, i, 0);
            if(i == 90)
            {
                _cardImage.sprite = faceUp ? _cardFaceDown : _cardFaceUp;
                yield return new WaitForEndOfFrame();
            }
        }
        for (int i = 90; i >= 0; i-=10)
        {
            transform.rotation = Quaternion.Euler(0, i, 0);
            yield return new WaitForEndOfFrame();
        }
    }
}
