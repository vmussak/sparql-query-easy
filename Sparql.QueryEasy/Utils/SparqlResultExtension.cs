using static System.Runtime.InteropServices.JavaScript.JSType;
using VDS.RDF.Writing;
using VDS.RDF;
using System.Runtime.CompilerServices;
using VDS.RDF.Query;

namespace Sparql.QueryEasy.Utils
{
    public static class SparqlResultExtension
    {
        public static string GetStringValue(this ISparqlResult? result, string variableName, bool removeSignals = false)
        {
            INode n;
            string resultData = string.Empty;

            if (!result.TryGetValue(variableName, out n)) return resultData;

            if(n == null) return resultData;

            resultData = n.NodeType switch
            {
                NodeType.Uri => removeSignals ? ((IUriNode)n).Uri.AbsoluteUri : $"<{((IUriNode)n).Uri.AbsoluteUri}>",
                NodeType.Blank => "blank",
                NodeType.Literal => ((ILiteralNode)n).Value,
                _ => throw new RdfOutputException("Unexpected Node Type"),
            };

            return resultData;
        }
    }
}
