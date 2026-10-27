// source https://github.com/UnityCommunity/UnitySingleton.git
namespace UnityCommunity.UnitySingleton
{
    /// <summary>
    /// The singleton interface.
    /// </summary>
    public interface ISingleton
    {
        public void InitializeSingleton();

        public void ClearSingleton();
    }
}