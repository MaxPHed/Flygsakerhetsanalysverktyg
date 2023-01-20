using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Flygsäkerhetsanalysverktyg
{
    public class Action : ITable
    {
        public int ActionID { get; set; }
        public string ActionContent { get; set; }
        public bool IsPreventive { get; set; }
        public int FromRiskID { get; set; }
        public bool IsChecked { get; set; }


        public Action(int ActionID, string ActionContent, int IsPreventive, int FromRiskID)
        {
            //workaround för att omvandla int i databasen till bool
            this.ActionID = ActionID;
            this.ActionContent = ActionContent;
            if (IsPreventive == 1) { this.IsPreventive = true; }
            else { this.IsPreventive = false; }
            this.FromRiskID = FromRiskID;
            IsChecked = false;
        }
        public Action(int ActionID, string ActionContent, bool IsPreventive, int FromRiskID)
        {
            this.ActionID = ActionID;
            this.ActionContent = ActionContent;
            this.IsPreventive= IsPreventive;
            this.FromRiskID = FromRiskID;
            IsChecked = false;
        }
        

        public Action clone()
        {
            //Klonar action och gör ett nytt identiskt objekt
            Action action = new Action(ActionID, ActionContent, IsPreventive, FromRiskID);
            return action;
        }

        //Kommentarer finns i ITable
        public string makeQuery()
        {
           
            string query = "call addAction('" + ActionContent + "', " + IsPreventive.ToString() + ", " + FromRiskID.ToString() + ", "+100+ ");";

            return query;
        }
        public string makeUpdateQuery()
        {
            string query = $"call updateAction({ActionID},'{ActionContent}', {IsPreventive});";

            return query;
        }

        public string makeDeleteQuery()
        {
            string query = $"call deleteAction({ActionID});";

            return query;
        }
    }
}
