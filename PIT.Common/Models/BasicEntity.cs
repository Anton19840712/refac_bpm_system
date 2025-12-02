using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIT.Common.Models
{
    /// <summary>
    /// Базовая сущность, используемая в системе
    /// </summary>
    public class BasicEntity : IEntity
    {
        public BasicEntity() { }
        public BasicEntity(BasicEntity be)
        {
            Id = be.Id;
            CreatedAt = be.CreatedAt;
            UpdatedAt = be.UpdatedAt;
            CreatedBy = be.CreatedBy;
            UpdatedBy = be.UpdatedBy;
            IsDeleted = be.IsDeleted;
            DisplayName = be.DisplayName;
        }
        /// <summary>
        /// Уникальный Идентификатор (Ключ)
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        /// <summary>
        /// Отображаемое значение
        /// </summary>
        public string? DisplayName { get; set; } = default!;
        /// <summary>
        /// Когда создано
        /// </summary>
        public long? CreatedAt { get; set; } = null!;
        /// <summary>
        /// Когда обновлено
        /// </summary>
        public long? UpdatedAt { get; set; } = null!;
        /// <summary>
        /// Кем создано
        /// </summary>
        public Guid? CreatedBy { get; set; } = null!;
        /// <summary>
        /// Кем обновлено
        /// </summary>
        public Guid? UpdatedBy { get; set; } = null!;
        /// <summary>
        /// Флаг мягкого удаления
        /// </summary>
        public bool? IsDeleted { get; set; } = null!;


        /// <summary>
        /// Обновление сущности
        /// </summary>
        /// <param name="accountId"></param>
        public void IEntityUpdate(Guid? useId)
        {
            UpdatedAt = DateTimeOffset.Now.ToUnixTimeSeconds();
            if (useId != null)
            {
                UpdatedBy = useId;
            }
        }
        /// <summary>
        /// Создание сущности
        /// </summary>
        /// <param name="accountId"></param>
        public void IEntityCreate(Guid? useId)
        {
            CreatedAt = DateTimeOffset.Now.ToUnixTimeSeconds();
            if (useId != null)
            {
                CreatedBy = useId;
            }
        }
    }
}
