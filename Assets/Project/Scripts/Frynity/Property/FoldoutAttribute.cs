using UnityEngine;

public class FoldoutAttribute : PropertyAttribute
{
    public string label;

    public FoldoutAttribute(string label)
    { this.label = label; }
}
