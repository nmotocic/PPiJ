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
    public float getFactor(string name, float def = 1f)
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
        factors[name] = value;
    }
    public void removeFactor(string name)
    {
        if (!factors.ContainsKey(name)) { return; }
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
    public void ChangeWithFactor(string name, float compoundValueDiff) {
        float v = getFactor(name);
        if (v == 0) {
            if (nullifiers == 1) {
                setFactor(name, compoundValueDiff / compoundValue);
                nullifiers = 0;
                return;
            }
        }
        if (nullifiers > 0) { return; }
        float otherCompoundValue = compoundValue / v;
        setFactor(name, v-otherCompoundValue);
    }
}
