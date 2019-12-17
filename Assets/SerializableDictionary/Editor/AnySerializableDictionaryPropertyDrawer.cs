using UnityEditor;

[CustomPropertyDrawer(typeof(GameObjectFloatDict))]
[CustomPropertyDrawer(typeof(UrgesDict))]

public class AnySerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer {}