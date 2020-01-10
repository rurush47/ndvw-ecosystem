using UnityEditor;

[CustomPropertyDrawer(typeof(GameObjectFloatDict))]
[CustomPropertyDrawer(typeof(UrgesDict))]
[CustomPropertyDrawer(typeof(UrgeFloatDict))]

public class AnySerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer {}