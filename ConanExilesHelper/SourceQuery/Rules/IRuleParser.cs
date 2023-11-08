using System.Collections.Generic;

namespace ConanExilesHelper.SourceQuery.Rules
{
    public interface IRuleParser<T>
    {
        T FromDictionary(Dictionary<string, string> rawRules);
    }
}
