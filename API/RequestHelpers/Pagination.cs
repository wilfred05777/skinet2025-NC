using System;

namespace API.RequestHelpers;

public class Pagination<T>(int PageIndex, int pageSize, int count, IReadOnlyList<T> data) 
{
    public int PageIndex { get; set; } = PageIndex;
    public int PageSize {get; set;} = pageSize;
    public int Count { get; set; } = count;
    public IReadOnlyList<T> Data {get; set;} = data;
}
