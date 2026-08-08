using Project.Core.Systems;
using UnityEngine;

namespace Project.Gameplay.Player
{
    /// <summary>
    /// Pierwszy przykładowy IGameSystem, weryfikujący cały pipeline Bootstrapper -> GameManager ->
    /// GameStateMachine -> GameSystemsManager -> IGameSystem w scenie Level_01. Czysta klasa C#
    /// (nie MonoBehaviour) - jedyne zależności od silnika to referencje do CharacterController i Transform,
    /// wstrzyknięte z zewnątrz przez <see cref="PlayerMotorBridge"/>. Odczyt Input Systemu odbywa się
    /// wyłącznie w bridge'u, ten system operuje tylko na już przetworzonych wartościach.
    /// </summary>
    public class PlayerMotorSystem : IGameSystem
    {
        private readonly CharacterController _controller;
        private readonly Transform _transform;
        private readonly float _moveSpeed;
        private readonly float _gravity;
        private readonly float _jumpHeight;
        private readonly float _sprintMultiplier;

        private Vector2 _moveInput;
        private bool _jumpRequested;
        private bool _isSprinting;
        private float _verticalVelocity;

        public PlayerMotorSystem(CharacterController controller, Transform transform, float moveSpeed, float gravity, float jumpHeight, float sprintMultiplier = 1.6f)
        {
            _controller = controller;
            _transform = transform;
            _moveSpeed = moveSpeed;
            _gravity = gravity;
            _jumpHeight = jumpHeight;
            _sprintMultiplier = sprintMultiplier;
        }

        /// <summary>Wywoływane przez bridge co klatkę z aktualną wartością akcji Move (-1..1 na oś).</summary>
        public void SetMoveInput(Vector2 moveInput)
        {
            _moveInput = moveInput;
        }

        /// <summary>
        /// Wywoływane przez bridge co klatkę - czy w TEJ klatce należy zastosować sprintMultiplier.
        /// Bridge już przesądził wszystko, co wymaga danych spoza tego czystego systemu (input Shift,
        /// GameplayInputLock, TrySpendStamina na PlayerStatsSystem) - ten system tylko aplikuje mnożnik
        /// do już obliczonej prędkości, nie wie nic o Staminie ani inpucie.
        /// </summary>
        public void SetSprinting(bool isSprinting)
        {
            _isSprinting = isSprinting;
        }

        /// <summary>Wywoływane przez bridge, gdy akcja Jump zostanie wykonana.</summary>
        public void RequestJump()
        {
            _jumpRequested = true;
        }

        public void Initialize()
        {
            _verticalVelocity = 0f;
        }

        public void Tick(float deltaTime)
        {
            float speed = _isSprinting ? _moveSpeed * _sprintMultiplier : _moveSpeed;
            Vector3 move = (_transform.right * _moveInput.x + _transform.forward * _moveInput.y) * speed;

            if (_controller.isGrounded)
            {
                if (_verticalVelocity < 0f)
                {
                    _verticalVelocity = -2f;
                }

                if (_jumpRequested)
                {
                    _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
                }
            }
            else
            {
                _verticalVelocity += _gravity * deltaTime;
            }

            _jumpRequested = false;

            move.y = _verticalVelocity;
            _controller.Move(move * deltaTime);
        }

        public void FixedTick(float fixedDeltaTime)
        {
        }

        public void Shutdown()
        {
        }
    }
}
