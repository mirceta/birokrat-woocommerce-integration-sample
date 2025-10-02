using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using si.birokrat.next.common.conversion;
using si.birokrat.next.common.reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace si.birokrat.next.common_mvc.querying {
    public class Query<T> : QueryInternal<T, T> {
        public Query(IQueryable<T> builder, ILogger logger, string route = "", Dictionary<string, Type> relations = null)
            : base(builder, logger, route, relations) { }

        public async Task<PaginatedResult<T>> Execute(int perPage, int page) {
            int total = await Builder.CountAsync();

            if (perPage < 0) {
                perPage = 10;
            } else if (perPage == 0) {
                perPage = total;
            }

            int count = total > perPage ? perPage : total;
            int lastPage = (int)Math.Ceiling((double)total / perPage);
            int from = count > 0 ? (page - 1) * perPage + 1 : 0;
            int to = count > 0 ? from + count - 1 : 0;

            var data = count > 0 ? await Builder.Skip(from - 1).Take(count).ToArrayAsync() : new T[] { };

            return new PaginatedResult<T> {
                PerPage = perPage,
                Page = page,
                From = from,
                To = to,
                Total = total,
                LastPage = lastPage,
                Filter = _filter,
                OrderBy = _orderBy,
                OrderDirection = _orderDirection,
                Items = data.ToList()
            };
        }
    }

    public class Query<T, U> : QueryInternal<T, U> {
        public Query(IQueryable<T> builder, ILogger logger, string route = "", Dictionary<string, Type> relations = null)
            : base(builder, logger, route, relations) { }

        public async Task<PaginatedResult<U>> Execute(int perPage, int page, Func<T, U> selectFunction) {
            int total = await Builder.CountAsync();

            if (perPage < 0) {
                perPage = 10;
            } else if (perPage == 0) {
                perPage = total;
            }

            int count = total > perPage ? perPage : total;
            int lastPage = (int)Math.Ceiling((double)total / perPage);
            int from = count > 0 ? (page - 1) * perPage + 1 : 0;
            int to = count > 0 ? from + count - 1 : 0;

            var data = count > 0 ? await Builder.Skip(from - 1).Take(count).ToArrayAsync() : new T[] { };

            return new PaginatedResult<U> {
                PerPage = perPage,
                Page = page,
                From = from,
                To = to,
                Total = total,
                LastPage = lastPage,
                Filter = _filter,
                OrderBy = _orderBy,
                OrderDirection = _orderDirection,
                Items = data.Select(selectFunction).ToList()
            };
        }
    }

    public class QueryInternal<T, U> {
        private readonly Dictionary<string, Type> _relations = null;
        protected readonly string _route = string.Empty;
        protected readonly ILogger _logger = null;
        private List<string> _errors = new List<string>();
        protected string _filter = string.Empty;
        protected string _orderBy = string.Empty;
        protected string _orderDirection = string.Empty;

        public QueryInternal(IQueryable<T> builder, ILogger logger, string route, Dictionary<string, Type> relations = null) {
            Builder = builder;
            _logger = logger;
            _route = route;
            _relations = relations ?? new Dictionary<string, Type>();
        }

        public IQueryable<T> Builder { get; set; } = null;

        public bool HasErrors() {
            return _errors.Count > 0;
        }

        public string GetFirstError() {
            return HasErrors() ? _errors[0] : string.Empty;
        }

        public void SetFilter(string filter, string additionalFilter = null, string[] exclude = null) {
            if (additionalFilter != null) {
                filter = filter == null ? additionalFilter : $"{filter};{additionalFilter}";
            }
            if (!string.IsNullOrWhiteSpace(filter)) {
                string error;
                string[] parts = filter.Split(';');

                foreach (string part in parts) {
                    string[] tokens = part.Split(':');
                    if (tokens.Length != 2) {
                        error = $"Invalid part of filter query parameter: {part} (expected format: [<relation>.]<property>:<value(s)>).";
                        _errors.Add(error);
                        return;
                    }

                    string key = tokens[0].ToLower();
                    string value = tokens[1].ToLower();

                    if (exclude != null && exclude.Contains(key)) {
                        continue;
                    }

                    tokens = key.Split('.');

                    // filter on primary entity
                    if (tokens.Length == 1) {
                        AddFilter(typeof(T), key, value);
                        // filter on related entity
                    } else if (tokens.Length == 2) {
                        string relation = tokens[0].ToLower();
                        if (!_relations.ContainsKey(relation)) {
                            error = $"Invalid entity in filter query parameter: {relation} (expected format: [<relation>.]<property>:<value(s)>).";
                            _errors.Add(error);
                            return;
                        }
                        Type relationType = _relations[relation];
                        key = tokens[1];
                        AddFilter(relationType, key, value, relation);
                    } else {
                        error = $"Invalid property in filter query parameter: {key} (expected format: [<relation>.]<property>:<value(s)>).";
                        _errors.Add(error);
                        return;
                    }
                }
            }
            _filter = filter;
        }

        public void SetOrderBy(string orderBy, string orderDirection) {
            string error;

            string[] tokens = orderBy.Split('.');

            // order by primary entity property
            if (tokens.Length == 1) {
                AddOrderBy(typeof(T), orderBy, orderDirection);
                // order by related entity property
            } else if (tokens.Length == 2) {
                string relation = tokens[0].ToLower();
                if (!_relations.ContainsKey(relation)) {
                    error = $"Invalid entity in order by query parameter: {relation}.";
                    _errors.Add(error);
                    return;
                }
                Type relationType = _relations[relation];
                orderBy = tokens[1];
                AddOrderBy(relationType, orderBy, orderDirection, relation);
            } else {
                error = $"Invalid order by query parameter: {orderBy}.";
                _errors.Add(error);
                return;
            }
            _orderBy = orderBy;
            _orderDirection = orderDirection;
        }

        private void AddFilter(Type entityType, string key, string value, string relation = "") {
            string error;

            string fullKey = string.IsNullOrEmpty(relation) ? key : $"{relation}.{key}";

            if (!Property.Has(entityType, key)) {
                error = $"Invalid property in filter query parameter: {fullKey} (expected format: [<relation>.]<property>:<value(s)>).";
                _errors.Add(error);
                return;
            }

            if (value == "null") {
                Builder = Builder.Where($"it.{fullKey} == null");
            } else if (value == "notnull") {
                Builder = Builder.Where($"it.{fullKey} != null");
            } else if (value == "notempty") {
                Builder = Builder.Where($"it.{fullKey} != @0", "");
            } else {
                if (Property.HasType(entityType, typeof(string), key)) {
                    Builder = Builder.Where($"it.{fullKey}.Contains(@0)", value);
                } else if (Property.HasType(entityType, typeof(bool), key) || Property.HasType(entityType, typeof(bool?), key)) {
                    Builder = Builder.Where($"it.{fullKey} = @0", TypeConverter.StringToBoolean(value));
                } else if (Property.HasType(entityType, typeof(byte), key) || Property.HasType(entityType, typeof(byte?), key)) {
                    var values = value.Split(',').Select(x => TypeConverter.StringToByte(x));
                    Builder = values.Count() > 1 ? Builder.Where($"@0.Contains(it.{fullKey})", values) : Builder.Where($"it.{fullKey} = @0", TypeConverter.StringToByte(value));
                } else if (Property.HasType(entityType, typeof(short), key) || Property.HasType(entityType, typeof(short?), key)) {
                    var values = value.Split(',').Select(x => TypeConverter.StringToShort(x));
                    Builder = values.Count() > 1 ? Builder.Where($"@0.Contains(it.{fullKey})", values) : Builder.Where($"it.{fullKey} = @0", TypeConverter.StringToShort(value));
                } else if (Property.HasType(entityType, typeof(int), key) || Property.HasType(entityType, typeof(int?), key)) {
                    var values = value.Split(',').Select(x => TypeConverter.StringToInteger(x));
                    Builder = values.Count() > 1 ? Builder.Where($"@0.Contains(it.{fullKey})", values) : Builder.Where($"it.{fullKey} = @0", TypeConverter.StringToInteger(value));
                } else if (Property.HasType(entityType, typeof(long), key) || Property.HasType(entityType, typeof(long?), key)) {
                    var values = value.Split(',').Select(x => TypeConverter.StringToLong(x));
                    Builder = values.Count() > 1 ? Builder.Where($"@0.Contains(it.{fullKey})", values) : Builder.Where($"it.{fullKey} = @0", TypeConverter.StringToLong(value));
                } else if (Property.HasType(entityType, typeof(decimal), key) || Property.HasType(entityType, typeof(decimal?), key)) {
                    var values = value.Split(',').Select(x => TypeConverter.StringToDecimal(x));
                    Builder = values.Count() > 1 ? Builder.Where($"@0.Contains(it.{fullKey})", values) : Builder.Where($"it.{fullKey} = @0", TypeConverter.StringToDecimal(value));
                } else if (Property.HasType(entityType, typeof(float), key) || Property.HasType(entityType, typeof(float?), key)) {
                    var values = value.Split(',').Select(x => TypeConverter.StringToFloat(x));
                    Builder = values.Count() > 1 ? Builder.Where($"@0.Contains(it.{fullKey})", values) : Builder.Where($"it.{fullKey} = @0", TypeConverter.StringToFloat(value));
                } else if (Property.HasType(entityType, typeof(double), key) || Property.HasType(entityType, typeof(double?), key)) {
                    var values = value.Split(',').Select(x => TypeConverter.StringToDouble(x));
                    Builder = values.Count() > 1 ? Builder.Where($"@0.Contains(it.{fullKey})", values) : Builder.Where($"it.{fullKey} = @0", TypeConverter.StringToDouble(value));
                }
            }
        }

        private void AddOrderBy(Type entityType, string orderBy, string orderDirection, string relation = "") {
            string error;

            string fullOrderBy = string.IsNullOrEmpty(relation) ? orderBy : $"{relation}.{orderBy}";

            if (!Property.Has(entityType, orderBy)) {
                error = $"Invalid order by query parameter: {fullOrderBy}.";
                _errors.Add(error);
                return;
            }

            if (!(new[] { "asc", "desc" }).Contains(orderDirection.ToLower())) {
                error = $"Invalid order direction query parameter: {orderDirection}.";
                _errors.Add(error);
                return;
            }

            Builder = Builder.OrderBy($"it.{fullOrderBy} {orderDirection}");
        }
    }
}
