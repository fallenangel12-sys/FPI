using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory Settings")]
    public int maxSlots = 4;
    public List<ItemData> items = new List<ItemData>();

    [Header("UI Refrences")]
    public GameObject inventoryUI;
    public Transform slotContainer;
    public GameObject slotPrefab;

    [Header("Hold Item Settings")]
    public Transform holdPoint;
    private GameObject currentHeldItem;

    public bool isInventoryOpen = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (slotContainer == null)
        {
            slotContainer = GameObject.Find("SlotContainer")?.transform;
            if (slotContainer == null)
            {
                Debug.LogError("Could not find SlotContainer in scene!");
                return;
            }
        }

        if (slotContainer.childCount == 0 && slotPrefab != null)
        {
            for (int i = 0; i < maxSlots; i++)
                Instantiate(slotPrefab, slotContainer);
        }

        items.Clear();
        UpdateUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    public void AddItem(ItemData item)
    {
        if (item == null) return;

        if (items.Count < maxSlots)
        {
            items.Add(item);
            UpdateUI();
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;

        if (inventoryUI != null)
        {
            inventoryUI.SetActive(isInventoryOpen);
        }

        if (isInventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            UpdateUI();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void DropItem(int index)
    {
        if (index < 0 || index >= items.Count) return;

        ItemData itemToDrop = items[index];
        GameObject prefabToSpawn = (itemToDrop.worldPrefab != null) ? itemToDrop.worldPrefab : itemToDrop.itemPrefab;

        if (prefabToSpawn != null)
        {
            // 1. POSITIONING (RELATIVE TO PLAYER)
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Vector3 playerPos = (player != null) ? player.transform.position : transform.position;
            Vector3 playerForward = (player != null) ? player.transform.forward : transform.forward;

            Vector3 spawnPos = playerPos + (playerForward * 1.5f) + (Vector3.up * 1.2f);

            // 2. SPAWN
            GameObject droppedObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            droppedObj.transform.localScale = prefabToSpawn.transform.localScale;
            droppedObj.name = itemToDrop.itemName;

            // --- HOVER SCRIPT DISABLE ---
            // This looks for any component with "Hover" in the name and disables it
            MonoBehaviour hoverScript = droppedObj.GetComponentInChildren<MonoBehaviour>();
            // If your hover script is exactly named 'Hover', use: droppedObj.GetComponentInChildren<Hover>();
            // Here we use a string check to be safe if we don't know the exact class name
            foreach (var script in droppedObj.GetComponentsInChildren<MonoBehaviour>())
            {
                if (script.GetType().Name.Contains("Hover"))
                {
                    script.enabled = false;
                    Debug.Log($"🚫 Disabled {script.GetType().Name} on {droppedObj.name}");
                }
            }

            // 3. COLLIDER CHECK
            Collider col = droppedObj.GetComponentInChildren<Collider>();
            if (col == null)
            {
                BoxCollider box = droppedObj.AddComponent<BoxCollider>();
                MeshRenderer mesh = droppedObj.GetComponentInChildren<MeshRenderer>();
                if (mesh != null)
                {
                    box.center = droppedObj.transform.InverseTransformPoint(mesh.bounds.center);
                    box.size = mesh.bounds.size;
                }
            }

            // 4. ATTACH PICKUP SCRIPT
            PickupItem pickupScript = droppedObj.GetComponent<PickupItem>();
            if (pickupScript == null) pickupScript = droppedObj.AddComponent<PickupItem>();
            pickupScript.item = itemToDrop; 

            // 5. PHYSICS SETUP
            Rigidbody rb = droppedObj.GetComponent<Rigidbody>();
            if (rb == null) rb = droppedObj.AddComponent<Rigidbody>();

            rb.isKinematic = false;   
            rb.useGravity = true;     
            rb.linearDamping = 0f;    
            rb.angularDamping = 0.05f; 

            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.AddForce(playerForward * 1.2f, ForceMode.Impulse);

            MeshRenderer mr = droppedObj.GetComponentInChildren<MeshRenderer>();
            if (mr != null) mr.enabled = true;

            Debug.Log($"✅ {droppedObj.name} dropped. Hover disabled and gravity active.");
        }

        if (currentHeldItem != null && currentHeldItem.name.Contains(itemToDrop.itemName))
        {
            Destroy(currentHeldItem);
            currentHeldItem = null;
        }

        items.RemoveAt(index);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (slotContainer == null) return;

        for (int i = 0; i < slotContainer.childCount; i++)
        {
            Transform slot = slotContainer.GetChild(i);
            Image panelImage = slot.GetComponent<Image>();
            if (panelImage == null) continue;

            panelImage.raycastTarget = true;

            Button button = slot.GetComponent<Button>();
            if (button == null) button = slot.gameObject.AddComponent<Button>();

            button.targetGraphic = panelImage; 
            button.onClick.RemoveAllListeners();

            if (i < items.Count)
            {
                ItemData currentItem = items[i];
                if (currentItem != null && currentItem.itemIcon != null)
                {
                    panelImage.sprite = currentItem.itemIcon;
                    panelImage.color = Color.white;

                    int index = i; 
                    button.onClick.AddListener(() => OnItemClicked(index));
                }
            }
            else
            {
                panelImage.sprite = null;
                panelImage.color = new Color(1f, 1f, 1f, 0.25f);
                button.onClick.RemoveAllListeners();
            }
        }
    }

    private void OnItemClicked(int index)
    {
        if (index < 0 || index >= items.Count) return;
        DropItem(index);
    }
}
