using UnityEngine;
using VorgGates.Core;

namespace VorgGates.Gameplay
{
    /// <summary>Reward room prize: the next missing weapon, or a healing elixir when all are owned.</summary>
    public sealed class WeaponPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject[] weaponModels = new GameObject[InventoryState.SlotCount];
        [SerializeField] private GameObject elixirModel;
        [SerializeField] private Transform floatRoot;
        [SerializeField] private Collider trigger;
        [SerializeField] private int elixirHealing = 40;

        private PlayerStateMachine player;
        private bool offersWeapon;
        private WeaponKind weapon;
        private string prompt;
        private Vector3 restPosition;

        public bool IsCollected { get; private set; }
        public bool OffersWeapon => offersWeapon;
        public WeaponKind Weapon => weapon;
        public string Prompt => prompt;
        public bool CanInteract => !IsCollected;

        public void Configure(PlayerStateMachine playerState, WeaponKind preferred)
        {
            player = playerState;
            restPosition = floatRoot.localPosition;
            weapon = preferred;
            offersWeapon = !player.Inventory.Owns(preferred) || player.Inventory.TryGetNextMissing(out weapon);
            for (var i = 0; i < weaponModels.Length; i++) weaponModels[i].SetActive(offersWeapon && i == (int)weapon);
            elixirModel.SetActive(!offersWeapon);
            prompt = offersWeapon ? $"[E] Pegar {player.Inventory.Definition(weapon).DisplayName}" : "[E] Beber elixir de cura";
        }

        public void Interact()
        {
            if (IsCollected) return;
            if (offersWeapon) player.Inventory.Add(weapon);
            else player.Health.Heal(elixirHealing);
            IsCollected = true;
            trigger.enabled = false;
            floatRoot.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (IsCollected) return;
            floatRoot.Rotate(Vector3.up, 70f * Time.deltaTime, Space.World);
            floatRoot.localPosition = restPosition + Vector3.up * (Mathf.Sin(Time.time * 2.2f) * 0.1f);
        }
    }
}
