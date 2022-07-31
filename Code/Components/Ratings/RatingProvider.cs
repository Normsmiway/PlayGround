using static Geo.Api.Code.Components.RatingConatiner;

namespace Geo.Api.Code.Components
{
    public static class RatingProvider
    {
        static RatingConatiner _container = new();
        public static Rating GetRating(string agentId) => _container.GetAgentRating(agentId) ?? new(0, 0);
        public static string GetRatingSummary(string agentId, string magicString)
        {

            double totalRating = 0;
            int numberofRatings = 0;
            foreach (var ratingstr in magicString)
            {
                double rating = int.Parse(ratingstr.ToString());
                _container.AddOrUpdateAgentRating(agentId, rating);
                totalRating += rating;
                numberofRatings += 1;
            }
            var cr = _container.GetAgentRating(agentId);
            return $"Total: {totalRating} {Environment.NewLine}" +
                   $"Number of Ratings: {numberofRatings} {Environment.NewLine}" +
                   $"Average rating {Math.Round(Convert.ToDecimal(totalRating / numberofRatings), 4)} {Environment.NewLine}" +
                   $"___________________________________________________________{Environment.NewLine}" +
                   $"Stored Average rating: {Math.Round(Convert.ToDecimal(cr?.AverageRating), 4)} {Environment.NewLine}" +
                   $"Stored Number of rating:{cr?.NumberOfRating} {Environment.NewLine}" +
                   $"Compressed Rating string: {Helper.Compress(magicString)}";
        }
    }
    public class RatingConatiner
    {
        private readonly Dictionary<string, Rating> _ratingStore = new();
        public class Rating
        {
            public double AverageRating { get; private set; }
            public int NumberOfRating { get; private set; }
            public Rating(double averagerating, int numberOfRating)
            {
                AverageRating = averagerating;
                NumberOfRating = numberOfRating;
            }
        }
        public void AddOrUpdateAgentRating(string agentId, double newRating)
        {
            TryGetRating(agentId, out Rating? existingRating);
            double avg = existingRating?.AverageRating ?? 0; ;
            int num = existingRating?.NumberOfRating ?? 0;
            (int numOfRatings, double rating) = ComputeNewRating(newRating, avg, num);
            TryAddRating(agentId, new(rating, numOfRatings));

        }
        // Most likely to be extracted to a seperate service
        private static (int numberOfRatings, double averageRating) ComputeNewRating(double newRating, double averageRating, int numberOfRatings)
        {
            double avgRating = averageRating;
            int numOfRatings = numberOfRatings;
            double rating = (numOfRatings * avgRating + newRating) / (numOfRatings + 1);
            numOfRatings += 1;
            return (numOfRatings, rating);
        }
        public Rating? GetAgentRating(string agentId)
        {
            return _ratingStore.TryGetValue(agentId, out Rating? rating) ? rating : null;
        }
        private bool TryGetRating(string agentId, out Rating? rating)
        {
            return _ratingStore.TryGetValue(agentId, out rating);
        }

        private bool TryAddRating(string agentId, Rating rating)
        {
            if (_ratingStore.TryAdd(agentId, rating)) //key does not exist, so add it
                return true;
            _ratingStore[agentId] = rating;
            return true;
        }
    }

    public static class Helper
    {
        public static string Compress(string s)
        {
            string arr = "FEDCBA";
            Console.WriteLine($"--------------Compress {s}-------------------");

            Dictionary<string, int> store = new();
            string output = string.Empty;
            int counter = 0;
            while (counter < s.Length)
            {
                if (!store.ContainsKey(s[counter].ToString()))
                    store.Add(s[counter].ToString(), 1);
                else
                    store[s[counter].ToString()]++;
                counter++;
            }
            output = string.Join("", store.Keys.Select(key => $"{store[key]}{key}"));
            return output;
        }

        public static T? Get<T>(this Dictionary<string, T> store, string key)
        {
            return store.TryGetValue(key, out T? data) ? data : default;
        }
        public static void Set<T>(this Dictionary<string, T> store, string key, T data)
        {
           store.TryAddData(key, data);

        }

        private static bool TryAddData<T>(this Dictionary<string, T> store, string key, T data)
        {
            if (store.TryAdd(key, data)) //key does not exist, so add it
                return true;
            store[key] = data;
            return true;
        }
    }
}
