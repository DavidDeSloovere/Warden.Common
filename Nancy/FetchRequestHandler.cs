﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Nancy;
using Nancy.Responses.Negotiation;
using NLog;
using Warden.Common.Extensions;
using Warden.Common.Queries;
using Warden.Common.Types;

namespace Warden.Common.Nancy
{
    public class FetchRequestHandler<TQuery, TResult> where TQuery : IQuery, new() where TResult : class
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly string PageParameter = "page";
        private readonly TQuery _query;
        private readonly Func<TQuery, Task<Maybe<TResult>>> _fetch;
        private readonly Func<TQuery, Task<Maybe<PagedResult<TResult>>>> _fetchCollection;
        private readonly Negotiator _negotiator;
        private readonly Url _url;
        private readonly IMapper _autoMapper;
        private Func<TResult, object> _mapper;

        public FetchRequestHandler(TQuery query, Func<TQuery, Task<Maybe<TResult>>> fetch, Negotiator negotiator,
            Url url, IMapper autoMapper = null)
        {
            _query = query;
            _fetch = fetch;
            _negotiator = negotiator;
            _url = url;
            _autoMapper = autoMapper;
        }

        public FetchRequestHandler(TQuery query, Func<TQuery, Task<Maybe<PagedResult<TResult>>>> fetchCollection,
            Negotiator negotiator, Url url, IMapper autoMapper = null)
        {
            _query = query;
            _fetchCollection = fetchCollection;
            _negotiator = negotiator;
            _url = url;
            _autoMapper = autoMapper;
        }

        public FetchRequestHandler<TQuery, TResult> MapTo(Func<TResult, object> mapper)
        {
            _mapper = mapper;

            return this;
        }

        public FetchRequestHandler<TQuery, TResult> MapTo<T>()
        {
            _mapper = x => _autoMapper.Map<T>(x);

            return this;
        }

        public async Task<Negotiator> HandleAsync()
            => _fetch == null ? await HandleCollectionAsync() : await HandleResultAsync();

        private async Task<Negotiator> HandleResultAsync()
        {
            var result = await _fetch(_query);
            if (result.HasNoValue)
                return _negotiator.WithStatusCode(HttpStatusCode.NotFound);

            var value = result.Value;
            if (value is Response)
                return _negotiator.WithModel(value);

            return FromResult(result);
        }

        private async Task<Negotiator> HandleCollectionAsync()
        {
            var result = await _fetchCollection(_query);

            return FromPagedResult(result);
        }

        private Negotiator FromResult(Maybe<TResult> result)
        {
            if (result.HasNoValue)
            {
                _logger.Debug($"Result of {_query.GetType().Name} has no value {typeof(TResult).Name}");
                return _negotiator.WithStatusCode(HttpStatusCode.NotFound);
            }
            _logger.Debug($"Result of {_query.GetType().Name} contains {typeof(TResult).Name} object");
            var model = _mapper == null ? result.Value : _mapper(result.Value);

            return _negotiator.WithModel(model);
        }

        private Negotiator FromPagedResult(Maybe<PagedResult<TResult>> result)
        {
            if (result.HasNoValue)
            {
                _logger.Debug($"Result of {_query.GetType().Name} has no value {typeof(TResult).Name}");
                return _negotiator.WithModel(new List<object>());
            }
            _logger.Debug($"Result of {_query.GetType().Name} contains {result.Value.TotalResults} {typeof(TResult).Name} elements");
            var model = _mapper == null ? result.Value.Items : result.Value.Items.Select(x => _mapper(x));

            return _negotiator.WithModel(model)
                .WithHeader("Link", GetLinkHeader(result.Value))
                .WithHeader("X-Total-Count", result.Value.TotalResults.ToString());
        }

        private string GetLinkHeader(PagedResultBase result)
        {
            var first = GetPageLink(result.CurrentPage, 1);
            var last = GetPageLink(result.CurrentPage, result.TotalPages);
            var prev = string.Empty;
            var next = string.Empty;
            if (result.CurrentPage > 1 && result.CurrentPage <= result.TotalPages)
                prev = GetPageLink(result.CurrentPage, result.CurrentPage - 1);
            if (result.CurrentPage < result.TotalPages)
                next = GetPageLink(result.CurrentPage, result.CurrentPage + 1);

            return $"{FormatLink(next, "next")}{FormatLink(last, "last")}" +
                   $"{FormatLink(first, "first")}{FormatLink(prev, "prev")}";
        }

        private string GetPageLink(int currentPage, int page)
        {
            var url = _url.ToString();
            var sign = _url.Query.Empty() ? "&" : "?";
            var pageArg = $"{PageParameter}={page}";
            var link = url.Contains($"{PageParameter}=")
                ? url.Replace($"{PageParameter}={currentPage}", pageArg)
                : url += $"{sign}{pageArg}";

            return link;
        }

        private string FormatLink(string url, string rel)
            => url.Empty() ? string.Empty : $"<{url}>; rel=\"{rel}\",";
    }
}