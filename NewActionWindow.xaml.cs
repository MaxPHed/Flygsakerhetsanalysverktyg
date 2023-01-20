using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Flygsäkerhetsanalysverktyg
{
    /// <summary>
    /// Interaction logic for NewActionWindow.xaml
    /// </summary>
    public partial class NewActionWindow : Window
    {
        public List<Risk> RiskList { get; set; }
        public List<Action> ActionList { get; set; }
        public ObservableCollection<Risk> choosenRisks { get; set; }
        public ObservableCollection<Action> choosenActions { get; set; } = new ObservableCollection<Action>();
        public NewActionWindow(List<Risk> RiskList, List<Action> ActionList, ObservableCollection<Risk> choosenRisks)
        {
            InitializeComponent();
            this.RiskList = RiskList;
            this.ActionList = ActionList;
            this.choosenRisks = choosenRisks;
            bindRiskToCB();
        }

        private void bindRiskToCB()
        {
            //Sätter resursen för Comboboxen beroende på checkboxen
            if (!(bool)showAllCheckBox.IsChecked)
            {
                riskCB.ItemsSource = RiskList;
                riskCB.Text = "Välj risk";
            }
            else
            {
                riskCB.ItemsSource = choosenRisks;
                if (choosenRisks.Count <= 0)
                {
                    riskCB.Text = "Inga risker";
                }
            }
        }

        private void bindActionToLB()
        {
            //VIsar action i listbox vbereiende på vilken risk som är vald
            choosenActions.Clear();
            actionLB.Items.Clear();
            try
            {
                Risk risk = (Risk)riskCB.SelectedItem;
                foreach (Action ac in ActionList)
                {
                    if (ac.FromRiskID == risk.Id)
                    {

                        actionLB.Items.Add(ac);
                        choosenActions.Add(ac);
                    }
                }
            }
            catch (Exception e) { MessageBox.Show(e.Message); }
        }

        private void showAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            bindRiskToCB();
        }

        private void riskCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bindActionToLB();
        }

        private void changeActionButton_Click(object sender, RoutedEventArgs e)
        {
            addOrEditAction(false);
        }

        private void newActionButton_Click(object sender, RoutedEventArgs e)
        {
            addOrEditAction(true);
        }

        private void addOrEditAction(bool newAction)
        {
            //Lägger till eller editerar risk beorende på vilken knapp som trycktes på
            int fromRiskId = 0;
            try
            {
                Risk ri = (Risk)riskCB.SelectedItem;
                fromRiskId = ri.Id;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Välj en risk att lägga till åtgärd till");
                return;
            }
            string actionContent = addActionTextBox.Text;
            bool isPreventive = (bool)isPreventiveCheckBox.IsChecked;
            int fromActionId = 0;

            //Validerar input
            if (!InputCheck.checkIfLengthIsOverInt(actionContent, 5)) return;
            if (!InputCheck.checkRiskIdIsNotZero(fromRiskId)) return;
            if (!InputCheck.checkIfLengthIsLessThanInt(actionContent, 150)) return;


            try
            {
                //Lägger till eller uppdaterar i databasen beoende på bool newAction
                FlightSafetyDB flightSafetyDB = new FlightSafetyDB();
                if (newAction)
                {
                    if (!InputCheck.checkIfTextIsRemotelyUnique(actionContent, choosenActions)) return;

                    Action action = new Action(fromActionId, actionContent, isPreventive, fromRiskId);
                    ActionList.Add(action);
                    fromActionId = flightSafetyDB.storeItemInDbGetId(action.makeQuery());
                    action.ActionID = fromActionId;
                    MessageBox.Show(action.ActionContent + " tillagd!");
                    bindActionToLB();
                }
                else
                {

                    Action action = (Action)actionLB.SelectedItem;
                    flightSafetyDB.updateItemInDB(action.makeUpdateQuery());
                    action.ActionContent = actionContent;
                    action.IsPreventive = (bool)isPreventiveCheckBox.IsChecked;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void removeActionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //tar bort action ur DB
                Action action = (Action)actionLB.SelectedItem;
                FlightSafetyDB flightSafetyDB = new FlightSafetyDB();
                flightSafetyDB.updateItemInDB(action.makeDeleteQuery());
                MessageBox.Show(action.ActionContent + " borttagen!");
                ActionList.Remove(action);

                bindActionToLB();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            //HÄmtar en lista med alla actions med sökvärdet från DB. VIsar även vilken risk de är kopplade med.
            //Hade varit lättare att bara söka i listan som redan är sparad i applikationen, men ville ha med sökning i databasen
            string text = searchInput.Text;
            FlightSafetyDB flightSafetyDB = new FlightSafetyDB();
            string query = $"call searchInActions('{text}');";
            searchResultDataGrid.ItemsSource = flightSafetyDB.fillDataTable(query);
        }


    }
}





