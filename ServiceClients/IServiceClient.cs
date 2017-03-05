﻿using System.IO;
using System.Threading.Tasks;
using Warden.Common.Queries;
using Warden.Common.Security;
using Warden.Common.Types;

namespace Warden.Common.ServiceClients
{
    public interface IServiceClient
    {
        void SetSettings(ServiceSettings serviceSettings);
        Task<Maybe<T>> GetAsync<T>(string name, string endpoint) 
            where T : class;
        Task<Maybe<dynamic>> GetAsync(string name, string endpoint);
        Task<Maybe<Stream>> GetStreamAsync(string name, string endpoint);
        Task<Maybe<PagedResult<T>>> GetCollectionAsync<T>(string name, string endpoint) 
            where T : class;
        Task<Maybe<PagedResult<dynamic>>> GetCollectionAsync(string name, string endpoint);
        Task<Maybe<PagedResult<TResult>>> GetFilteredCollectionAsync<TQuery,TResult>(TQuery query, 
            string name, string endpoint)
            where TResult : class where TQuery : class, IPagedQuery;
        Task<Maybe<PagedResult<dynamic>>> GetFilteredCollectionAsync<TQuery>(TQuery query,
            string name, string endpoint)
            where TQuery : class, IPagedQuery;
    }
}