using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flygsäkerhetsanalysverktyg
{
    public class OperationRisk
    {
        public int OpId { get; set; }
        public int RiskId { get; set; }

        public OperationRisk(int OpId, int RiskID) 
        {
            this.OpId = OpId;
            this.RiskId = RiskID;
        }

        public string makeQuery()
        {
            string query = $"call addOpRi({OpId}, {RiskId});";

            return query;
        }

        public string makeDeleteQuery()
        {
            string query = $"call deleteOpRi({OpId}, {RiskId});";

            return query;
        }

        //Denna har ingen updateQuery då databsen gör det automatiskt via Cascade
    }
}
