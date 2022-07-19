namespace mark.davison.common.persistence.EntityDefaulter;

public interface IEntityDefaulter<T> where T : BaseEntity
{
    Task DefaultAsync(T entity, User user);

}