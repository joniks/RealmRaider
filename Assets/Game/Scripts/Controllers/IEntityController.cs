namespace RealmRaiders.Controllers
{
    public interface IEntityController
    {
        bool IsActive { get; }
        void SetControl(bool active);
        void Tick();
    }
}
