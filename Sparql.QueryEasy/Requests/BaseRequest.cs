﻿namespace Sparql.QueryEasy.Queries
{
    public record BaseRequest(string EndpointUrl = "https://query.wikidata.org/sparql", int Limit = 20);
    
}
// https://query.wikidata.org/sparql https://dbpedia.org/sparql/

