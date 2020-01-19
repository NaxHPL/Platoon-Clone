using UnityEngine;

public class PlayArea : MonoBehaviour {

    public Vector2 Center => transform.localPosition;

    public Vector2[] GetCardLayout(int numCards, float cardWidth, float padding) {
        return CardLayoutHelper.GetCardLayout(Center, numCards, cardWidth, padding);
    }
}