using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatStat
{
    string name; 
    Dictionary<string, float> factors;
    int nullifiers = 0;
    float compoundValue;
    public FloatStat(string name) {
        compoundValue = 1f;
        this.name = name;
        this.factors = new Dictionary<string, float>();
    }
    public FloatStat(string name, float baseValue)
    {
        compoundValue = 1f;
        this.name = name;
        this.factors = new Dictionary<string, float>();
        this.setFactor("baseValue", baseValue);
    }
    public string getName() { return name; }
    public float getCompoundValue() {
        return (nullifiers == 0) ? compoundValue : 0;
    }
    public float getFactor(string name, float def = 0f)
    {
        if (!factors.ContainsKey(name)) { return def; }
        return factors[name];
    }
    public void setFactor(string name, float value)
    {
        if (factors.ContainsKey(name))
        {
            this.removeFactor(name);
        }
        if (value == 0f)
        {
            nullifiers += 1;
        }
        else
        {
            compoundValue *= value;
        }
    }
    public void removeFactor(string name)
    {
        if (factors[name] == 0f)
        {
            nullifiers -= 1;
        }
        else
        {
            compoundValue /= factors[name];
        }
        factors.Remove(name);
    }
}
