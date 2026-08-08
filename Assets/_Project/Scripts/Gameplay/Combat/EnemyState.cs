namespace Project.Gameplay.Combat
{
    /// <summary>
    /// Stany prostej maszyny stanów <see cref="EnemyAI"/>: patrolowanie, pościg za graczem
    /// i atak z zasięgu.
    /// </summary>
    public enum EnemyState
    {
        Patrol,
        Chase,
        Attack
    }
}
