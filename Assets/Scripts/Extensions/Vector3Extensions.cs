using UnityEngine;

public static class Vector2Extensions {

	public static Vector2 WithX(this Vector2 v, float x) {
		return new Vector2(x, v.y);
	}

	public static Vector2 WithY(this Vector2 v, float y) {
		return new Vector2(v.x, y);
	}
}
