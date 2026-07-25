using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    public interface IEnemy
    {
        bool IsActive { get; }
        void Initialize(Transform target);
        void Activate();
        void Deactivate();
    }
}
