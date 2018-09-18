using System.Collections.Generic;

namespace Framework.Domain.Repositories
{
    public class SortingSettings
    {
        public SortingSettings()
        {
            SortingRules = new List<SortingRule>();
        }

        public SortingSettings(SortingRule sortingRule) : this()
        {
            SortingRules.Add(sortingRule);
        }
        
        public SortingSettings(SortingRule[] sortingRules) : this()
        {
            SortingRules.AddRange(sortingRules);
        }
        
        public List<SortingRule> SortingRules { get; set; }

        public void AddRule(SortingRule sortingRule)
        {
            SortingRules.Add(sortingRule);
        }
    }
}