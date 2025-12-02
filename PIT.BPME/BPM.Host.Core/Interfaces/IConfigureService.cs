namespace BPME.BPM.Host.Core.Interfaces
{
    /// <summary>
    /// Сервис управления конфигурациями.
    ///
    /// Слой бизнес-логики поверх репозитория:
    /// - Валидация конфигураций
    /// - Работа с кэшем
    /// - Логирование операций
    ///
    /// На схеме это IConfigureService&lt;TConfig&gt;.
    ///
    /// Lifetime: Scoped
    /// </summary>
    /// <typeparam name="TConfig">Тип конфигурации</typeparam>
    public interface IConfigureService<TConfig> where TConfig : class
    {
        /// <summary>
        /// Создать новую конфигурацию
        /// </summary>
        Task<TConfig> CreateAsync(TConfig config, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновить конфигурацию
        /// </summary>
        Task<TConfig> UpdateAsync(TConfig config, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить конфигурацию по публичному идентификатору
        /// </summary>
        Task<TConfig?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить все активные конфигурации
        /// </summary>
        Task<IEnumerable<TConfig>> GetAllActiveAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Удалить конфигурацию
        /// </summary>
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверить существование конфигурации
        /// </summary>
        Task<bool> ExistsAsync(string publicId, CancellationToken cancellationToken = default);
    }
}
