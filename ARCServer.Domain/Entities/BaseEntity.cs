namespace ARCServer.Domain.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }

        public DateTime CreateDate { get; set; }

        public int? CreatorId { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public int? UpdaterId { get; set; }

        public DateTime? DeletedDate { get; set; }

        public int? DeletorId { get; set; }

        /// <summary>
        /// 0 = active. On soft delete, set to the entity's own Id.
        /// </summary>
        public int Deleted { get; set; }
    }
}
