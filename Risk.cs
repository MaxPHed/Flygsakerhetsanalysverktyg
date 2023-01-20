using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flygsäkerhetsanalysverktyg
{
    public class Risk : ITable
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int Probability { get; set; }
        public int Consequence { get; set; }
        public int ProbabilityMitigated { get; set; } = 0;
        public int ConsequenceMitigated { get; set; } = 0;
        public Risk(int Id, string Type, int Probability, int Consequence) 
        {
            this.Id = Id;
            this.Type= Type;
            this.Probability = Probability;
            this.Consequence = Consequence;
        }

        public Risk clone()
        {
            Risk risk = new Risk(Id, Type, Probability, Consequence);
            return risk;
        }

        public string makeQuery()
        {
            string query = $"call addRisk('100','{Type}', '{ Probability}', '{Consequence}');";

            return query;
        }

        public string makeUpdateQuery()
        {
            string query = $"call updateRisk({Id},'{Type}', {Probability}, {Consequence});";
            
            return query;
        }

        public string makeDeleteQuery()
        {
            string query = $"call deleteRisk({Id});";

            return query;
        }

        public string getConsequenceChangeString()
        {
            string consequenceString = "";
            if(Consequence.Equals( ConsequenceMitigated))
            {
                consequenceString = $"Konsekvensen oförändrad {Consequence}";
            }
            if (ConsequenceMitigated < Consequence)
            {
                consequenceString = $"Konsekvensen sänkt till {ConsequenceMitigated}";
            }
            return consequenceString;
        }
        public string getProbabilityChangeString()
        {
            string probabilityString = "";
            if (Probability.Equals( ProbabilityMitigated))
            {
                probabilityString = $"Sannolikheten oförändrad {Probability}";
            }
            if (ProbabilityMitigated < Probability)
            {
                probabilityString = $"Sannolikheten sänkt till {ProbabilityMitigated}";
            }
            return probabilityString;
        }
    }
}
