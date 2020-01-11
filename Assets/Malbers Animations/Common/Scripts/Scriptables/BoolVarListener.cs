using MalbersAnimations.Scriptables;
using MalbersAnimations.Events;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations
{
    public class BoolVarListener : MonoBehaviour
    {
        public BoolVar value;
        public BoolEvent Raise = new BoolEvent();
        public UnityEvent OnFalse = new UnityEvent();
        public UnityEvent OnTrue = new UnityEvent();

        void OnEnable()
        {
            value?.OnValueChanged.AddListener(InvokeBool);
            Raise.Invoke(value ?? false);
        }

        void OnDisable()
        {
            value?.OnValueChanged.RemoveListener(InvokeBool);
        }

        public virtual void InvokeBool(bool value)
        {
            Raise.Invoke(value);

            if (value)
                OnTrue.Invoke();
            else
                OnFalse.Invoke();

        }
    }
}
