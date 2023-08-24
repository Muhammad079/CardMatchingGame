using System;
using UnityEngine;
using UnityEngine.UI;
using Scripts.GamePlay;
using Scripts.Managers;
using System.Collections;

namespace Scripts.DataContainersAndUI
{
    public class CardDisplay : MonoBehaviour
    {
        [SerializeField]
        public CardData CardData;
        public Card CardDetails;

        public float CardAnimationTime;
        public string CardID;
        public enum CardFace { faceup, facedown }
        public CardFace cardFace;

        public enum Selection { Selected, NotSelected }
        public Selection SelectionStatus;

        private Image _cardImage;

        private Sprite _cardFaceDown;
        private Sprite _cardFaceUp;

        private bool _FaceUp = false;
        private bool isRotating;
        private void OnEnable()
        {
            SetupCard();
            RectTransform rect = GetComponent<RectTransform>();
            rect.sizeDelta = Vector2.zero;
            rect.anchorMax = new Vector2(1, 1);
            rect.anchorMin = new Vector2(0, 0);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMax = Vector2.zero;
            rect.offsetMin = Vector2.zero;
        }

        public void SetupCard()
        {
            if (_cardImage == null)
                _cardImage = transform.Find("Image").GetComponent<Image>();
            if (CardDetails != null)
            {
                _cardFaceDown = CardDetails.FaceDown;
                _cardFaceUp = CardDetails.FaceUp;
                CardID = CardDetails.CardID;
                CardData.CardID = CardDetails.CardID;
            }
        }
        Coroutine coroutine;

        public void OnClick()
        {
            _FaceUp = !_FaceUp;
            if (!isRotating)
            {
                coroutine = StartCoroutine(RotateThisCard(_FaceUp, () =>
                {
                    AudioManager.Instance.PlayInstantMusic(CardDetails.FlipAudio);
                    SetCardStatus(_FaceUp, () =>
                    {
                        if (cardFace == CardFace.faceup)
                        {
                            GameManager.Instance.CardFlippedInvoke(this);
                        }
                        else
                        {
                            GameManager.Instance.ClearCards();
                        }
                        StopCoroutine(coroutine);
                    });
                }));
            }
        }

        public void SetCardStatus(bool _faceUp, Action StatusFinalized = null)
        {
            cardFace = _faceUp ? CardFace.faceup : CardFace.facedown;
            SelectionStatus = _faceUp ? Selection.Selected : Selection.NotSelected;
            _FaceUp = _faceUp;
            StatusFinalized?.Invoke();
        }

        public IEnumerator ShowCardsForTime(float time)
        {
            SetCardFaceUp(true);
            yield return new WaitForSeconds(time);
            SetCardFaceUp(false);
        }
        public void SetCardFaceUp(bool status, Action FacedUp = null)
        {
            if (!isRotating)
            {
                StartCoroutine(RotateThisCard(status, FacedUp));
            }
        }

        private IEnumerator RotateThisCard(bool faceUp, Action RotationCompleted = null)
        {
            isRotating = true;
            for (int i = 0; i <= 90; i += 10)
            {
                transform.rotation = Quaternion.Euler(0, i, 0);
                if (i == 90)
                {
                    _cardImage.sprite = faceUp ? _cardFaceUp : _cardFaceDown;
                    yield return new WaitForSeconds(CardAnimationTime);
                }
            }
            for (int i = 90; i >= 0; i -= 10)
            {
                transform.rotation = Quaternion.Euler(0, i, 0);
                yield return new WaitForSeconds(CardAnimationTime);
            }
            isRotating = false;
            RotationCompleted?.Invoke();
        }
    }
    [Serializable]
    public class CardData
    {
        public string CardID;
    }
}