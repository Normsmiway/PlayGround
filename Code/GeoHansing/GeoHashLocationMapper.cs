using Geohash;
using Geo.Api.Code.Components;

namespace Geo.Api.Code.GeoHansing
{
    public class GeoHashLocationMapper : ILocationSearch
    {
        private readonly int _precision/*can mmake this value configurable*/;
        private readonly Geohasher _hasher;
        private readonly IGeoIndexProvidder _geoIndexProvidder;
        public GeoHashLocationMapper(IGeoIndexProvidder geoIndexProvidder, Geohasher? hasher = null, int precison = 6)
        {
            _hasher = hasher ?? new();
            _precision = precison;
            _geoIndexProvidder = geoIndexProvidder;
        }
        public async IAsyncEnumerable<IRegion> FindNearest(double latitide, double longitude, double radius)
        {
            //get coordinate hash
            string geohash = _hasher.Encode(latitide, longitude, _precision);

            var neighboringGeoHashes = _hasher.GetNeighbors(geohash).Select(c => c.Value);

            //get maching geo hashes from geoIndex
            var maches = _geoIndexProvidder.GetGeoIndeces(neighboringGeoHashes);

            foreach (var geoHash in maches)
            {
                var (lat, log) = _hasher.Decode(geohash);
                yield return await Task.FromResult(CoordinateMap.ReturnIfWithinRegion(new(latitide, longitude), new(lat, log), radius));
            }
        }
    }

    public interface IGeoIndexProvidder
    {
        public IEnumerable<IGeoIndexData<string>> GetGeoIndeces(IEnumerable<string> neighboringGeoHashes);
    }

    internal sealed class DefaultGeoIndexDataProvider : IGeoIndexProvidder
    {
        private readonly IGeoStore<GeoIndex> _geoStore;

        public DefaultGeoIndexDataProvider(IGeoStore<GeoIndex> geoStore)
        {
            _geoStore = geoStore;
        }

        public IEnumerable<IGeoIndexData<string>> GetGeoIndeces(IEnumerable<string> neighboringGeoHashes)
        {
            return _geoStore.Find(neighboringGeoHashes.ToList());
        }
    }

    public interface IGeoIndexData<T>
    {
        public T? GeoHash { get; set; }
        public string? Id { get; set; }

    }

    public interface IGeoStore<T>
    {
        public void Save(string key, T data);
        public IEnumerable<T> Find(List<string> geoHashNeighbours);

    }
    public interface IGeoIndexStore : IGeoStore<GeoIndex> { }
    internal sealed class InMemoryGeoStore : IGeoIndexStore
    {
        private readonly Dictionary<string, GeoIndex> _store;
        public InMemoryGeoStore()
        {
            _store = new Dictionary<string, GeoIndex>();
        }
        public IEnumerable<GeoIndex>? Find(List<string> geoHashNeighbours)
        {
            List<GeoIndex> geoIndexes = new();
            foreach (var geoHash in geoHashNeighbours)
            {
                var geoIndex = _store.Where(v => (v.Value is not null &&
                                                  v.Value.GeoHash is not null &&
                                                  v.Value.GeoHash.StartsWith(geoHash)))
                                                 .Select(c => c.Value);
                geoIndexes.AddRange(geoIndex);
            }
            return geoIndexes ?? Enumerable.Empty<GeoIndex>();
        }


        public void Save(string key, GeoIndex data) => _store.Set(key, new GeoIndex() { Id = key, GeoHash = data.GeoHash });

    }

    internal sealed class SqlDBGeoStore : IGeoIndexStore
    {
        public IEnumerable<GeoIndex> Find(List<string> geoHashNeighbours)
        {
            string sql = $"SELECT Id,GeoHash  AS {nameof(GeoIndex.GeoHash)}  " +
                $"FROM LocationGeoIndex WHERE {nameof(GeoIndex.GeoHash)} LIKE '{geoHashNeighbours.First()}%'" +
                BuildQuerySuffix(geoHashNeighbours, nameof(GeoIndex.GeoHash));

            return Enumerable.Empty<GeoIndex>();


            string BuildQuerySuffix(List<string> geoHashes, string field)
            {
                var infiexes = geoHashes.Skip(1);
                return string.Join($" OR {field} LIKE ", infiexes.Select(c => $"{c}%"));
            }
        }

        public void Save(string key, GeoIndex data)
        {
            throw new NotImplementedException();
        }
    }
    public class GeoIndex : IGeoIndexData<string>
    {
        public string? Id { get; set; }
        public string? GeoHash { get; set; }
    }


    public static class Extensions
    {
        public static IServiceCollection AddLocationServices(this IServiceCollection services)
        {
            //  services.AddScoped<IGeoIndexStore, SqlDBGeoStore>();
            services.AddScoped<IGeoIndexStore, InMemoryGeoStore>();
            services.AddTransient<ILocationSearch, GeoHashLocationMapper>();
            services.AddScoped<IGeoIndexProvidder, DefaultGeoIndexDataProvider>();

            return services;
        }
    }
}
