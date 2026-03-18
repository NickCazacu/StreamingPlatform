using System;

namespace StreamingPlatform.Models
{
    public abstract class Entity
    {
        private static int _idCounter = 0;

        public int Id { get; }
        public DateTime CreatedAt { get; }

        protected Entity()
        {
            _idCounter++;
            Id = _idCounter;
            CreatedAt = DateTime.Now;
        }

        public abstract string GetInfo();
    }
}
