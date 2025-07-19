using System;
using System.Linq.Expressions;
using Core.Interfaces;

namespace Core.Specifications;

public class BaseSpecifications<T>(Expression<Func<T, bool>>? criteria) : ISpecification<T>
{
    // empty constructor ProductSpecification
    protected BaseSpecifications() : this(null) { }

    public Expression<Func<T, bool>>? Criteria => criteria;

    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }


    public bool IsDistinct { get; private set; }

    public int Take { get; private set; }

    public int Skip { get; private set; }

    public bool IsPagingEnabled { get; private set; }

    public List<Expression<Func<T, object>>> Indcludes { get; } = [];
    public List<string> IncludeStrings { get; } = [];

    protected void AppyDistinct()
    {
        IsDistinct = true;
    }

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    public IQueryable<T> ApplyCriteria(IQueryable<T> query)
    {
        if (Criteria != null)
        {
            query = query.Where(Criteria); // x => x.Brand == brand
        }
        return query;

    }

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Indcludes.Add(includeExpression);
    }

    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString); // For ThenInclude
    }

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
    protected BaseSpecifications() : this(null!) {}
    public Expression<Func<T, TResult>>? Select {get; private set; }
    protected void AddSelect(Expression<Func<T, TResult>> selectExpression)
    {
        Select = selectExpression;
    }
}