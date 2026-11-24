using System;
using UnityEngine;

namespace VorgGates.Gameplay
{
    public sealed class PrototypePortal : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject activeVisual;

        public bool IsAvailable { get; private set; }
        public string Prompt => "[E] Atravessar o portal";
        public bool CanInteract => IsAvailable;

        public event Action Used;

        public void SetAvailable(bool available)
        {
            IsAvailable = available;
            if (activeVisual != null) activeVisual.SetActive(available);
        }

        public void Interact()
        {
            if (!IsAvailable) return;
            Used?.Invoke();
        }
    }
}
