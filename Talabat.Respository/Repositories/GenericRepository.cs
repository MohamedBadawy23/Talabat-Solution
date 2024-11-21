using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Specifications;
using Talabat.Respository.Data;
using Talabat.Respository.Specifications;

namespace Talabat.Repository.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly StoreDbContext _context;

        public GenericRepository(StoreDbContext context) // ASK CLR for Creating Object from DbContext Implicitly
        {
            _context = context;
        }



        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            if (typeof(T) == typeof(Product))
            {
                return (IReadOnlyList<T>)await _context.Products.OrderByDescending(P => P.Price).Skip(400).Take(50).Include(P => P.ProductBrand).Include(P => P.ProductType).ToListAsync();
            }
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecifications<T> spec)
        {
            return await SpecificationsEvaluator<T>.GetQuery(_context.Set<T>(), spec).ToListAsync();
            // _context.Set<Product>().Where(P => P.id == 10).OrderBy(P => P.Name).Skip(400).Take(50).Include(P => P.Brands).Include(P => P.Category).ToListAsync();
        }

        public async Task<T?> GetAsync(int id)
        {
            if (typeof(T) == typeof(Product))
            {
                return await _context.Products.Where(P => P.Id == id).Include(P => P.ProductBrand).Include(P => P.ProductType).FirstOrDefaultAsync() as T;
            }
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<int> GetCountAsync(ISpecifications<T> spec)
        {
            return await SpecificationsEvaluator<T>.GetQuery(_context.Set<T>(), spec).CountAsync();
        }

        public Task<T?> GetWithSpecAsync(ISpecifications<T> spec)
        {
            return SpecificationsEvaluator<T>.GetQuery(_context.Set<T>(), spec).FirstOrDefaultAsync();

        }
        public async Task AddAsync(T item) => await _context.Set<T>().AddAsync(item);
        public void Update(T item) => _context.Set<T>().Update(item);
        public void Delete(T item) => _context.Set<T>().Remove(item);
        
    }
}
