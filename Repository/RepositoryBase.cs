
using Contracts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Repository;

public class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    protected DataContext DataContext;

    public RepositoryBase(DataContext dataContext)
    {
        DataContext = dataContext;
    }

    public void Create(T entity) => DataContext.Set<T>().Add(entity);


    public void Delete(T entity) => DataContext.Set<T>().Remove(entity);

    public IQueryable<T> FindAll(bool trackChanges) => trackChanges
        ? DataContext.Set<T>()
        : DataContext.Set<T>().AsNoTracking();


    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges) => trackChanges
        ? DataContext.Set<T>().Where(expression)
        : DataContext.Set<T>().Where(expression).AsNoTracking();


    public void Update(T entity) => DataContext.Set<T>().Update(entity);
}
