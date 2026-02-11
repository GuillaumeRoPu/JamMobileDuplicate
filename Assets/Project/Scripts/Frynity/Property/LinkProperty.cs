using System;
using UnityEngine;

public class LinkAttribute : PropertyAttribute
{
    public string Link1;
    public string Link2;

    public LinkAttribute(string link1, string link2)
    {
        Link1 = link1;
        Link2 = link2;
    }
}
