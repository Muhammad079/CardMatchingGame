using System;
using UnityEngine;

namespace Scripts.Statistics
{
    public class Stats : MonoBehaviour
    {
        public TMPro.TMP_Text Score;
        public TMPro.TMP_Text Turns;

        public Action<string> ScoreEvent;
        public Action<string> TurnEvent;

        public static Stats Instance;
        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            ScoreEvent += OnScoreEvent;
            TurnEvent += OnTurnEvent;
        }

        public void InvokeTurn(string Points)
        {
            TurnEvent?.Invoke(Points);
        }
        public void InvokeScore(string Points)
        {
            ScoreEvent?.Invoke(Points);
        }

        private void OnTurnEvent(string obj)
        {
            Turns.text = obj;
        }

        private void OnScoreEvent(string obj)
        {
            Score.text = obj;
        }

        public void ResetStats()
        {
            Score.text = "0";
            Turns.text = "0";
        }
    }
}