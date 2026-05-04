using TowerDefence.Core;

namespace TowerDefence.Interfaces
{
    public interface IDamageable
    {
        void TakeDamage(float amount);
        bool IsDead { get; }
        Side GetSide();
    }
}
