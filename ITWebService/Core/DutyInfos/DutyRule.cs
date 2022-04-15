using System;
using System.Collections.Generic;
using System.Text.Json;
namespace ITWebService.Core
{
    public abstract class FurHander
    {


    }
    public class DutyHander : FurHander
    {
        private string RuleFilePath { get; set; }
        public DutyHander(string path)
        {
            this.RuleFilePath = path;
        }
        public DutyHander()
        {
            this.Execute();
        }
        public void Execute()
        {
            DutyRule duty = new DutyRule();
            duty.LocationKeyDit = new Dictionary<int, string>();
            duty.LocationInOnly = true;
            duty.Cycle = 4;
            duty.MaskItem = new string[] { "白（初）" };
            duty.tup = new(1, "白", "休息");
            duty.LocationKeyDit.Add(2, "15F");
            duty.LocationKeyDit.Add(1, "40F");
            duty.Dispute = new Dictionary<string, Dictionary<int, string>>();
            duty.Dispute.Add("白", duty.LocationKeyDit);
            duty.SortRules=new Dictionary<string, List<string>>();
            List<string> locationlist=new List<string>();
            locationlist.Add("15F");
            locationlist.Add("40F");
            duty.SortRules.Add("Location",locationlist);
            string var = JsonSerializer.Serialize<DutyRule>(duty);
            Console.WriteLine(var);
            var ob = JsonSerializer.Deserialize<DutyRule>(var);
        }
    }
    public class DutyRule
    {
        public bool LocationInOnly { get; set; }
        public int Cycle { get; set; }
        public Dictionary<int, string> LocationKeyDit { get; set; }
        public Dictionary<string, Dictionary<int, string>> Dispute { get; set; }
        public string[] MaskItem { get; set; }
        public Dictionary<string,List<string>> SortRules{get;set;}
        public Tuple<int,string,string> tup { get; set;}
    }
    public class DisputeHander
    {
        public Dictionary<string, Dictionary<int, string>> Dispute { get; set; }
    }
}

