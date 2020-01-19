using UnityEngine;

public class UiPlatoonPlayerScores : MonoBehaviour {

    [SerializeField]
    private bool isPlayerScores = default;
    [SerializeField]
    private GameObject winPrefab = default;
    [SerializeField]
    private GameObject losePrefab = default;
    [SerializeField]
    private GameObject drawPrefab = default;

    private void Start() {
        Game.Instance.OnBattleResolved += HandleBattleResolved;
        Game.Instance.OnGameStateChanged += HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state) {
        switch (state) {
            case GameState.DealingCards:
                RemoveAllScoreIcons();
                break;
        }
    }

    private void RemoveAllScoreIcons() {
        for (int i = 0; i < transform.childCount; i++)
            Destroy(transform.GetChild(i).gameObject);
    }

    private void HandleBattleResolved(BattleOutcome outcome) {
        switch (outcome) {
            case BattleOutcome.OpponentWins:
                AddScoreIcon(isPlayerScores ? losePrefab : winPrefab);
                break;
            case BattleOutcome.PlayerWins:
                AddScoreIcon(isPlayerScores ? winPrefab : losePrefab);
                break;
            case BattleOutcome.Draw:
                AddScoreIcon(drawPrefab);
                break;
        }
    }

    private void AddScoreIcon(GameObject prefab) {
        Instantiate(prefab, transform);
    }
}