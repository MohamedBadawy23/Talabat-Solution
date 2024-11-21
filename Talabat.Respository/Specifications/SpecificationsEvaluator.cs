using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.Specifications;

namespace Talabat.Respository.Specifications
{
    public class SpecificationsEvaluator<TEntity> where TEntity : BaseEntity
    {

        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecifications<TEntity> spec)
        {
            var query = inputQuery; // _context.Set<Order>()
            // _context.Set<Product>()

            if(spec.Criteira is not null)
            {
                query = query.Where(spec.Criteira);
            }
            // _context.Set<Product>().Where(P => P.Id == 4)

            // _context.Set<Product>()
            if(spec.OrderBy is not null) // P => P.Name
            {
                query = query.OrderBy(spec.OrderBy);
            }
            // _context.Set<Product>().OrderBy(P => P.Name)

            if(spec.OrderByDesc is not null)
            {
                query = query.OrderByDescending(spec.OrderByDesc);
            }

            // includes
            // 1. P => P.Brands
            // 2. P => P.Category

            if (spec.IsPaginationEnabled)
            {
                query = query.Skip(spec.Skip).Take(spec.Take);
            }



            // ahmed ali omar mohamoud

            query = spec.Includes.Aggregate(query, (currentQuery, includeExpression) => currentQuery.Include(includeExpression));

            // // _context.Set<Product>().OrderBy(P => P.Name).Include(P => P.Brand).Include(P => P.Category)

            // _context.Set<Product>().Where(P =>(!brandId.HasValue || P.BrandId == brandId) &&(!categoryId.HasValue || P.CategoryId == categoryId)).OrderBy(P => P.Name).Include(P => P.Brands).Include(P => P.Category).count()


            return query;
        }

    }
}



//_context.Products.Include(P => P.Brand).Include(P => P.Category).ToListAsync();

//_context.Products.Where(P=>P.Id == id).Include(P => P.Brand).Include(P => P.Category).FirstOrDefaultAsync();
