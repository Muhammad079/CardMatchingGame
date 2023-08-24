using System;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using Scripts.Statistics;
using System.Collections;
using System.Collections.Generic;
using Scripts.DataContainersAndUI;

namespace Scripts.GamePlay
{
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

            GameUI.Instance.ResetEvent += RestartTheGame;
            GameUI.Instance.SaveEvent += SavePlayerState;
            GameUI.Instance.LoadEvent += LoadPlayerState;
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
            if (!Cards.Contains(card) && card.SelectionStatus == CardDisplay.Selection.Selected)
            {
                Cards.Add(card);
            }
            else
            {
                ClearSelectedCards();
            }

            if (IsMatchingConditionMet())
            {
                UpdateTurnAndMatchCards();
            }

        }
        private void ClearSelectedCards()
        {
            Cards.Clear();
            clickCount = 0;
        }

        private bool IsMatchingConditionMet()
        {
            return clickCount % 2 == 0 && Cards.Count == 2;
        }

        private void UpdateTurnAndMatchCards()
        {
            Stats.Instance.InvokeTurn(TurnCalculator.Instance.GetCurrentTurn().ToString());
            StartCoroutine(MatchTheCards());
        }


        private IEnumerator MatchTheCards()
        {
            yield return new WaitForSeconds(_timeToCompareResult);

            if (AreCardsMatching() && Cards.Count == 2)
            {
                HideMatchingCards();
                UpdateScoreAndCheckCompletion();
            }
            else
            {
                RevertSelectedCards();
            }

            ClearSelectedCards();

        }
        private bool AreCardsMatching()
        {
            return Cards[0].CardID == Cards[1].CardID;
        }

        private void HideMatchingCards()
        {
            Cards.ForEach(card => card.gameObject.SetActive(false));
        }

        private void UpdateScoreAndCheckCompletion()
        {
            Stats.Instance.InvokeScore(ScoreCalculator.Instance.GetScore().ToString());
            CheckIfGameCompleted();
        }

        private void RevertSelectedCards()
        {
            foreach (var card in Cards.Where(go =>
                go.gameObject.activeInHierarchy &&
                go.GetComponent<CardDisplay>().SelectionStatus == CardDisplay.Selection.Selected))
            {
                card.GetComponent<CardDisplay>().SetCardFaceUp(false, () =>
                {
                    card.GetComponent<CardDisplay>().SetCardStatus(false);
                });
            }
        }

        public void RestartTheGame()
        {
            GameUI.Instance.SetInteractable(false, GameUI.Instance.Reset);
            StartCoroutine(RestartingGame());
            DisplayLoadingPopUp(true);
        }

        private IEnumerator RestartingGame()
        {
            Stats.Instance.InvokeScore(ScoreCalculator.Instance.ResetScore().ToString());
            Stats.Instance.InvokeTurn(TurnCalculator.Instance.ResetTurns().ToString());
            yield return new WaitForSeconds(5);
            GamePlayCards.ToList().ForEach(x => x.SetActive(false));
            Cards.Clear();
            _CardHolders.ToList().ForEach(x => x.DetachChildren());
            Shuffle(GamePlayCards,
                () =>
                {
                    for (int i = 0; i < GamePlayCards.Count; i++)
                    {
                        GamePlayCards[i].transform.SetParent(_CardHolders[i]);
                    }
                    GamePlayCards.Select(y => y.GetComponent<CardDisplay>()).ToList().ForEach(x =>
                    {
                        x.gameObject.SetActive(true);
                        x.SetupCard();
                        x.SetCardStatus(false);
                        StartCoroutine(x.ShowCardsForTime(_timeToShowCards));
                        x.SetCardFaceUp(false);
                    });
                });

            GameUI.Instance.SetInteractable(true, GameUI.Instance.Reset);
            DisplayLoadingPopUp(false);
        }

        void CheckIfGameCompleted()
        {
            if (GamePlayCards.Where(x => x.activeInHierarchy).ToList().Count == 0)
            {
                DisplayLoadingPopUp(true);
            }
        }

        private void DisplayLoadingPopUp(bool status)
        {
            LoadingScreen.SetActive(status);
        }
        public void SavePlayerState()
        {
            PlayerPrefs.DeleteAll();
            GameUI.Instance.SetInteractable(false, GameUI.Instance.Save, 5);

            PlayerState playerState = new PlayerState
            {
                PlayerCardsState = GamePlayCards.Where(a => a.activeInHierarchy).Select(x => x.GetComponent<CardDisplay>().CardData).ToList()
            };

            var json = JsonConvert.SerializeObject(playerState);
            PlayerPrefs.SetString("Progress", json);
        }

        public void LoadPlayerState()
        {
            GameUI.Instance.SetInteractable(false, GameUI.Instance.Load, 5);
            if (string.IsNullOrEmpty(PlayerPrefs.GetString("Progress")))
            {
                return;
            }

            var json = PlayerPrefs.GetString("Progress");
            PlayerState playerState = JsonConvert.DeserializeObject<PlayerState>(json);

            DeactivateAllGamePlayCards();

            foreach (var item in GamePlayCards)
            {
                foreach (var playerCardState in playerState.PlayerCardsState)
                {
                    if (item.GetComponent<CardDisplay>().CardData.CardID == playerCardState.CardID)
                    {
                        item.SetActive(true);
                    }
                }
            }

            ResetScoreAndTurns();

            ClearSelectedCards();

            UpdateActiveCards();
        }
        private void DeactivateAllGamePlayCards()
        {
            GamePlayCards.ForEach(card => card.SetActive(false));
        }

        private void ResetScoreAndTurns()
        {
            Stats.Instance.InvokeScore(ScoreCalculator.Instance.ResetScore().ToString());
            Stats.Instance.InvokeTurn(TurnCalculator.Instance.ResetTurns().ToString());
        }

        private void UpdateActiveCards()
        {
            var activeCards = GamePlayCards.Where(card => card.activeInHierarchy).ToList();

            activeCards.Select(display => display.GetComponent<CardDisplay>()).ToList().ForEach(card =>
            {
                card.gameObject.SetActive(false);
                card.gameObject.SetActive(true);
                card.SetupCard();
                card.SetCardStatus(false);
                card.SetCardFaceUp(false);
            });
        }

    }
    [Serializable]
    public class PlayerState
    {
        [SerializeField]
        public List<CardData> PlayerCardsState = new();
    }
}