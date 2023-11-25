using System.Collections.Generic;

namespace ConanExilesHelper.SourceQuery.Rules;

public class DictionaryOnlyRuleParser : IRuleParser<Dictionary<string, string>>
{
    public Dictionary<string, string> FromDictionary(Dictionary<string, string> rawRules) => rawRules;
}
