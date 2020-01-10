using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class UrgesDict : SerializableDictionary<Urge, UrgeProperties> { }

[Serializable]
public class UrgeFloatDict : SerializableDictionary<Urge, float> { }

public enum Urge
{
    Thirst,
    Hunger,
    Mating
}

[Serializable]
public class UrgeProperties
{
    public AnimationCurve curve;
    [Range(0, 1)] public float utilityValue;
    public float urgeValue;
    public float increaseSpeed;

    public delegate void VoidDelegate();
    public event VoidDelegate onUrgeExceedsLimit;

    public void TriggerOnUrgeExceedsLimit()
    {
        onUrgeExceedsLimit?.Invoke();
    }
}

public class UtilitySystem : MonoBehaviour
{
    
    [SerializeField] private UrgesDict urgeCurveDict;

    private void Start()
    {
        foreach (var urgeProperties in urgeCurveDict.Values)
        {
            urgeProperties.utilityValue = Random.Range(0, 0.1f);
        }
    }

    private void Update()
    {
        foreach (var kvp in urgeCurveDict)
        {
            UrgeProperties up = kvp.Value;
            up.urgeValue += up.increaseSpeed / 10000;

            up.utilityValue = up.curve.Evaluate(up.urgeValue);
            
            if (up.utilityValue >= 1)
            {
                up.TriggerOnUrgeExceedsLimit();
            }
        }
    }

    public Urge GetUrgeWithHighestVal()
    {
        return urgeCurveDict.OrderByDescending(u => u.Value.utilityValue).First().Key;
    }

    public void ResetUrge(Urge currentUrge)
    {
        urgeCurveDict[currentUrge].urgeValue = 0;
    }

    public void SubscribeOnUrgeExceedLimit(UrgeProperties.VoidDelegate action)
    {
        foreach (UrgeProperties urgeProperties in urgeCurveDict.Values)
        {
            urgeProperties.onUrgeExceedsLimit += action;
        }
    }
}