using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flygsäkerhetsanalysverktyg
{
    internal interface ITable
    {
        //Ger en query för varje objekt som använder interfacet. 

        //Ger tillbaka en string som innehåller information om det specifika objektet och returnerar en int
         string makeQuery();
        //Tar en int och uppdaterar i databasen posten med samma id som int
         string makeUpdateQuery();
        //Gör en deletew wuery för det objekt som kallar den .
         string makeDeleteQuery();
    }
}
