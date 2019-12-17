using UnityEditor;

[CustomPropertyDrawer(typeof(GameObjectFloatDict))]
[CustomPropertyDrawer(typeof(UrgeCurveDict))]
[CustomPropertyDrawer(typeof(UrgeValueDict))]
public class AnySerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer {}