using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Flygsäkerhetsanalysverktyg
{
    public class Operation : ITable
    {
        
        public int Id { get; set; }
        public string Type { get; set; }
        

        public Operation(int Id, string Type) 
        {
            this.Id = Id;
            this.Type = Type;
        }

        public Operation clone()
        {
            Operation operation = new Operation(Id, Type);
            return operation;
        }

        public string makeQuery()
        {
            string query = $"call addOperation(100,'{ Type}');";

            return query;
        }
        public string makeDeleteQuery()
        {
            //Ej ännu inlagd
            string query = $"call deleteOperation({Id});";

            return query;
        }
        public string makeUpdateQuery()
        {
            string query = $"call updateOperation({Id},'{Type}');";

            return query;
        }
    }
}
