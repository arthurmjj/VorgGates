using Unity.Cinemachine;
using UnityEngine;

namespace VorgGates.Presentation
{
    public sealed class CameraRig : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera cinemachine;

        /// <summary>Skips damping for one update, so a teleport never shows the camera flying across the map.</summary>
        public void Snap() => cinemachine.PreviousStateIsValid = false;
    }
}
