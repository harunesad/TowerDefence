namespace TowerDefence.Interfaces
{
    public interface IDamageable
    {
        void TakeDamage(float amount);
        bool IsDead { get; }
    }
}
