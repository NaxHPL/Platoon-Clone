using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Card : MonoBehaviour, IComparable<Card> {

    public float CardWidth => spriteRenderer.bounds.size.x;
    
    public CardData cardData;
    [HideInInspector]
    public PlatoonPlayer owner;
    [HideInInspector]
    public bool isSelected;
    [HideInInspector]
    public CardGroup group;

    private SpriteRenderer spriteRenderer;
    private bool isHidden;
    private bool isSelectable;

    public void HideImmediately(bool hidden) {
        spriteRenderer.sprite = hidden ? cardData.BackSprite : cardData.FrontSprite;
        isHidden = hidden;
    }

    public IEnumerator MoveFromTo(Vector2 startPos, Vector2 endPos, float duration) {
        if (Mathf.Approximately(duration, 0f)) {
            transform.localPosition = endPos;
            yield break;
        }

        isSelectable = false;
        
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
        isSelectable = true;
    }

    public IEnumerator FlipCard(float duration) {
        if (Mathf.Approximately(duration, 0f)) {
            HideImmediately(!isHidden);
            yield break;
        }

        isSelectable = false;
        
        Vector2 startScale = transform.localScale;
        Vector2 endScale = new Vector2(0f, startScale.y);
        
        float elapsedTime = 0f;
        float progress = 0f;

        while (progress < 1f) {
            transform.localScale = Vector2.Lerp(startScale, endScale, progress);

            elapsedTime += Time.deltaTime;
            progress = elapsedTime / (duration * 0.5f);
            
            yield return null;
        }

        HideImmediately(!isHidden);
        
        elapsedTime = 0f;
        progress = 0f;
        
        while (progress < 1f) {
            transform.localScale = Vector2.Lerp(endScale, startScale, progress);

            elapsedTime += Time.deltaTime;
            progress = elapsedTime / (duration * 0.5f);
            
            yield return null;
        }

        transform.localScale = startScale;
        isSelectable = true;
    }

    public void Select(bool select, bool doAnimation) {
        if (doAnimation) {
            Vector2 start = transform.localPosition;
            Vector2 end = select ? start.WithY(start.y + 0.35f) : start.WithY(start.y - 0.35f);
            StartCoroutine(MoveFromTo(start, end, 0.05f));
        }

        isSelected = select;
        owner.cardSelected = isSelected ? this : null;
    }

    public int CompareTo(Card other) {
        if (cardData.IsPawn && other.cardData.IsPawn) {
            if (cardData.Value < other.cardData.Value)
                return -1;
            if (cardData.Value == other.cardData.Value)
                return 0;
            if (cardData.Value > other.cardData.Value)
                return 1;
        }
        else {
            if (cardData.cardType < other.cardData.cardType)
                return -1;
            if (cardData.cardType == other.cardData.cardType)
                return 0;
            if (cardData.cardType > other.cardData.cardType)
                return 1;
        }

        return 0;
    }
    
    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable() {
        Game.Instance.OnGameStateChanged += HandleGameStateChanged;
    }
    
    private void OnDisable() {
        Game.Instance.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state) {
        switch (state) {
            case GameState.PlayerCreatingGroups:
            case GameState.PlayersTurn:
            case GameState.OpponentsTurn:
                isSelectable = true;
                break;
            default:
                isSelectable = false;
                break;
        }
    }

    private void OnMouseUpAsButton() {
        if (Game.Instance.State == GameState.PlayersTurn) {
            group.Select(true);
            return;
        }
        
        if (!isSelectable || owner.isOpponent || Game.Instance.State == GameState.OpponentsTurn)
            return;

        if (owner.cardSelected != null && owner.cardSelected != this) {
            if (group != null) {
                group.AddCard(owner.cardSelected, true);
                if (owner.cardSelected != null)
                    owner.cardSelected.Select(false, false);
                
                return;
            }

            owner.cardSelected.Select(false, true);
        }

        if (group == null)
            Select(!isSelected, true);
    }
}