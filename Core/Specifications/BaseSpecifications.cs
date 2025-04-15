using System;
using System.Linq.Expressions;
using Core.Interfaces;

namespace Core.Specifications;

public class BaseSpecifications<T>(Expression<Func<T, bool>>?  criteria) : ISpecification<T>
{
    // empty constructor ProductSpecification
    protected BaseSpecifications() : this(null) {}

    public Expression<Func<T, bool>>?  Criteria => criteria;

    public Expression<Func<T, object>>? OrderBy {get; private set;} 
    public Expression<Func<T, object>>? OrderByDescending {get; private set;}

    protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    protected void AddOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
    }
}


public class BaseSpecifications<T, TResult>(Expression<Func<T, bool>>? criteria) : BaseSpecifications<T>(criteria), ISpecification<T, TResult>
{
    public Expression<Func<T, TResult>>? Select {get; private set; }
    protected void AddSelect(Expression<Func<T, TResult>> selectExpression)
    {
        Select = selectExpression;
    }
}