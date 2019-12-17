using System;
using UnityEngine;

[Serializable]
public class UrgeCurveDict : SerializableDictionary<Urge, AnimationCurve> { }

[Serializable]
public class UrgeValueDict : SerializableDictionary<Urge, float> { }


public enum Urge
{
    Thirst,
    Hunger,
    Mating
}

public class UtilitySystem : MonoBehaviour
{
    [SerializeField] private UrgeCurveDict urgeCurveDict;
    [SerializeField] private UrgeValueDict urgeValueDict;
}