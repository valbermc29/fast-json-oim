using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fast_json_oim.DTOs
{
    public class Generation
    {
        public string Description { get; set; } = "";
        public List<string> Services { get; set; } = new();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> PolicyNumbers { get; set; } = new();
        public List<string> ClaimNumbers { get; set; } = new();
        public List<string> TreatyCodes { get; set; } = new();
        public List<string> BorderauxNumbers { get; set; } = new();
        public List<string> EndorsementNumbers { get; set; } = new();
        public List<string> PolicyNumbersNotExported { get; set; } = new();
        public string OutputDirectory { get; set; } = "";
        public bool PorItem { get; set; }
        public int LotSize { get; set; }
    }
}
