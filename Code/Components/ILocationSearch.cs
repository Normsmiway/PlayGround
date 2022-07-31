using System.Collections.Generic;

namespace Geo.Api.Code.Components
{
    public interface ILocationSearch
    {
        ///Given the user’s latitude and longitude, fetch all places that are within a radius of say 10KM from the user.
        public  IAsyncEnumerable<IRegion> FindNearest(double latitide, double longitude, double radius);
    }

    public interface ISortingAlgorithm
    {
        /*
         * distance of the user from a agent location
         * type of the services offered — “withdrwal”, “deposit”, etc.
         * average rating of the place
         * number of ratings of the place
         * has user visited this place earlier
         * fraction of times user has been to this 'agent' earlier.
         * … and so on.
         */
        public IEnumerable<IRegion> GetSortedRegions(IEnumerable<IRegion> regions, SortOrder order = SortOrder.Descending);
        public enum SortOrder { Ascending, Descending };
    }
    public interface IScoring
    {
        /*
         * The model computes a score using the above features:
         * score(user, agent) = w1*distance + w2*services + w3*rating + …
         */

        public void Score<T>(Func<T, bool>? criteria = default);
    }
    public interface IRegion
    {
        public Point Point { get; init; }
        public double Distance { get; init; }
    }

    public interface ILocationService
    {
        /*
         * When a new place is added, 
         * it is inserted into a local Postgres 
         * database table “places” with the following fields:
         */
        public Task InitializeAsync(string id, string name, string area, string city, double latitude, double longitude);
    }

    public interface IAgentRating
    {
        public string AgentId { get; set; }
        public double AverageRating { get; set; }
        public int TotalRating { get; set; }
    }

    public interface IProcessingWorkflow
    {
        /*
         * Whenever a new place is added,
         * make a POST/PUT request to the API server. 
         * The API server does the following
         */
        public IProcessingWorkflow GenerateAgentId();
        public IProcessingWorkflow AddAgentLocation();

        public IProcessingWorkflow AddAgentRating();
        public IProcessingWorkflow IndexAgentLocationForSearch();

        public bool Build();


        private bool Usage()
        {
            /*
            * 1. Generates a place_id.
            * 2. Create a SQL query and insert the record into “places” table
            * 3. Create a SQL query for average_rating and num_ratings and insert it into “ratings” table
            * 4. Insert the place_id along with latitude and longitude into the QuadTree service.
            * 5. Return success/failure message to client.
            */
            IProcessingWorkflow builder = null;
            return builder?.GenerateAgentId()
                    .AddAgentLocation()
                    .AddAgentRating()
                    .IndexAgentLocationForSearch()
                    .Build() ?? false;
        }
    }
}
