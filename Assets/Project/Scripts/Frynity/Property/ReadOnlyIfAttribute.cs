using UnityEngine;

public class ReadOnlyIfAttribute : PropertyAttribute
{
    public string condition;

    public ReadOnlyIfAttribute(string condition)
        { this.condition = condition; }
}
