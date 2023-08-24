using Newtonsoft.Json;
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
    private List<GameObject> GamePlayCards = new();

    [SerializeField]
    private CardDisplay _card;
    
    [SerializeField]
    private GameObject LoadingScreen;
    
    [SerializeField]
    private Transform[] _CardHolders;
   
    [SerializeField]
    private float _timeToCompareResult;
    
    [SerializeField]
    private Button Restart, Save, Load;
    
    [SerializeField]
    private int _timeToShowCards;
    private int clickCount;

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
        Save.onClick.AddListener(() => SavePlayerState());
        Load.onClick.AddListener(() => LoadPlayerState());
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
        if(!Cards.Contains(card) && card.SelectionStatus == CardDisplay.Selection.Selected)
        {
            Cards.Add(card);
        }
        else
        {
            Cards.Clear();
            clickCount = 0;
        }
        if(clickCount % 2 == 0 && Cards.Count == 2)
        {
            Stats.Instance.InvokeTurn(TurnCalculator.Instance.GetCurrentTurn().ToString());
            StartCoroutine(TestTheCards());
        }
    }

    private IEnumerator TestTheCards()
    {
        yield return new WaitForSeconds(_timeToCompareResult);
        if (Cards[0].CardID == Cards[1].CardID && Cards.Count == 2)
        {
            Cards.ForEach(x => x.gameObject.SetActive(false));
            Stats.Instance.InvokeScore(ScoreCalculator.Instance.GetScore ().ToString());
            CheckIfGameCompleted();
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
        Restart.interactable = false;
        StartCoroutine(RestartingGame());
        DisplayLoadingPopUp(true);
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
                for (int i = 0; i < GamePlayCards.Count; i++)
                {
                    GamePlayCards[i].transform.SetParent(_CardHolders[i]);
                }
                GamePlayCards.Select(y=>y.GetComponent<CardDisplay>()).ToList().ForEach(x =>
                {
                    x.gameObject.SetActive(true);
                    x.SetupCard();
                    x.SetCardStatus(false);
                    StartCoroutine(x.ShowCardsForTime(_timeToShowCards));
                    x.SetCardFaceUp(false);
                });
            });

        Restart.interactable = true;
        DisplayLoadingPopUp(false);
    }

    void CheckIfGameCompleted()
    {
        if(GamePlayCards.Where(x=>x.activeInHierarchy).ToList().Count ==0)
        {
            DisplayLoadingPopUp(true);
        }
    }

    private void DisplayLoadingPopUp(bool status)
    {
        LoadingScreen.gameObject.SetActive(status);
    }
    public PlayerState playerState;
    public void SavePlayerState()
    {
        playerState = new PlayerState();
        playerState.PlayerCardsState = GamePlayCards.Where(a=>a.activeInHierarchy).Select(x => x.GetComponent<CardDisplay>().cardData).ToList();

        var json = JsonConvert.SerializeObject(playerState);
        Debug.LogError(json);
        PlayerPrefs.SetString("Progress", json);
    }

    public void LoadPlayerState()
    {
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("Progress")))
            return;
        var json = PlayerPrefs.GetString("Progress");
        playerState = JsonConvert.DeserializeObject<PlayerState>(json);
        GamePlayCards.ForEach(x => x.SetActive(false));
        foreach (var item in GamePlayCards)
        {
            for (int i = 0; i < playerState.PlayerCardsState.Count; i++)
            {
                if (item.GetComponent<CardDisplay>().cardData.CardID == playerState.PlayerCardsState[i].CardID)
                    item.SetActive(true);
            }
        }

        Stats.Instance.InvokeScore(ScoreCalculator.Instance.ResetScore().ToString());
        Stats.Instance.InvokeTurn(TurnCalculator.Instance.ResetTurns().ToString());
        Cards.Clear();
        var oldCards = GamePlayCards.Where(x => x.activeInHierarchy).ToList();
        Debug.LogError("Old count" + oldCards.Count);
        oldCards.Select(y => y.GetComponent<CardDisplay>()).ToList().
            ForEach(z =>
            {
                z.gameObject.SetActive(false);
                z.gameObject.SetActive(true);
                z.SetupCard();
                z.SetCardStatus(false);
                //StartCoroutine(z.ShowCardsForTime(_timeToShowCards));
                z.SetCardFaceUp(false);
            });
    }
}
[Serializable]
public class PlayerState
{
    [SerializeField]
    public List<CardData> PlayerCardsState = new();
}