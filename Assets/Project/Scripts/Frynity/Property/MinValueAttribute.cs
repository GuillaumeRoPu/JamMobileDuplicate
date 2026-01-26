using UnityEngine;

public class MinValueAttribute : PropertyAttribute
{
    public float minValue;

    public MinValueAttribute(float minValue)
        { this.minValue = minValue; }
}
