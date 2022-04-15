using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Forms;

namespace ITWebService.Models
{
    public class PersonOnDutyInfoModel
    {
        public DateTime SelectTime { get; set; }
        public string [] Location { get; set; }
        public List<string> Infos { get; set; }
        public string Message { get; set; }
    }
}

