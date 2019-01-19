using System.Collections.Generic;

namespace PortfolioTool.Models
{
    class Portfolio
    {
        public string Name { get; set; }
        public IDictionary<string, double> Holdings { get; set; }
        public IDictionary<string, double> Allocations { get; set; }
        public double Cash { get; set; }
    }
}