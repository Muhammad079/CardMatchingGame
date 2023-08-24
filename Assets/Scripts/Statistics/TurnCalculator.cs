using UnityEngine;

namespace Scripts.Statistics
{
    public class TurnCalculator : MonoBehaviour
    {
        [SerializeField]
        private int Increment = 1;

        private int CurrentTurnNumber = 0;

        public static TurnCalculator Instance;
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }
        public int GetCurrentTurn()
        {
            CurrentTurnNumber += Increment;
            return CurrentTurnNumber;
        }
        public int ResetTurns()
        {
            return CurrentTurnNumber = 0;
        }
    }
}