using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method)]
public class ButtonAttribute : Attribute
{
    public string text;

    public ButtonAttribute(string text = null)
    {  this.text = text; }
}
