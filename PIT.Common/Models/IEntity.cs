namespace PIT.Common.Models
{
    /// <summary>
    /// Интерфейс для базовой сущности, используемый для отслеживания создание и изменение сущности
    /// </summary>
    public interface IEntity
    {
        void IEntityCreate(Guid? useId);
        void IEntityUpdate(Guid? useId);
    }
}
