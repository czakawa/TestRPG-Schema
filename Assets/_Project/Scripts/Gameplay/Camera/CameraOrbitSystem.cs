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
        private readonly float _recenterDelay;
        private readonly float _recenterSpeed;

        private float _yaw;
        private float _pitch;
        private float _timeSinceLastInput;
        private bool _inputReceivedThisFrame;

        public Quaternion CurrentRotation => Quaternion.Euler(_pitch, _yaw, 0f);

        public CameraOrbitSystem(float sensitivity, float pitchMin, float pitchMax, float recenterDelay, float recenterSpeed)
        {
            _sensitivity = sensitivity;
            _pitchMin = pitchMin;
            _pitchMax = pitchMax;
            _recenterDelay = recenterDelay;
            _recenterSpeed = recenterSpeed;
        }

        /// <summary>Wywoływane przez bridge co klatkę z aktualną wartością akcji Look.</summary>
        public void AddLookInput(Vector2 delta)
        {
            if (delta.sqrMagnitude <= InputThresholdSqr)
            {
                return;
            }

            _yaw += delta.x * _sensitivity;
            _pitch = Mathf.Clamp(_pitch - delta.y * _sensitivity, _pitchMin, _pitchMax);
            _timeSinceLastInput = 0f;
            _inputReceivedThisFrame = true;
        }

        public void Initialize()
        {
            _yaw = 0f;
            _pitch = 0f;
            _timeSinceLastInput = 0f;
            _inputReceivedThisFrame = false;
        }

        public void Tick(float deltaTime)
        {
            if (!_inputReceivedThisFrame)
            {
                _timeSinceLastInput += deltaTime;
            }

            _inputReceivedThisFrame = false;

            if (_timeSinceLastInput > _recenterDelay)
            {
                float t = _recenterSpeed * deltaTime;
                _yaw = Mathf.LerpAngle(_yaw, 0f, t);
                _pitch = Mathf.Lerp(_pitch, 0f, t);
            }
        }

        public void FixedTick(float fixedDeltaTime)
        {
        }

        public void Shutdown()
        {
        }
    }
}
