using System.Collections.Generic;

namespace Restaurant.Services.Repositories
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetById(int id);
        int Add(T entity);
        bool Update(T entity);
        bool Delete(int id);
    }
}