using System.Net;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Writing;

SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("https://query.wikidata.org/sparql"), "https://query.wikidata.org/sparql");
SparqlConnector sparql = new SparqlConnector(endpoint);









//var consulta = sparql.Query(query);

SparqlWikidata();

var a = 10;

void SparqlWikidata()
{

    try
    {
        //Options.HttpDebugging = true;

        String query = @"PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
PREFIX owl: <http://www.w3.org/2002/07/owl#> 
PREFIX bd: <http://www.bigdata.com/rdf#> 
PREFIX wikibase: <http://wikiba.se/ontology#> 

SELECT DISTINCT ?property ?propertyLabel ?kind ?uri ?uriLabel
WHERE {
  <http://www.wikidata.org/entity/Q529207> ?property [] .
  ?p wikibase:directClaim ?property .
  OPTIONAL { ?p rdfs:label ?propertyLabel .
            FILTER (lang(?propertyLabel) = ""en"")
           } BIND( IF(EXISTS { ?property rdf:type owl:ObjectProperty}, 1, IF(EXISTS {?property rdf:type owl:DatatypeProperty}, 2, 0)) as ?kind) .

 
   OPTIONAL { <http://www.wikidata.org/entity/Q529207> ?property ?uri .
             ?uri rdfs:label ?uriLabel . FILTER (lang(?uriLabel) = ""en"")}
}"; ;

        SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("https://query.wikidata.org/sparql"), "https://www.wikidata.org")
        {
            UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36"
        };
        SparqlResultSet results = endpoint.QueryWithResultSet(query);


        foreach (SparqlResult result in results)
        {
            Console.WriteLine(results.Variables.FirstOrDefault());
            Console.WriteLine(result.ToString());
            Console.WriteLine("--");
            INode n;
            String data;
            if (result.TryGetValue("s", out n))
            {
                switch (n.NodeType)
                {
                    case NodeType.Uri:
                        data = ((IUriNode)n).Uri.AbsoluteUri;
                        break;
                    case NodeType.Blank:
                        data = "blank";
                        break;
                    case NodeType.Literal:
                        //You may want to inspect the DataType and Language properties and generate
                        //a different string here
                        data = ((ILiteralNode)n).Value;
                        break;
                    default:
                        throw new RdfOutputException("Unexpected Node Type");
                }
            }
            else
            {
                data = String.Empty;
            }

            Console.WriteLine($"data -> {data}");
            //Do what you want with the extracted string
        }

        using (HttpWebResponse response = endpoint.QueryRaw(query))
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                while (!reader.EndOfStream)
                {
                    Console.WriteLine(reader.ReadLine());
                }
                reader.Close();
            }
            response.Close();
        }
    }
    finally
    {
        //Options.HttpDebugging = false;
    }
}
void SparqlDBPedia()
{
    try
    {
        //Options.HttpDebugging = true;

        String query = "PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#> SELECT * WHERE {?s a rdfs:Class } LIMIT 50";

        SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");
        SparqlResultSet results = endpoint.QueryWithResultSet(query);
        //TestTools.ShowResults(results);

        using (HttpWebResponse response = endpoint.QueryRaw(query))
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                while (!reader.EndOfStream)
                {
                    Console.WriteLine(reader.ReadLine());
                }
                reader.Close();
            }
            response.Close();
        }

    }
    finally
    {
        //Options.HttpDebugging = false;
    }
}
