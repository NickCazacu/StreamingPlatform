using System.Collections.Generic;
using System.Linq;
using StreamingPlatform.Interfaces;
using StreamingPlatform.Models;

namespace StreamingPlatform.Repositories
{
    public class InMemoryRepository<T> : IRepository<T> where T : Entity
    {
        private readonly List<T> _storage = new List<T>();

        public void Add(T entity)
        {
            _storage.Add(entity);
        }

        public T GetById(int id)
        {
            return _storage.FirstOrDefault(e => e.Id == id);
        }

        public IEnumerable<T> GetAll()
        {
            return _storage.ToList();
        }

        public void Remove(int id)
        {
            var entity = GetById(id);
            if (entity != null)
                _storage.Remove(entity);
        }
    }
}
