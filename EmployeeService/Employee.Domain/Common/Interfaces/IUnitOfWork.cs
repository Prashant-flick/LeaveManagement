namespace Employee.Domain.Common.Interfaces;
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
}