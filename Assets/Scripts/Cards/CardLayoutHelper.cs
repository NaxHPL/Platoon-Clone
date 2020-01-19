using UnityEngine;

public static class CardLayoutHelper {

    public static Vector2[] GetCardLayout(Vector2 center, int numCards, float cardWidth, float padding) {
        Vector2[] positions = new Vector2[numCards];
        
        float mul = numCards % 2 != 0
            ? Mathf.Floor(numCards / 2f)
            : (numCards / 2f) - 0.5f;
        
        positions[0] = center - new Vector2((mul * cardWidth) + (mul * padding), 0f);

        for (int i = 1; i < numCards; i++) {
            Vector2 offsetFromFirstPos  = new Vector2((i * cardWidth) + (i * padding), 0f);
            positions[i] = positions[0] + offsetFromFirstPos;
        }

        return positions;
    }
}
