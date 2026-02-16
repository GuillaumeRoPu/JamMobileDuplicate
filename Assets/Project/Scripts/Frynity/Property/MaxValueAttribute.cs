using UnityEngine;

public class MaxValueAttribute : PropertyAttribute
{
    public float maxValue;

    public MaxValueAttribute(float maxValue)
    { this.maxValue = maxValue; }
}
