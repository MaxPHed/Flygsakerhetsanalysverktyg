using Org.BouncyCastle.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Flygsäkerhetsanalysverktyg
{
    public static class InputCheck
    {
        //Statisk klass för att returnera bools ifall en validering klaras eller inte. Ger ett felmeddelande om det går fel
        public static bool checkIfNumberIsBetween1And10(int number)
        {
            if (number <= 0 || number > 10)
            {
                MessageBox.Show("Ange risk och/eller sannolikhet som en siffra mellan 1-10");
                return false;
            }
            else return true;
        }
        public static bool checkIfLengthIsOverInt(string content, int length)
        {
            if (content.Length >= length) return true;
            else { MessageBox.Show($"Texten behöver innehålla minst {length} tecken"); return false; }
        }

        public static bool checkRiskIdIsNotZero(int fromRiskId)
        {
            if (fromRiskId != 0) return true;
            else { MessageBox.Show("Välj en risk att knyta åtgärden till"); return false; }
        }

        public static bool checkIfTextIsRemotelyUnique(string content, ObservableCollection<Action> list)
        {
            //Metod som ger tillbaka false om 90% av orden i en string är samma
            double highestSimilarity = 0;
            foreach (Action ac in list)
            {
                string[] vs = content.Split(new char[] { ' ', '-', '/', '(', ')', '!', '.' }, StringSplitOptions.RemoveEmptyEntries);
                string[] vs1 = ac.ActionContent.Split(new char[] { ' ', '-', '/', '(', ')', '!', '.' }, StringSplitOptions.RemoveEmptyEntries);
                double equalWords = vs.Intersect(vs1, StringComparer.OrdinalIgnoreCase).Count();
                double length = Convert.ToDouble(vs.Length);
                double similarity = equalWords / length;
                if (similarity > highestSimilarity)
                {
                    highestSimilarity = similarity;
                }
            }
            if (highestSimilarity < 0.9) return true;
            else { MessageBox.Show("En liknande åtgärd finns redan"); return false; }
        }

        public static bool checkIfLengthIsLessThanInt(string content, int max)
        {
            if (content.Count() > max)
            {
                MessageBox.Show($"Texten får innehålla som mest {max} tecken");
                return false;
            }
            else return true;
        }

        //TODO gör checkifriskalreadyexists och checkifoperationalreadyexist generiska
        public static bool checkIfOperationAlreadyExists(string operationName, List<Operation> OperationList)
        {
            bool operationExists = false;
            foreach (Operation op in OperationList)
            {
                if (op.Type.Equals(operationName)) operationExists = true;
            }
            if (operationExists)
            {
                MessageBox.Show("Verksamheten finns redan");
            }
            return operationExists;
        }
        public static bool checkIfRiskAlreadyExists(string riskName, List<Risk> RiskList)
        {
            bool riskExists = false;
            foreach (Risk ri in RiskList)
            {
                if (ri.Type.Equals(riskName)) riskExists = true;
            }
            if (riskExists)
            {
                MessageBox.Show("Risken finns redan");
            }
            return riskExists;
        }
    }
}
