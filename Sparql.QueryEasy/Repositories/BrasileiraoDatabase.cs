using System.Resources;
using VDS.RDF;
using VDS.RDF.Parsing;
using Graph = VDS.RDF.Graph;

namespace Sparql.QueryEasy.Repositories
{
    public class BrasileiraoDatabase
    {
        private readonly IGraph _graph;
        public BrasileiraoDatabase()
        {
            ResourceManager resourceManager = new ResourceManager("Sparql.QueryEasy.SparqlResources", typeof(Program).Assembly);
            string ttlString = resourceManager.GetString("BrasileiraoDatabase");
            _graph = new Graph();
            StringParser.Parse(_graph, ttlString, new TurtleParser());
        }

        public IGraph Database
        {
            get
            {
                return _graph;
            }
        }
    }
}
