using System.Collections.Generic;
using UnityEngine;

public class CardPool : MonoBehaviour {

    public static CardPool Instance { get; private set; }
    
    [SerializeField]
    private Card cardPrefab = default;
    
    private readonly Queue<Card> cards = new Queue<Card>();

    public Card Get(CardData cardData) {
        if (cards.Count == 0)
            AddCard();
        
        Card card = cards.Dequeue();
        card.cardData = cardData;
        return card;
    }

    public void ReturnToPool(Card card) {
        card.gameObject.SetActive(false);
        card.isSelected = false;
        card.owner = null;
        card.group = null;
        
        cards.Enqueue(card);
    }

    public void ReturnAllToPool() {
        foreach (Card c in FindObjectsOfType<Card>())
            ReturnToPool(c);
    }

    private void AddCard() {
        Card card = Instantiate(cardPrefab);
        card.gameObject.SetActive(false);
        cards.Enqueue(card);
    }

    private void Awake() {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
}