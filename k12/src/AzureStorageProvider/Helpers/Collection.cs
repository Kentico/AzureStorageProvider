using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AzureStorageProvider.Helpers
{
    public class Collection<TModel, TCollection> : Singleton<TCollection>
        where TModel : IObjectWithPath<TModel>, new()
        where TCollection : new()
    {
        protected static ConcurrentDictionary<string, TModel> _items = new ConcurrentDictionary<string, TModel>();

        public Collection()
        {
        }

        public virtual TModel GetOrCreate(string name)
        {
            return _items.GetOrAdd(name, n => new TModel().Initialize(n));
        }

        public TModel TryGet(string name)
        {
            TModel item;
            _items.TryGetValue(name, out item);

            return item;
        }

        public bool Contains(string name)
        {
            return _items.ContainsKey(name);
        }
        
        public IEnumerable<TModel> GetStartingWith(string path, bool flat)
        {
            var condition = new Func<TModel, bool>(i => i.Path.StartsWith(path));
            if (flat)
            {
                path = AzurePathHelper.GetValidPathForwardSlashes(path);
                condition = i => i.Path.StartsWith(path) && AzurePathHelper.GetBlobDirectory(i.Path) == path;
            }
            
            // need to use Values to work on top of moment-in-time snapshot
            return _items.Values.Where(condition);
        }

        public void AddRangeDistinct(IEnumerable<TModel> data)
        {
            foreach (var item in data)
            {
                // in case item already exists, keep the old one
                _items.AddOrUpdate(item.Path, p => item, (p, oldItem) => item);
            }
        }

        public void ForAll(Func<TModel, bool> condition, Action<TModel> function)
        {
            var keys = _items.Keys.ToList();

            foreach (var key in keys)
            {
                var item = _items[key];
                if (condition(item))
                    function(item);
            }
        }
    }
}
