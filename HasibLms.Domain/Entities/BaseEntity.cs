namespace HasibLms.Domain.Entities;

public class BaseEntity
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public DateTime CreatedDate { get; set; } = DateTime.Now;
	public Guid? CreatedBy { get; set; }
	public DateTime LastModifiedDate { get; set; } = DateTime.Now;
	public Guid? LastModifiedBy { get; set; }
	public bool IsDeleted { get; set; } = false;
}