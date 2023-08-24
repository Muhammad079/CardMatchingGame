using UnityEngine;

namespace Scripts.Statistics
{
    public class ScoreCalculator : MonoBehaviour
    {
        [SerializeField]
        public int ScoreIncrement;

        private int CurrentScore = 0;

        public static ScoreCalculator Instance;
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }
        public int GetScore()
        {
            CurrentScore += ScoreIncrement;
            return CurrentScore;
        }

        public int ResetScore()
        {
            return CurrentScore = 0;
        }
    }
}