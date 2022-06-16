using System;
using System.Collections.Generic;
using System.Text.Json;
namespace ITWebService.Core
{
    public class DutyRule
    {
        public bool LocationInOnly { get; set; }
        public int Cycle { get; set; }
        public Dictionary<int, string> LocationKeyDit { get; set; }
        public Dictionary<string, Dictionary<int, string>> Dispute { get; set; }
        public string[] MaskItem { get; set; }
        public Dictionary<string,DutyRuleBody> SortRules{get;set;}
        public Tuple<int,string,string> tup { get; set;}
    }
    public struct DutyRuleBody{

        public string type{get;set;}
        public List<string> ruleVaule{get;set;}
    }
    public class DisputeHander
    {
        public Dictionary<string, Dictionary<int, string>> Dispute { get; set; }
    }
}

