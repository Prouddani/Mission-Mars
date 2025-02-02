using UnityEngine;
using UnityEngine.Events;

public class InteractionBehaviour : MonoBehaviour, IInteract
{
    [SerializeField] private InteractionType interactionType;
    public InteractionType type { get { return interactionType; } set { interactionType = value; } }

    [SerializeField] private UnityEvent onInteracted;
    public UnityEvent OnInteracted { get { return onInteracted; } set { onInteracted = value; } }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
