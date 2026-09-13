using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CezStudentAssistant.Application.Dtos.Common;
using CezStudentAssistant.Application.Interfaces.CQRS;
using Microsoft.EntityFrameworkCore;

namespace CezStudentAssistant.Application.Queries;

public abstract class BasePagedQuery<TItem> : IQuery<PagedResultDto<TItem>>, IUserRequest
{
    public Guid UserId { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? SearchTerm { get; set; }
}

public abstract class BasePagedQueryHandler<TQuery, TEntity, TItem> : BaseQueryHandler<TQuery, PagedResultDto<TItem>>
    where TQuery : BasePagedQuery<TItem>
{
    protected abstract Task<IQueryable<TEntity>> GetQueryableAsync(TQuery query, CancellationToken ct);

    protected abstract TItem MapToDto(TEntity entity, TQuery query);

    protected override async Task<PagedResultDto<TItem>> ExecuteAsync(TQuery query, CancellationToken ct)
    {
        var queryable = await GetQueryableAsync(query, ct);

        var totalCount = await queryable.CountAsync(ct);

        var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 10;

        var entities = await queryable
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = entities.Select(e => MapToDto(e, query)).ToList();

        return new PagedResultDto<TItem>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}

public abstract class BasePagedQueryHandler<TQuery, TItem> : BasePagedQueryHandler<TQuery, TItem, TItem>
    where TQuery : BasePagedQuery<TItem>
{
    protected override TItem MapToDto(TItem entity, TQuery query) => entity;
}
