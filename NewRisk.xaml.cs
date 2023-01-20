using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for NewRisk.xaml
    /// </summary>
    public partial class NewRisk : Window
    {
        public List<Operation> AllOperations { get; set; }
        public ObservableCollection<Operation> CurrentOperations { get; set; }
        public List<OperationRisk> OpRiskList { get; set; }
        public List<Risk> RiskList { get; set; }
        public List<Action> ActionList { get; set; }
        public ObservableCollection<Operation> SelectedOperation1 { get; set; } = new ObservableCollection<Operation>();
        public ObservableCollection<Operation> SelectedOperation2 { get; set; } = new ObservableCollection<Operation>();
        public NewRisk(List<Operation> AllOperations, List<OperationRisk> OpRiskList, List<Risk> RiskList, List<Action> ActionList, ObservableCollection<Operation> currentOperations)
        {
            InitializeComponent();
            this.AllOperations = AllOperations;
            this.OpRiskList = OpRiskList;
            this.RiskList = RiskList;
            this.CurrentOperations = CurrentOperations;

            //Bind data
            foreach (Operation op in AllOperations)
            {
                SelectedOperation1.Add(op);
            }
            riskCB.ItemsSource = RiskList;
            operationControl1.ItemsSource = SelectedOperation1;
            operationControl2.ItemsSource = SelectedOperation2;
            CurrentOperations = currentOperations;

            // risksControl1.ItemsSource = SelectedRisks1;


        }
        private void riskCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Risk ri = (Risk)riskCB.SelectedItem;
            SelectedOperation1.Clear();
            SelectedOperation2.Clear();

            foreach (Operation op in AllOperations)
            {

                if (OpRiskList.Any(o => o.RiskId.Equals(ri.Id) && o.OpId.Equals(op.Id)))
                {

                    SelectedOperation2.Add(op);
                }
                else
                {
                    SelectedOperation1.Add(op);
                }
            }
            riskTextBox.Text = ri.Type;
            consequenceTextBox.Text = ri.Consequence.ToString();
            probabilityTextBox.Text = ri.Probability.ToString();

        }
        private void risksControl1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
        private void addSelectedOperation_Click(object sender, RoutedEventArgs e)
        {
            Operation op = (Operation)operationControl1.SelectedItem;
            if (op == null)
            {

            }
            else
            {
                try
                {
                    Risk ri = (Risk)riskCB.SelectedItem;

                    makeNewOperationRiskConnection(op, ri);


                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void makeNewOperationRiskConnection(Operation op, Risk ri)
        {
            OperationRisk operationRisk = new OperationRisk(op.Id, ri.Id);
            OpRiskList.Add(operationRisk);
            FlightSafetyDB flightSafetyDB = new FlightSafetyDB();
            flightSafetyDB.storeItemInDbGetId(operationRisk.makeQuery());
            SelectedOperation1.Remove(op);
            operationControl1.Items.Refresh();
            SelectedOperation2.Add(op);
            operationControl2.Items.Refresh();
        }

        private void removeSelectedOperation_Click(object sender, RoutedEventArgs e)
        {
            Operation op = (Operation)operationControl2.SelectedItem;
            if (op == null)
            {
            }
            else
            {
                try
                {
                    Risk ri = (Risk)riskCB.SelectedItem;
                    int nrOfOpRi = OpRiskList.Count;
                    FlightSafetyDB flightSafetyDB = new FlightSafetyDB();
                    for (int i = 0; i < nrOfOpRi; i++)
                    {
                        if (ri.Id == OpRiskList[i].RiskId && op.Id == OpRiskList[i].OpId)
                        {
                            OperationRisk opRi = OpRiskList[i];
                            OpRiskList.Remove(opRi);
                            flightSafetyDB.updateItemInDB(opRi.makeDeleteQuery());
                            break;
                        }
                    }
                    SelectedOperation2.Remove(op);
                    SelectedOperation1.Add(op);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void removeRiskButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Risk ri = riskCB.SelectedItem as Risk;
                FlightSafetyDB flightSafetyDB = new FlightSafetyDB();
                flightSafetyDB.updateItemInDB(ri.makeDeleteQuery());

                int nrOfOpRi = OpRiskList.Count;
                for (int i = 0; i < nrOfOpRi; i++)
                {
                    if (ri.Id == OpRiskList[i].RiskId)
                    {
                        OpRiskList.RemoveAt(i);
                    }
                }
                MessageBox.Show($"{ri.Type} togs bort");
                RiskList.Remove(ri);

                riskCB.SelectedItem = RiskList[0];
                riskCB.Items.Refresh();

            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }



        private void addRiskButton_Click(object sender, RoutedEventArgs e)
        {
            addOrChangeRisk(true);
        }

        private void addOrChangeRisk(bool isNew)
        {
            Risk risk;
            string riskName = riskTextBox.Text;
            string riskProb = probabilityTextBox.Text;
            int riskProbability = 0;
            string riskCon = consequenceTextBox.Text;
            int riskConsequence = 0;

            if (!InputCheck.checkIfLengthIsOverInt(riskName, 2)) return;
            if (!InputCheck.checkIfLengthIsLessThanInt(riskName, 45)) return;
            try
            {
                riskProbability = Convert.ToInt32(riskProb);
                riskConsequence = Convert.ToInt32(riskCon);
                if (!InputCheck.checkIfNumberIsBetween1And10(riskProbability)) return;
                if (!InputCheck.checkIfNumberIsBetween1And10(riskConsequence)) return;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Konsekvens och sannolikhet behöver anges i siffror");
                return;

            }

            try
            {
                FlightSafetyDB flightSafetyDB = new FlightSafetyDB();

                if (isNew)
                {
                    if (InputCheck.checkIfRiskAlreadyExists(riskName, RiskList)) return;

                    risk = new Risk(1, riskName, riskProbability, riskConsequence);

                    int id = flightSafetyDB.storeItemInDbGetId(risk.makeQuery());
                    risk.Id = id;
                    RiskList.Add(risk);
                    riskCB.SelectedItem = risk;
                    foreach (Operation op in CurrentOperations)
                    {
                        makeNewOperationRiskConnection(op, risk);
                    }
                }
                if (!isNew)
                {
                    risk = riskCB.SelectedItem as Risk;
                    risk.Type = riskName;
                    risk.Probability = riskProbability;
                    risk.Consequence = riskConsequence;

                    //Skriv funktion för att uppdatera Item
                    flightSafetyDB.updateItemInDB(risk.makeUpdateQuery());
                    MessageBox.Show($"{risk.Type} uppdaterad!");
                }
                riskCB.Items.Refresh();
            }
            catch (Exception exe)
            {
                MessageBox.Show(exe.Message);
            }
        }

        private void changeRiskButton_Click(object sender, RoutedEventArgs e)
        {
            addOrChangeRisk(false);

        }
    }
}
