using UnityEngine;
using UnityEngine.Events;

namespace KorroAI.Architecture
{
    [CreateAssetMenu(menuName = "Events/Void Event Channel")]
    public class VoidEventChannelSO : ScriptableObject
    {
        public event UnityAction OnEventRaised;

        public void RaiseEvent()
        {
            if (OnEventRaised != null)
            {
                OnEventRaised.Invoke();
            }
            else
            {
                Debug.LogWarning($"Event {name} was raised but no listeners are registered.");
            }
        }
    }
}

