namespace Game.Foundation
{
    public interface ISaveService
    {
        bool HasSave();
        GameSaveData Load();
        void Save(GameSaveData data);
        void DeleteSave();
    }
}