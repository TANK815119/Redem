using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Slot : MonoBehaviour
{
    [SerializeField] private Rigidbody slotAnchor;
    [field: SerializeField] public SlotType SlotType { get; private set; }
    public Item SlottedItem { get; private set; }
    private ConfigurableJoint slotJoint;
    private List<Item> competitors;

    void Start()
    {
        competitors = new List<Item>();
    }

    void Update()
    {
        SlotUpdate();
    }

    private void SlotUpdate()
    {
        // make the closest competitor the slotted item
        if (competitors.Count == 0)
        {
            return;
        }

        Item closestItem = competitors[0];
        for (int i = 0; i < competitors.Count; i++)
        {
            if (Vector3.Distance(competitors[i].transform.position, transform.position) <
                Vector3.Distance(closestItem.transform.position, transform.position))
            {
                closestItem = competitors[i];
            }
        }

        if (closestItem != SlottedItem)
        {
            UnslotItem();
            SlotItem(closestItem);
        }
    }

    private void SlotItem(Item item)
    {
        // assign
        SlottedItem = item;

        // fetch the item's rigidbody
        if (!item.TryGetComponent<Rigidbody>(out Rigidbody itemBody))
        {
            Debug.LogError("ItemSlot: Could not find Rigidbody on Item to slot.");
            return;
        }
        itemBody.useGravity = false;

        // make physics joint from slotAnchor to slotted item
        slotJoint = slotAnchor.gameObject.AddComponent<ConfigurableJoint>();

        slotJoint.connectedBody = itemBody;
        slotJoint.autoConfigureConnectedAnchor = false;
        slotJoint.connectedAnchor = Vector3.zero;
        slotJoint.anchor = Vector3.zero;

        JointDrive jointDrive = new JointDrive
        {
            positionSpring = 1000f,
            positionDamper = 100f,
            maximumForce = Mathf.Infinity
        };

        slotJoint.xDrive = jointDrive;
        slotJoint.yDrive = jointDrive;
        slotJoint.zDrive = jointDrive;
        slotJoint.angularXDrive = jointDrive;
        slotJoint.angularYZDrive = jointDrive;
    }

    private void UnslotItem()
    {
        if (SlottedItem == null)
        {
            return;
        }

        // fetch the item's rigidbody
        if (!SlottedItem.TryGetComponent<Rigidbody>(out Rigidbody itemBody))
        {
            Debug.LogError("ItemSlot: Could not find Rigidbody on Item to slot.");
            return;
        }
        itemBody.useGravity = true;

        Destroy(slotJoint);
        SlottedItem = null;
        slotJoint = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Item>(out Item item)  && item.ItemType == SlotType)
        {
            competitors.Add(item);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<Item>(out Item item))
        {
            return;
        }

        if (item == SlottedItem)
        {
            UnslotItem();
        }

        if(competitors.Contains(item))
        {
            competitors.Remove(item);
        }
    }
}
