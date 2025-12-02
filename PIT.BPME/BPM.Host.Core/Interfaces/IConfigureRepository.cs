namespace BPME.BPM.Host.Core.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с конфигурациями.
    ///
    /// Паттерн Repository — абстрагирует доступ к хранилищу данных.
    /// Реализация может работать с:
    /// - PostgreSQL (через EF Core)
    /// - MongoDB
    /// - In-Memory (для тестов)
    /// - Файловой системой
    ///
    /// На схеме это IConfigureRepository&lt;TConfig&gt;.
    ///
    /// Lifetime: Scoped (создаётся для каждого запроса)
    /// </summary>
    /// <typeparam name="TConfig">Тип конфигурации</typeparam>
    public interface IConfigureRepository<TConfig> where TConfig : class
    {
        /// <summary>
        /// Создать новую конфигурацию
        /// </summary>
        /// <param name="config">Конфигурация для создания</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Созданная конфигурация с заполненным Id</returns>
        Task<TConfig> CreateAsync(TConfig config, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновить существующую конфигурацию
        /// </summary>
        /// <param name="config">Конфигурация с обновлёнными данными</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Обновлённая конфигурация</returns>
        Task<TConfig> UpdateAsync(TConfig config, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить конфигурацию по Id
        /// </summary>
        /// <param name="id">Идентификатор конфигурации</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Конфигурация или null если не найдена</returns>
        Task<TConfig?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить конфигурацию по публичному идентификатору
        /// </summary>
        /// <param name="publicId">Публичный идентификатор</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Конфигурация или null если не найдена</returns>
        Task<TConfig?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить все конфигурации
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список всех конфигураций</returns>
        Task<IEnumerable<TConfig>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить активные конфигурации
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список активных конфигураций</returns>
        Task<IEnumerable<TConfig>> GetActiveAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Удалить конфигурацию по Id
        /// </summary>
        /// <param name="id">Идентификатор конфигурации</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>true если удалена, false если не найдена</returns>
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверить существование конфигурации
        /// </summary>
        /// <param name="publicId">Публичный идентификатор</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>true если существует</returns>
        Task<bool> ExistsAsync(string publicId, CancellationToken cancellationToken = default);
    }
}
