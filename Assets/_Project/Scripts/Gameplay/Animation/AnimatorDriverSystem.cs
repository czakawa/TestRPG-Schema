using Project.Core.Systems;
using UnityEngine;

namespace Project.Gameplay.Animation
{
    /// <summary>
    /// Czysta klasa C# sterująca parametrami Animatora: Speed (float, blend tree Idle/Jog/Run)
    /// oraz triggery Attack/Death. Zero logiki decyzyjnej - tylko aplikuje wartości otrzymane
    /// z bridge'a, analogicznie do PlayerMotorSystem/CameraOrbitSystem.
    /// </summary>
    public class AnimatorDriverSystem : IGameSystem
    {
        private static readonly int VelocityXParam = Animator.StringToHash("VelocityX");
        private static readonly int VelocityZParam = Animator.StringToHash("VelocityZ");
        private static readonly int AttackTriggerParam = Animator.StringToHash("Attack");
        private static readonly int DeathTriggerParam = Animator.StringToHash("Death");
        private static readonly int IsArmedParam = Animator.StringToHash("IsArmed");
        private static readonly int DrawTriggerParam = Animator.StringToHash("Draw");
        private static readonly int SheatheTriggerParam = Animator.StringToHash("Sheathe");

        private readonly Animator _animator;
        private readonly float _speedDampTime;

        private float _targetVelocityX;
        private float _targetVelocityZ;

        public AnimatorDriverSystem(Animator animator, float speedDampTime)
        {
            _animator = animator;
            _speedDampTime = speedDampTime;
        }

        /// <summary>Wywoływane przez bridge co klatkę z aktualną prędkością poziomą gracza.</summary>
        public void SetMoveVelocity(float velocityX, float velocityZ)
        {
            _targetVelocityX = velocityX;
            _targetVelocityZ = velocityZ;
        }

        public void TriggerAttack()
        {
            _animator.SetTrigger(AttackTriggerParam);
        }

        public void TriggerDeath()
        {
            _animator.SetTrigger(DeathTriggerParam);
        }

        public void SetArmed(bool isArmed)
        {
            _animator.SetBool(IsArmedParam, isArmed);
        }

        public void TriggerDraw()
        {
            _animator.SetTrigger(DrawTriggerParam);
        }

        public void TriggerSheathe()
        {
            _animator.SetTrigger(SheatheTriggerParam);
        }

        public void Initialize()
        {
            _targetVelocityX = 0f;
            _targetVelocityZ = 0f;
        }

        public void Tick(float deltaTime)
        {
            _animator.SetFloat(VelocityXParam, _targetVelocityX, _speedDampTime, deltaTime);
            _animator.SetFloat(VelocityZParam, _targetVelocityZ, _speedDampTime, deltaTime);
        }

        public void FixedTick(float fixedDeltaTime)
        {
        }

        public void Shutdown()
        {
        }
    }
}
