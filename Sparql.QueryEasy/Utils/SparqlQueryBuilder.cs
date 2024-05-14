using System.Text;

namespace Sparql.QueryEasy.Utils
{
    public class SparqlQueryBuilder
    {
        private readonly StringBuilder _query;
        private readonly bool _isWikidata;

        public SparqlQueryBuilder(bool isWikidata)
        {
            _isWikidata = isWikidata;
            _query = new StringBuilder();
        }

        public SparqlQueryBuilder AddDefaultPrefixes()
        {
            _query.AppendLine("PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>");
            _query.AppendLine("PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>");
            _query.AppendLine("PREFIX owl: <http://www.w3.org/2002/07/owl#>");

            if(_isWikidata)
            {
                _query.AppendLine("PREFIX wikibase: <http://wikiba.se/ontology#>");
            }

            return this;
        }

        public SparqlQueryBuilder Select(string fields)
        {
            _query.AppendLine($"SELECT DISTINCT {fields}");

            return this;
        }

        public SparqlQueryBuilder StartWhere()
        {
            _query.AppendLine("WHERE {");
            return this;
        }

        public SparqlQueryBuilder EndWhere()
        {
            _query.AppendLine("}");
            return this;
        }

        public SparqlQueryBuilder StartOptional()
        {
            _query.AppendLine("OPTIONAL {");
            return this;
        }

        public SparqlQueryBuilder EndOptional()
        {
            _query.AppendLine("}");
            return this;
        }

        public SparqlQueryBuilder Where(string subject, string predicate, string @object)
        {
            //<http://www.wikidata.org/entity/Q529207> ?property [] .

            var formatedObject = @object.StartsWith("<") || @object.StartsWith("[") || @object.StartsWith("?") ? @object : $"\"{@object}\"";
            var formatedSubject = subject.StartsWith("<") || subject.StartsWith("[") || subject.StartsWith("?") ? subject : $"\"{subject}\"";

            _query.AppendLine($"{formatedSubject} {predicate} {formatedObject} .");
            return this;
        }

        public SparqlQueryBuilder Filter(FilterType filterType, string variable, string value)
        {
            var filter = filterType switch
            {
                FilterType.Starts => "STRSTARTS",
                _ => "CONTAINS",
            };

            _query.AppendLine($"FILTER  ({filter}({variable}, \"{value}\")) .");
            return this;
        }
        public SparqlQueryBuilder Limit(int limit)
        {
            //<http://www.wikidata.org/entity/Q529207> ?property [] .
            _query.AppendLine($"LIMIT {limit}");
            return this;
        }

        public SparqlQueryBuilder GetVariableLabel(string variableName, bool ignoreWikidata = false)
        {
            _query.AppendLine("OPTIONAL {");
            if (_isWikidata && !ignoreWikidata)
            {
                _query.AppendLine($"{variableName}Claim wikibase:directClaim {variableName} .");
                _query.AppendLine($"{variableName}Claim rdfs:label {variableName}Label .");
            }
            else
            {
                _query.AppendLine($"{variableName} rdfs:label {variableName}Label .");
            }
            _query.AppendLine($"FILTER (lang({variableName}Label) = \"en\")");
            _query.AppendLine("}");
            return this;
        }

        public SparqlQueryBuilder AddVariableType(string variableName)
        {
            _query.AppendLine($"BIND(IF(EXISTS {{ {variableName} rdf:type owl:ObjectProperty}}, \"objeto\", IF(EXISTS {{{variableName} rdf:type owl:DatatypeProperty}}, \"label\", \"outro\")) as {variableName}Type)");

            return this;
        }

        public string Build()
        {
            return _query.ToString();
        }
    }

    public enum FilterType
    {
        Starts
    }
}
