﻿namespace Warden.Common.Queries
{
    public interface IPagedQuery : IQuery
    {
        int Page { get; }
        int Results { get; }
    }
}