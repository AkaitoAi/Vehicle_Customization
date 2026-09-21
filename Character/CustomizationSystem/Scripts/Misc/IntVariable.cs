using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Int", menuName = "Variables/Int", order = 1)]
public class IntVariable : ScriptableObject, ISerializationCallbackReceiver
{
    public int value;

    [NonSerialized] public int runtimeValue;

    public void OnAfterDeserialize()
    {
        runtimeValue = value;
    }

    public void OnBeforeSerialize(){}
}
