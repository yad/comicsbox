using Comicsbox.Cache;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Comicsbox
{
    public class BookContainer<T> : ICacheConfiguration
    {
        public string Thumbnail { get; private set; }

        public IReadOnlyCollection<T> Collection { get; private set; }

        public bool HasNextPagination { get; private set; }

        public bool CacheResult { get; private set; }

        public BookContainer(string thumbnail, IEnumerable<T> collection)
            : this(thumbnail, false, collection)
        {
        }

        private BookContainer(string thumbnail, bool hasNextPagination, IEnumerable<T> collection)
        {
            Thumbnail = thumbnail;
            HasNextPagination = hasNextPagination;
            Collection = collection.ToArray();
        }

        public BookContainer<T> WithCache(bool enableCache)
        {
            CacheResult = enableCache;
            return this;
        }

        public BookContainer<T> WithPagination(int pagination)
        {
            const int MaxDisplay = 5;
            int total = Collection.Count;
            int iteration = (int)Math.Ceiling(total / (double)MaxDisplay);

            var filter = Collection.Skip((pagination - 1) * MaxDisplay).Take(MaxDisplay);

            bool hasNextPagination = !filter.Last().Equals(Collection.Last());
            return new BookContainer<T>(Thumbnail, hasNextPagination, filter);
        }
    }
}
