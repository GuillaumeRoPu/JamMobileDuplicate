using UnityEngine;

public class SpaceAttribute : PropertyAttribute
{
    public float width { get; private set; }


    public SpaceAttribute(float nb)
    {
        width = nb;
    }
}
