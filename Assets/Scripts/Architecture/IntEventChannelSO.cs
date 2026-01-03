using UnityEngine;
using UnityEngine.Events;

namespace KorroAI.Architecture
{
    [CreateAssetMenu(menuName = "Events/Int Event Channel")]
    public class IntEventChannelSO : ScriptableObject
    {
        public event UnityAction<int> OnEventRaised;

        public void RaiseEvent(int value)
        {
            if (OnEventRaised != null)
                OnEventRaised.Invoke(value);
            else
                Debug.LogWarning($"Event {name} raised but no listeners.");
        }
    }
}