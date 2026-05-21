using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("Interaction Info")]
    public string promptMessage = "Press E to interact";

    public virtual void OnInteract()
    {
        Debug.Log("Interacted with " + gameObject.name);
    }
}