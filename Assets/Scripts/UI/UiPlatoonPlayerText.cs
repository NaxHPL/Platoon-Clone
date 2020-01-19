using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UiPlatoonPlayerText : MonoBehaviour {

    [SerializeField]
    private bool isPlayerScoreText = default;

    private TextMeshProUGUI text;
    
    private void Awake() {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Start() {
        Game.Instance.OnGameStateChanged += HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state) {
        if ((state == GameState.OpponentsTurn && !isPlayerScoreText) ||
            (state == GameState.PlayersTurn && isPlayerScoreText)) {
            text.color = Color.green;
            text.fontStyle = FontStyles.Bold;
            text.fontSize = 34f;
        }
        else if (state != GameState.BattleResolving) {
            text.color = Color.white;
            text.fontStyle = FontStyles.Normal;
            text.fontSize = 28f;
        }
    }
}