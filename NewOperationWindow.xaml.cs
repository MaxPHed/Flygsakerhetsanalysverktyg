using MySql.Data.MySqlClient;
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
    /// Interaction logic for NewOperationWindow.xaml
    /// </summary>
    public partial class NewOperationWindow : Window
    {
        public List<Operation> AllOperations { get; set; }
        public List<OperationRisk> OpRiskList { get; set; }
        public List<Risk> RiskList { get; set; }
        public List<Action> ActionList { get; set; }
        public ObservableCollection<Risk> SelectedRisk1 { get; set; } = new ObservableCollection<Risk>();
        public ObservableCollection<Risk> SelectedRisk2 { get; set; } = new ObservableCollection<Risk>();
        public NewOperationWindow(List<Operation> AllOperations, List<OperationRisk> OpRiskList, List<Risk> RiskList)
        {
            InitializeComponent();
            this.AllOperations = AllOperations;
            this.OpRiskList = OpRiskList;
            this.RiskList = RiskList;

            //Bind data
            foreach (Risk ri in RiskList)
            {
                SelectedRisk1.Add(ri);
            }
            operationCB.ItemsSource = AllOperations;
            riskControl1.ItemsSource = SelectedRisk1;
            riskControl2.ItemsSource = SelectedRisk2;
        }
        private void riskCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Visar risker som hör till varje operation
            Operation op = (Operation)operationCB.SelectedItem;
            SelectedRisk1.Clear();
            SelectedRisk2.Clear();

            foreach (Risk ri in RiskList)
            {

                if (OpRiskList.Any(o => o.RiskId.Equals(ri.Id) && o.OpId.Equals(op.Id)))
                {

                    SelectedRisk2.Add(ri);
                }
                else
                {
                    SelectedRisk1.Add(ri);
                }
            }
            operationTextBox.Text = op.Type;
        }
 
        private void addSelectedRisk_Click(object sender, RoutedEventArgs e)
        {
            //Lägger till koppling mellan risk och operation och uppdaterar databasen.
            Risk ri = (Risk)riskControl1.SelectedItem;
            if (ri == null)
            {
            }
            else
            {
                try
                {
                    Operation op = (Operation)operationCB.SelectedItem;


                    OperationRisk operationRisk = new OperationRisk(op.Id, ri.Id);
                    OpRiskList.Add(operationRisk);
                    FlightSafetyDB flightSafetyDB = new FlightSafetyDB();
                    flightSafetyDB.storeItemInDbGetId(operationRisk.makeQuery());
                    SelectedRisk1.Remove(ri);
                    SelectedRisk2.Add(ri);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }


        private void removeSelectedRisk_Click(object sender, RoutedEventArgs e)
        {
            // Tar bort kopplin mellan risk och operation
            // junction table i DB uppdateras genom cascade
            Risk ri = (Risk)riskControl2.SelectedItem;
            if (ri == null)
            {
            }
            else
            {
                try
                {
                    Operation op = (Operation)operationCB.SelectedItem;
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
                    SelectedRisk2.Remove(ri);
                    SelectedRisk1.Add(ri);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void removeOperationButton_Click(object sender, RoutedEventArgs e)
        {
            //Tar bort operation från databas
            try
            {
                Operation op = operationCB.SelectedItem as Operation;
                FlightSafetyDB flightSafetyDB = new FlightSafetyDB();
                flightSafetyDB.updateItemInDB(op.makeDeleteQuery());

                int nrOfOpRi = OpRiskList.Count;
                for (int i = 0; i < nrOfOpRi; i++)
                {
                    if (op.Id == OpRiskList[i].OpId)
                    {
                        OpRiskList.RemoveAt(i);
                    }
                }
                MessageBox.Show($"{op.Type} togs bort");
                AllOperations.Remove(op);

                operationCB.SelectedItem = AllOperations[0];
                operationCB.Items.Refresh();

            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }



        private void addOperationButton_Click(object sender, RoutedEventArgs e)
        {
            addOrChangeOperation(true);
        }

        private void addOrChangeOperation(bool isNew)
        {
            //Tar bort eller ändrar operation i db beroende på vilken knapp som trycks på (bool)
            Operation operation;
            string operationName = operationTextBox.Text;
            //Validering
            if (!InputCheck.checkIfLengthIsOverInt(operationName, 2)) return;
            if (!InputCheck.checkIfLengthIsLessThanInt(operationName, 45)) return;
            try
            {
                FlightSafetyDB flightSafetyDB = new FlightSafetyDB();

                if (isNew)
                {
                    if (!InputCheck.checkIfOperationAlreadyExists(operationName, AllOperations)) return;

                    operation = new Operation(1, operationName);

                    int id = flightSafetyDB.storeItemInDbGetId(operation.makeQuery());
                    operation.Id = id;
                    AllOperations.Add(operation);
                    operationCB.SelectedItem = operation;
                }
                if (!isNew)
                {
                    operation = operationCB.SelectedItem as Operation;
                    operation.Type = operationName;

                    //Skriv funktion för att uppdatera Item
                    flightSafetyDB.updateItemInDB(operation.makeUpdateQuery());
                    MessageBox.Show($"{operation.Type} uppdaterad!");
                }
                operationCB.Items.Refresh();
            }
            catch (Exception exe)
            {
                MessageBox.Show(exe.Message);
            }

        }

        private void changeOperationButton_Click(object sender, RoutedEventArgs e)
        {
            addOrChangeOperation(false);

        }

        private void showAllOperationsAndRisks_Click(object sender, RoutedEventArgs e)
        {
            //Visar alla risker och alla operations. Hade lika gärna kunnats göra genom att jämföra listor, men har det för att 
            //ha en selectsats i db med join och junctiontable för VG :)
            try
            {
                FlightSafetyDB flightSafetyDB = new FlightSafetyDB();
                string query = "viewConnectionsBetweenOperationsAndRisks();";
                operationsDataGrid.ItemsSource = flightSafetyDB.fillDataTable(query);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
