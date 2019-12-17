using System;
using UnityEngine;

[Serializable]
public class UrgesDict : SerializableDictionary<Urge, UrgeProperties> { }

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

    private void Update()
    {
        foreach (var kvp in urgeCurveDict)
        {
            UrgeProperties up = kvp.Value;
            up.urgeValue += up.increaseSpeed;

            up.utilityValue = up.curve.Evaluate(up.urgeValue);
            
            if (up.utilityValue >= 1)
            {
                up.TriggerOnUrgeExceedsLimit();
            }
        }
    }
}