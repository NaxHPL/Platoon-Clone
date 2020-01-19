using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class CardGroup : MonoBehaviour {

    public static List<CardGroup> allEnabledCardGroups = new List<CardGroup>();
    public static event Action OnCardAddedToAGroup;
    public static event Action OnGroupSelected;

    public static int EmptyPlayerGroupCount() => allEnabledCardGroups.Count(group => !group.isOpponentCardGroup && group.IsEmpty());
    public static int EmptyOpponentGroupCount() => allEnabledCardGroups.Count(group => group.isOpponentCardGroup && group.IsEmpty());
    public static CardGroup[] PlayerGroups() => allEnabledCardGroups.Where(group => !group.isOpponentCardGroup).ToArray();
    public static CardGroup[] NonEmptyPlayerGroups() => allEnabledCardGroups.Where(group => !group.isOpponentCardGroup && group.cards.Count > 0).ToArray();
    public static CardGroup[] OpponentGroups() => allEnabledCardGroups.Where(group => group.isOpponentCardGroup).ToArray();
    public static CardGroup[] NonEmptyOpponentGroups() => allEnabledCardGroups.Where(group => group.isOpponentCardGroup && group.cards.Count > 0).ToArray();

    public static (bool, string) IsValidForGroup(List<Card> groupOfCards, Card cardToCheck) {
        bool isValid = true;
        string errMsg = string.Empty;
        
        CardData cardData = cardToCheck.cardData;
        
        bool containsKing = groupOfCards.FirstOrDefault(card => card.cardData.IsKing) != null;
        bool containsBishop = groupOfCards.FirstOrDefault(card => card.cardData.IsBishop) != null;
        bool containsWizard = groupOfCards.FirstOrDefault(card => card.cardData.IsWizard) != null;
        int specialCardsInGroup = groupOfCards.Count(card =>
            card.cardData.IsWizard || card.cardData.IsKing || card.cardData.IsBishop
        );
        
        if (groupOfCards.Count == 0 && cardData.IsWizard) {
            errMsg = "A unit cannot be composed solely of a Wizard.";
            isValid = false;
        }
        
        if ((cardData.IsBishop || cardData.IsKing || cardData.IsWizard) && specialCardsInGroup == 2) {
            errMsg = "A unit cannot have more than two special cards.";
            isValid = false;
        }

        if ((containsBishop && cardData.IsKing) || (containsKing && cardData.IsBishop)) {
            errMsg = "A unit cannot contain both a Bishop and a King.";
            isValid = false;
        }
        
        if ((containsWizard && cardData.IsWizard) || (containsKing && cardData.IsKing || (containsBishop && cardData.IsBishop))) {
            errMsg = "A unit cannot contain two of the same special card.";
            isValid = false;
        }

        bool mustHaveOneCard;

        if (!cardToCheck.owner.isOpponent) {
            mustHaveOneCard = groupOfCards.Count != 0 &&
                              !cardData.IsWizard &&
                              EmptyPlayerGroupCount() + Game.Instance.Player.cardsInHand.Count(card => card.cardData.IsWizard) >= Game.Instance.Player.cardsInHand.Count;
        }
        else {
            mustHaveOneCard = groupOfCards.Count != 0 &&
                              !cardData.IsWizard &&
                              EmptyOpponentGroupCount() + Game.Instance.Opponent.cardsInHand.Count(card => card.cardData.IsWizard) >= Game.Instance.Opponent.cardsInHand.Count;
        }

        if (mustHaveOneCard) {
            errMsg = "Each unit must have at least one card.";
            isValid = false;
        }

        return (isValid, errMsg);
    }
    
    [HideInInspector]
    public List<Card> cards = new List<Card>();
    public bool isOpponentCardGroup = default;
    
    [SerializeField]
    private Canvas canvas = default;
    [SerializeField]
    private TextMeshProUGUI valueText = default;
    [SerializeField]
    private GameObject kingIconObj = default;
    [SerializeField]
    private GameObject bishopIconObj = default;
    [SerializeField]
    private GameObject wizardIconObj = default;
    [SerializeField]
    private Color selectedColor = Color.green;
    [SerializeField]
    private Color unselectedColor = Color.red;
    
    private Vector2 nextCardPosition;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider2D;
    private int totalValue;
    private Vector2 initialPosition;
    
    public void AddCard(Card card, bool checkForValidity) {
        if (checkForValidity) {
            (bool, string) valid = IsValidForGroup(cards, card);

            if (!valid.Item1) {
                card.Select(false, !card.owner.isOpponent);
                Game.Instance.ShowMessage(valid.Item2, 3.0f);
                return;
            }
        }

        StartCoroutine(card.MoveFromTo(card.transform.localPosition, nextCardPosition, 0.2f));
        nextCardPosition.y -= 0.4f;
        
        totalValue += card.cardData.Value;
        valueText.text = totalValue.ToString();
        
        cards.Add(card);
        card.group = this;

        card.owner.RemoveCardFromHand(card);

        if (card.cardData.IsBishop || card.cardData.IsKing || card.cardData.IsWizard)
            UpdateSpecialCardIcons(card.cardData);

        ReLayoutCardsInHand();
        
        OnCardAddedToAGroup?.Invoke();
    }
    
    public IEnumerator MoveFromTo(Vector2 startPos, Vector2 endPos, float duration) {
        if (Mathf.Approximately(duration, 0f)) {
            transform.localPosition = endPos;
            yield break;
        }
        
        transform.localPosition = startPos;
        float elapsedTime = 0f;
        float progress = 0f;

        while (progress < 1) {
            transform.localPosition = Vector2.Lerp(startPos, endPos, progress);

            elapsedTime += Time.deltaTime;
            progress = elapsedTime / duration;

            yield return null;
        }

        transform.localPosition = endPos;
    }

    public Vector2[] GetPositions(float firstPosY, float offset, bool growUp) {
        Vector2[] positions = new Vector2[cards.Count];
        positions[0] = new Vector2(transform.position.x, firstPosY);

        for (int i = 1; i < cards.Count; i++) {
            float newY = growUp ? positions[i - 1].y + offset : positions[i - 1].y - offset;
            positions[i] = new Vector2(positions[0].x, newY);
        }

        return positions;
    }

    public void Select(bool select) {
        if (isOpponentCardGroup) {
            if (select && Game.Instance.Opponent.cardGroupSelectedForBattle != null)
                Game.Instance.Opponent.cardGroupSelectedForBattle.Select(false);

            Game.Instance.Opponent.cardGroupSelectedForBattle = select ? this : null;
        }
        else {
            if (select && Game.Instance.Player.cardGroupSelectedForBattle != null)
                Game.Instance.Player.cardGroupSelectedForBattle.Select(false);

            Game.Instance.Player.cardGroupSelectedForBattle = select ? this : null;
        }

        spriteRenderer.color = select ? selectedColor : unselectedColor;
        
        if (select)
            OnGroupSelected?.Invoke();
    }

    public void ClearCardsAndDisableCanvas() {
        cards.Clear();
        canvas.enabled = false;
    }
    
    private void UpdateSpecialCardIcons(CardData cardData) {
        if (cardData.IsBishop)
            ShowSpecialCardIcons(kingIconObj.activeSelf, cardData.IsBishop, wizardIconObj.activeSelf);
        else if (cardData.IsKing)
            ShowSpecialCardIcons(cardData.IsKing, bishopIconObj.activeSelf, wizardIconObj.activeSelf);
        else if (cardData.IsWizard)
            ShowSpecialCardIcons(kingIconObj.activeSelf, bishopIconObj.activeSelf, cardData.IsWizard);
    }

    private bool IsEmpty() => cards.Count == 0;
    
    private void Awake() {
        initialPosition = transform.localPosition;
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();
    }
    
    private void Start() {
        Initialize();
        Game.Instance.OnGameStateChanged += HandleGameStateChange;
    }

    private void OnEnable() {
        allEnabledCardGroups.Add(this);
    }
    
    private void OnDisable() {
        allEnabledCardGroups.Remove(this);
    }

    private void HandleGameStateChange(GameState state) {
        if (!isOpponentCardGroup) {
            switch (state) {
                case GameState.DealingCards:
                    Initialize();
                    Show(false);
                    break;
                case GameState.PlayerCreatingGroups:
                    ShowSpecialCardIcons(false, false, false);
                    Show(true);
                    break;
                case GameState.OpponentCreatingGroups:
                    FlipCardsAndMoveToPlayerArea(0.25f);
                    break;
                case GameState.PlayersTurn:
                    boxCollider2D.enabled = false;
                    break;
                case GameState.GameIsOver:
                    Show(false);
                    break;
            }
        }
        else {
            switch (state) {
                case GameState.DealingCards:
                    Initialize();
                    Show(false);
                    break;
                case GameState.OpponentCreatingGroups:
                    Show(true);
                    ShowSpecialCardIcons(false, false, false);
                    boxCollider2D.enabled = false;
                    canvas.enabled = false;
                    break;
                case GameState.DecidingFirstTurn:
                    MoveToOpponentPlayArea(0.25f);
                    break;
                case GameState.GameIsOver:
                    Show(false);
                    break;
            }
        }
    }

    private void FlipCardsAndMoveToPlayerArea(float duration) {
        Vector2[] positions = GetPositions(Game.Instance.Player.playArea.Center.y, 0.2f, true);
        StartCoroutine(MoveFromTo(transform.localPosition, positions[0], duration));
        
        for (int i = 0; i < cards.Count; i++) {
            cards[i].HideImmediately(true);
            StartCoroutine(cards[i].MoveFromTo(cards[i].transform.localPosition, positions[i], duration));
        }
    }

    private void MoveToOpponentPlayArea(float duration) {
        Vector2[] positions = GetPositions(Game.Instance.Opponent.playArea.Center.y, 0.2f, false);
        StartCoroutine(MoveFromTo(transform.localPosition, positions[0], duration));
        
        for (int i = 0; i < cards.Count; i++)
            StartCoroutine(cards[i].MoveFromTo(cards[i].transform.localPosition, positions[i], duration));
    }
    
    private void Initialize() {
        cards.Clear();
        transform.localPosition = initialPosition;
        nextCardPosition = transform.localPosition;
        totalValue = 0;
        valueText.text = "0";
        spriteRenderer.color = unselectedColor;
    }

    private void Show(bool show) {
        spriteRenderer.enabled = show;
        boxCollider2D.enabled = show;
        canvas.enabled = show;
    }

    private void ShowSpecialCardIcons(bool showKing, bool showBishop, bool showWizard) {
        kingIconObj.SetActive(showKing);
        bishopIconObj.SetActive(showBishop);
        wizardIconObj.SetActive(showWizard);
    }
    
    private void OnMouseUpAsButton() {
        if (isOpponentCardGroup)
            return;

        Card card = Game.Instance.Player.cardSelected;

        if (card != null) {
            if (card.group == this)
                return;
            
            card.Select(false, false);
            AddCard(card, true);
        }
    }

    private void ReLayoutCardsInHand() {
        List<Card> playersCards = Game.Instance.Player.cardsInHand;
        
        if (playersCards.Count == 0)
            return;
        
        Vector2[] positions = Game.Instance.Player.playArea.GetCardLayout(
            playersCards.Count,
            playersCards.First().CardWidth,
            0.1f
        );

        for (int i = 0; i < playersCards.Count; i++) {
            Vector2 startPos = playersCards[i].transform.localPosition;
            StartCoroutine(playersCards[i].MoveFromTo(startPos, positions[i], 0.1f));
        }
    }
}