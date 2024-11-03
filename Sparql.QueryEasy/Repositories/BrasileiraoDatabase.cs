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
            string ttlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "futebol_completo.ttl");
            string ttlString = File.ReadAllText(ttlFilePath);
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
