using UnityEngine;
using UnityEngine.Events;

public enum InteractionType { Default, Grabbable }

public interface IInteract
{
    public InteractionType type { get; set; }
    public UnityEvent OnInteracted { get; set; }
}
