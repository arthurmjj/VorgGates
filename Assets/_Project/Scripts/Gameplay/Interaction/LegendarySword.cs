using System;
using UnityEngine;

namespace VorgGates.Gameplay
{
    /// <summary>The Blade of Vorg. Taking it ends the campaign.</summary>
    public sealed class LegendarySword : MonoBehaviour, IInteractable
    {
        private const float RiseHeight = 1.3f;
        private const float RiseSpeed = 1.4f;

        [SerializeField] private Transform blade;
        [SerializeField] private GameObject glow;

        private Vector3 restPosition;
        private bool taken;

        public bool IsAvailable { get; private set; }
        public string Prompt => "[E] Retirar a Lâmina de Vorg";
        public bool CanInteract => IsAvailable && !taken;

        public event Action Taken;

        private void Awake() => restPosition = blade.localPosition;

        public void SetAvailable(bool available)
        {
            IsAvailable = available;
            glow.SetActive(available);
        }

        public void Interact()
        {
            if (!CanInteract) return;
            taken = true;
            Taken?.Invoke();
        }

        private void Update()
        {
            if (!taken) return;
            blade.localPosition = Vector3.MoveTowards(blade.localPosition, restPosition + Vector3.up * RiseHeight, RiseSpeed * Time.unscaledDeltaTime);
        }
    }
}
