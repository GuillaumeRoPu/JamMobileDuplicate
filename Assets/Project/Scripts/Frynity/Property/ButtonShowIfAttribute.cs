using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method)]
public class ButtonShowIfAttribute : PropertyAttribute
{
    public string conditionField { get; private set; }
    public object conditionValue { get; private set; }


    public ButtonShowIfAttribute(string conditionField, object value = null)
    {
        this.conditionField = conditionField;
        conditionValue = value;
    }
}
