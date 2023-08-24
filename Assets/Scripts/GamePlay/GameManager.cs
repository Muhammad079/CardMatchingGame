using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private List<Card> _cardsData;
    private List<CardDisplay> Cards = new();

    [SerializeField]
    private CardDisplay _card;
    
    [SerializeField]
    private Transform[] _CardHolders;
   
    [SerializeField]
    private float _timeToCompareResult;
    
    [SerializeField]
    private int _timeToShowCards;
    private int clickCount;
    
    private string TempCardID;

    public List<GameObject> GamePlayCards;
    public Action<CardDisplay> CardFlipped;
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        Shuffle(_cardsData, () =>
        {
            InstantiateCardsRandomly();
        });
        CardFlipped += OnCardFlipped;
    }

    private void Shuffle<T>(List<T> list, Action Shuffled)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        Shuffled?.Invoke();
    }

    private void InstantiateCardsRandomly()
    {
        for (int i = 0; i < _cardsData.Count; i++)
        {
            Card item = _cardsData[i];
            CardDisplay card = Instantiate(_card, _CardHolders[i]);
            card.CardDetails = item;
            card.SetupCard();
            StartCoroutine(card.ShowCardsForTime(_timeToShowCards));
            GamePlayCards.Add(card.gameObject);
        }
    }

    public void CardFlippedInvoke(CardDisplay card)
    {
        CardFlipped?.Invoke(card);
    }



    private void OnCardFlipped(CardDisplay card)
    {
        clickCount++;
        if(!Cards.Contains(card) && card.SelectionStatus == CardDisplay.Selection.Selected)
        {
            Cards.Add(card);
        }
        if(clickCount % 2 == 0 && Cards.Count ==2)
        {
            Stats.Instance.InvokeTurn(TurnCalculator.Instance.GetCurrentTurn().ToString());
            StartCoroutine(TestTheCards());
        }
    }

    private IEnumerator TestTheCards()
    {
        yield return new WaitForSeconds(_timeToCompareResult);
        if (Cards[0].CardID == Cards[1].CardID)
        {
            Cards.ForEach(x => x.gameObject.SetActive(false));
            Stats.Instance.InvokeScore(ScoreCalculator.Instance.GetScore().ToString());
        }
        else
        {
            Cards.Where(go => go.gameObject.activeInHierarchy &&
            go.GetComponent<CardDisplay>().SelectionStatus == CardDisplay.Selection.Selected).ToList().ForEach(x =>
            {
                x.GetComponent<CardDisplay>().SetCardFaceUp(false, () => { x.GetComponent<CardDisplay>().SetCardStatus(false); });

            });
        }
        Cards.Clear();
        clickCount = 0;
    }

    public void RestartTheGame()
    {
        //Shuffle()
    }
}