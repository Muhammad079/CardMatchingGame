using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
    private Button Restart;
    
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

        Restart.onClick.AddListener(() => RestartTheGame());
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

    internal void ClearCards()
    {
        Cards.Clear();
        clickCount = 0;
    }

    private void OnCardFlipped(CardDisplay card)
    {
        clickCount++;
        Debug.LogError(clickCount);
        if(!Cards.Contains(card) && card.SelectionStatus == CardDisplay.Selection.Selected)
        {
            Debug.LogError("inside if");
            Cards.Add(card);
        }
        else
        {
            Cards.Clear();
            clickCount = 0;
            Debug.Log("Clearing the list");
        }
        if(clickCount % 2 == 0 && Cards.Count == 2)
        {
            Debug.LogError("inside if");
            Stats.Instance.InvokeTurn(TurnCalculator.Instance.GetCurrentTurn().ToString());
            StartCoroutine(TestTheCards());
        }
    }

    private IEnumerator TestTheCards()
    {
        Debug.LogError("Testing the cards");
        yield return new WaitForSeconds(_timeToCompareResult);
        if (Cards[0].CardID == Cards[1].CardID && Cards.Count == 2)
        {
            Cards.ForEach(x => x.gameObject.SetActive(false));
            Stats.Instance.InvokeScore(ScoreCalculator.Instance.GetScore ().ToString());
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
        StartCoroutine(RestartingGame());
    }

    private IEnumerator RestartingGame()
    {
        Stats.Instance.InvokeScore(ScoreCalculator.Instance.ResetScore().ToString());
        Stats.Instance.InvokeTurn(TurnCalculator.Instance.ResetTurns().ToString());
        GamePlayCards.ToList().ForEach(x => x.gameObject.SetActive(false));
        Cards.Clear();
        _CardHolders.ToList().ForEach(x => x.DetachChildren());
        yield return new WaitForSeconds(5);
        Shuffle(GamePlayCards,
            () =>
            {
                Debug.Log("Shuffled");
            });

        for (int i = 0; i < GamePlayCards.Count; i++)
        {
            GamePlayCards[i].transform.SetParent(_CardHolders[i]);
        }
        GamePlayCards.ForEach(x =>
        {
            x.SetActive(true);
            x.GetComponent<CardDisplay>().SetupCard();
            StartCoroutine(x.GetComponent<CardDisplay>().ShowCardsForTime(_timeToShowCards));
            x.GetComponent<CardDisplay>().SetCardFaceUp(false);
        });
    }
}