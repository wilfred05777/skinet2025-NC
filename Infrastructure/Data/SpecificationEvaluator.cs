using System;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Data;

public class SpecificationEvalutaor<T> where T: BaseEntity
{
    public static IQueryable<T> GetQuery(IQueryable<T> query, ISpecification<T> spec)
    {
        if( spec.Criteria !=null)
        {
            query = query.Where(spec.Criteria); // x => x.Brand == brand
        }

        return query;
        
    }
}
