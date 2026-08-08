using Project.Core.Systems;
using UnityEngine;

namespace Project.Gameplay.Camera
{
    /// <summary>
    /// Czysta klasa C# licząca orbitalną rotację kamery trzecioosobowej (yaw/pitch) na podstawie
    /// inputu z akcji Look oraz auto-recenteringu po okresie bezczynności. Zero zależności od
    /// MonoBehaviour/Transform - <see cref="CameraOrbitBridge"/> odczytuje <see cref="CurrentRotation"/>
    /// co klatkę i aplikuje ją na CameraAnchor.
    /// </summary>
    public class CameraOrbitSystem : IGameSystem
    {
        private const float InputThresholdSqr = 0.0001f;

        private readonly float _sensitivity;
        private readonly float _pitchMin;
        private readonly float _pitchMax;

        private float _yaw;
        private float _pitch;

        public Quaternion CurrentRotation => Quaternion.Euler(_pitch, _yaw, 0f);

        public float Yaw => _yaw;
        public float Pitch => _pitch;

        public CameraOrbitSystem(float sensitivity, float pitchMin, float pitchMax)
        {
            _sensitivity = sensitivity;
            _pitchMin = pitchMin;
            _pitchMax = pitchMax;
        }

        /// <summary>Ustawia startowy yaw, żeby uniknąć "skoku" postaci przy starcie sceny, gdy jej
        /// rotacja startowa != 0.</summary>
        public void SetInitialYaw(float yaw)
        {
            _yaw = yaw;
        }

        /// <summary>Wywoływane przez bridge co klatkę z aktualną wartością akcji Look.</summary>
        public void AddLookInput(Vector2 delta)
        {
            if (delta.sqrMagnitude <= InputThresholdSqr)
            {
                return;
            }

            _yaw = Mathf.Repeat(_yaw + delta.x * _sensitivity, 360f);
            _pitch = Mathf.Clamp(_pitch - delta.y * _sensitivity, _pitchMin, _pitchMax);
        }

        public void Initialize()
        {
            _yaw = 0f;
            _pitch = 0f;
        }

        public void Tick(float deltaTime)
        {
        }

        public void FixedTick(float fixedDeltaTime)
        {
        }

        public void Shutdown()
        {
        }
    }
}
