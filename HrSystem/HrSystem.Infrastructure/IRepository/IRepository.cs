using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(Guid id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
       
    }