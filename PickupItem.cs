using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Pickup Settings")]
    // Changed to 'item' so InventoryManager can find it automatically
    public ItemData item; 
    public float pickupRadius = 3f;
    public KeyCode interactKey = KeyCode.E;

    private Transform player;
    private bool isInRange = false;
    private MeshRenderer meshRenderer;

    void Start()
    {
        FindPlayer();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        // --- OFFSET FIX ---
        // If this is the Red Key, the 'transform.position' is at Y: -293.
        // We use the meshRenderer.bounds.center to get the actual visual location of the key.
        Vector3 actualPosition = (meshRenderer != null) ? meshRenderer.bounds.center : transform.position;

        float distance = Vector3.Distance(player.position, actualPosition);
        isInRange = distance <= pickupRadius;

        if (isInRange && Input.GetKeyDown(interactKey))
        {
            Pickup();
        }
    }

    void Pickup()
    {
        if (item == null)
        {
            Debug.LogError($"Pickup failed: {gameObject.name} has no ItemData assigned!");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError($"Pickup failed: No InventoryManager instance found!");
            return;
        }

        // Check if inventory has space before picking up
        if (InventoryManager.Instance.items.Count < InventoryManager.Instance.maxSlots)
        {
            Debug.Log($"✅ Player picked up: {item.itemName}");
            InventoryManager.Instance.AddItem(item);
            
            // Destroy instead of SetActive(false) to keep the scene clean
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("Inventory Full!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        // Gizmos will now show around the actual mesh center if available
        Vector3 debugPos = (meshRenderer != null) ? meshRenderer.bounds.center : transform.position;
        Gizmos.DrawWireSphere(debugPos, pickupRadius);
    }
}