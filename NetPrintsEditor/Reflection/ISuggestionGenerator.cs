using NetPrints.Base;
using System.Collections.Generic;

namespace NetPrintsEditor.Reflection
{
    public interface ISuggestionGenerator
    {
        IEnumerable<(string, object)> GetSuggestions(INodeGraph graph, INodePin pin = null);
    }
}
