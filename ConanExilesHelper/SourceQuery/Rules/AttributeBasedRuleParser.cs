using System.Collections.Generic;
using System.Reflection;

namespace ConanExilesHelper.SourceQuery.Rules;

public class AttributeBasedRuleParser<TRules> : IRuleParser<TRules> where TRules : new()
{
    public TRules FromDictionary(Dictionary<string, string> rawRules)
    {
        var rules = new TRules();

        var props = rules.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in props)
        {
            if (prop.CanWrite)
            {
                var fromKeyAttr = prop.GetCustomAttribute<FromKey>();

                if (fromKeyAttr is not null)
                {
                    var key = fromKeyAttr.KeyName;

                    if (rawRules.TryGetValue(key, out var value))
                    {
                        prop.SetValue(rules, value);
                    }
                }
            }
        }

        return rules;
    }
}
