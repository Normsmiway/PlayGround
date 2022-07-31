using GeoCoordinatePortable;
using Geo.Api.Code.Components;

namespace Geo.Api.Code
{
    public class Region : IRegion
    {
        public Point Point { get; init; }
        public double Distance { get; init; }
        public string RegionId { get; set; }
        public IEnumerable<Point> AgentLocations { get; set; }

        public Region(Point point, double distance)
        {
            Point = point;
            Distance = distance;
        }
    }
    public record Point(double Latitude = 6.5576827, double Longitude = 3.3889053);

    public class CoordinateMap
    {
        private static readonly Dictionary<string, double> pointStore;
        const double MaxLatitude = 6.5623864; //6.6823864
        const double MaxLongitude = 3.353716; //3.593716
        private static readonly IEnumerable<Point> coordinates = Enumerable.Empty<Point>();
        static CoordinateMap()
        {
            var random = new RandomNumbers(500);
            if (!coordinates.Any())
            {
                coordinates = Enumerable.Range(1, 3000000)
                    .Select(index =>new Point(random.NextDouble(6.465422, MaxLatitude),
                    random.NextDouble(3.406448, MaxLongitude))).ToList();
            }
            pointStore = new();
        }
        public static double GetDistance(Point startPoint, Point endpoint)
        {
            GeoCoordinate geoCoordinate = new(startPoint.Latitude, startPoint.Longitude);
            GeoCoordinate pin2 = new(endpoint.Latitude, endpoint.Longitude);

            double distance = geoCoordinate.GetDistanceTo(pin2);

            return distance;
        }
        static double StoreAndForward(Point currentPosition, Point endPoint)
        {
            string key = $"{endPoint.Latitude},{endPoint.Longitude}";
            double distance = GetDistance(currentPosition, endPoint);
            pointStore.TryAdd(key, distance);
            return distance;
        }


        public static IEnumerable<Region> GetCoordinatesWithRadius(Point currentPosition, double radius, int take = 1, int skip = 0)
        {
            if (skip < 0) skip = 0;
            if (take < 1) take = 1;

            var filtered = coordinates
                .Where(point => StoreAndForward(currentPosition, point) <= radius)
                .Select(p => p.ToRegion(pointStore[$"{p.Latitude},{p.Longitude}"]));
            return filtered.Skip(skip - 1).Take(take);

        }

        public static Region? ReturnIfWithinRegion(Point currentPosition, Point endPoint, double radius)
        {
            double distance = StoreAndForward(currentPosition, endPoint);
            if (distance <= radius)
            {
                return endPoint.ToRegion(distance);
            }
            return default;
        }

    }
    public static class Extensions
    {
        public static Point ToPoint(this GeoCoordinate coordinate)
        {
            return new Point(coordinate.Latitude, coordinate.Longitude);
        }
        public static GeoCoordinate ToGeoCoordinate(this Point point)
        {
            return new GeoCoordinate(point.Latitude, point.Longitude);
        }

        public static Region ToRegion(this Point point, double distance)
        {
            return new Region(point, distance);
        }
    }
    public class RandomNumbers : Random
    {
        public RandomNumbers(int seed) : base(seed) { }

        public double NextDouble(double minimum, double maximum)
        {
            return base.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}



