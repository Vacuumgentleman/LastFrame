
namespace Senses2026
{
    public interface IUpdate
    {
        bool NeedsUpdate();
        void PerformUpdate(float time);
        /*
        bool NeedsFixedUpdate { get; set; }
        void PerformFixedUpdate(float time);
        bool NeedsFinalUpdate { get; set; }
        void PerformFinalUpdate(float time);
        bool NeedsLateUpdate { get; set; }
        void PerformLateUpdate(float time);
        */
    }
}

