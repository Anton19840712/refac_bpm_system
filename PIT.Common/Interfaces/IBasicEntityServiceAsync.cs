using PIT.Common.Models;

namespace PIT.Common.Interfaces
{
    /// <summary>
    /// Единый и общий асинхронный интерфейс, описывающий стандартный сервис подсистемы по управлению сущностями
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности</typeparam>
    /// <typeparam name="TFilter">Тип фильтра</typeparam>
    public interface IBasicEntityServiceAsync<TEntity, TFilter> where TEntity : class,
        new() where TFilter : class
    {
        Task<TEntity?> CreateEntityAsync(TEntity entity);
        Task<TEntity?> UpdateEntityAsync(TEntity entity);
        Task<TEntity?> GetEntityByIdAsync(Guid entityId);
        Task<Page<TEntity>?> GetEntitiesAsync(TFilter filter);
        Task<TEntity?> SoftDeleteEntityAsync(Guid entityId);
        Task<TEntity?> DeleteEntityAsync(Guid entityId);
    }

    /// <summary>
    /// Единый и общий интерфейс, описывающий стандартный сервис подсистемы по управлению сущностями
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности</typeparam>
    /// <typeparam name="TFilter">Тип фильтра</typeparam>
    public interface IBasicEntityService<TEntity, TFilter> where TEntity : class,
        new() where TFilter : class
    {
        TEntity? CreateEntity(TEntity entity);
        TEntity? UpdateEntity(TEntity entity);
        TEntity? GetEntityById(Guid entityId);
        Page<TEntity> GetEntities(TFilter filter);
        TEntity? SoftDeleteEntity(Guid entityId);
        TEntity? DeleteEntity(Guid entityId);
    }
}
