using System;
using UnityEngine;

public class ShowIfAttribute : PropertyAttribute
{
    public string conditionField {  get; private set; }
    public object conditionValue { get; private set; }
    public bool indentation { get; private set; }


    public ShowIfAttribute(string conditionField, bool indentation = true, object value = null)
    {
        this.conditionField = conditionField;
        conditionValue = value;
        this.indentation = indentation;
    }
}
