using System.Collections.Generic;

namespace StreamingPlatform.Interfaces
{
    public interface IRepository<T> where T : class
    {
        void Add(T entity);
        T GetById(int id);
        IEnumerable<T> GetAll();
        void Remove(int id);
    }
}
