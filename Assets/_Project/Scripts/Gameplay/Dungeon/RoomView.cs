using UnityEngine;
using VorgGates.Core;

namespace VorgGates.Gameplay
{
    /// <summary>Modular room shell. Each side shows either a solid wall or a doorway.</summary>
    public sealed class RoomView : MonoBehaviour
    {
        [SerializeField] private GameObject[] closedSides = new GameObject[SideUtil.Count];
        [SerializeField] private GameObject[] openSides = new GameObject[SideUtil.Count];
        [SerializeField] private Transform[] doorAnchors = new Transform[SideUtil.Count];

        public Transform DoorAnchor(Side side) => doorAnchors[(int)side];

        public void ApplyDoors(int doorMask)
        {
            for (var i = 0; i < SideUtil.Count; i++)
            {
                var isOpen = (doorMask & (1 << i)) != 0;
                openSides[i].SetActive(isOpen);
                closedSides[i].SetActive(!isOpen);
            }
        }
    }
}
