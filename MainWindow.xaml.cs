using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Spire.Doc;

//using DataLibrary;

namespace Flygsäkerhetsanalysverktyg
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Operation> AllOperations { get; set; }
        public ObservableCollection<Operation> CurrentOperations { get; set; } = new ObservableCollection<Operation>();
        public List<OperationRisk> OpRiskList { get; set; }
        public List<Risk> RiskList { get; set; }
        public ObservableCollection<Risk> SelectedRisks1 { get; set; } = new ObservableCollection<Risk>();
        public ObservableCollection<Risk> SelectedRisks2 { get; set; } = new ObservableCollection<Risk>();
        public List<Action> ActionList { get; set; }
        public ObservableCollection<Action> SelectedAction1 { get; set; } = new ObservableCollection<Action>();
        public ObservableCollection<Action> SelectedAction2 { get; set; } = new ObservableCollection<Action>();

        Operation currentOp;

        public MainWindow()
        {
            InitializeComponent();
            loadData();
            bindData();
        }

        private void loadData()
        {
            //Läser in all data från databas i listor med metod i flightsafetydb
            FlightSafetyDB flightSafetyDB = new FlightSafetyDB();
            AllOperations = flightSafetyDB.getDataFromDB("call getAllOperations();", AllOperations);
            RiskList = flightSafetyDB.getDataFromDB("call getAllRisks();", RiskList);
            OpRiskList = flightSafetyDB.getDataFromDB("call getAllOpRisk", OpRiskList);
            ActionList = flightSafetyDB.getDataFromDB("call getAllActions", ActionList);
        }

        private void bindData()
        {
            //Binder data till de olika Combobox.
            verksamhetControl.ItemsSource = CurrentOperations;
            verksamhetCB.ItemsSource = AllOperations;
            risksControl1.ItemsSource = SelectedRisks1;
            risksControl2.ItemsSource = SelectedRisks2;
            // Event som uppdaterar Big Grid när något ändras i SelectedAction2
            SelectedAction2.CollectionChanged += new NotifyCollectionChangedEventHandler(updateActionList);

        }
        private void updateActionList(object sender, NotifyCollectionChangedEventArgs e)
        {
            updateBigGrid();
        }

        private void updateBigGrid()
        {

            bigGrid.Children.Clear();

            //Lägger till data i grid en rad i taget för varje risk som finns.
            foreach (Risk risk in SelectedRisks2)
            {
                RowDefinition rowdef = new RowDefinition();
                rowdef.Height = GridLength.Auto;

                bigGrid.RowDefinitions.Add(rowdef);
                //Indexet av alla risker i listan bestämmet var i gridden de hamnar
                int row = SelectedRisks2.IndexOf(risk);
                // Alla borders för de 6 kolumnerna
                Border border1 = new Border();
                Border border2 = new Border();
                Border border3 = new Border();
                Border border4 = new Border();
                Border border5 = new Border();
                Border border6 = new Border();

                //Lägger till data till kollumn 0 i grid
                StackPanel column0Stack = new StackPanel();
                TextBlock textRiskType = new TextBlock();
                textRiskType.FontWeight = FontWeights.Bold;
                column0Stack.Children.Add(textRiskType);
                textRiskType.Text = risk.Type;
                bigGrid.Children.Add(column0Stack);
                Grid.SetRow(column0Stack, row);
                Grid.SetRow(border1, row);
                bigGrid.Children.Add(border1);
                foreach (Operation op in CurrentOperations)
                {
                    if (OpRiskList.Any(o => o.RiskId.Equals(risk.Id) && o.OpId.Equals(op.Id)))
                    {
                        TextBlock operationInRisk = new TextBlock();
                        operationInRisk.TextWrapping = TextWrapping.Wrap;
                        operationInRisk.Text = op.Type;
                        column0Stack.Children.Add(operationInRisk);
                    }
                }

                //Lägger till data i kollumn 1 i grid 
                TextBlock textProb = new TextBlock();
                textProb.Style = Resources["putInTheMiddle"] as System.Windows.Style;
                textProb.Text = risk.Probability.ToString();
                bigGrid.Children.Add(textProb);
                Grid.SetRow(textProb, row);
                Grid.SetColumn(textProb, 1);
                Grid.SetRow(border2, row);
                Grid.SetColumn(border2, 1);
                bigGrid.Children.Add(border2);

                //Lägger till data i kollumn 2 i grid
                TextBlock textCons = new TextBlock();
                textCons.Style = Resources["putInTheMiddle"] as System.Windows.Style;
                textCons.Text = risk.Consequence.ToString();
                bigGrid.Children.Add(textCons);
                Grid.SetRow(textCons, row);
                Grid.SetColumn(textCons, 2);
                Grid.SetRow(border3, row);
                Grid.SetColumn(border3, 2);
                bigGrid.Children.Add(border3);

                //Lägger till data i kollumn 3 i grid
                bigGrid.Children.Add(border4);
                Grid.SetRow(border4, row);
                Grid.SetColumn(border4, 3);
                string riskIndexString = calculateRiskIndex(risk.Probability, risk.Consequence);
                changeBackgroundColorOfBorder(border4, riskIndexString);
                TextBlock textRiskIndex = new TextBlock();
                textRiskIndex.FontWeight = FontWeights.Bold;
                textRiskIndex.Style = Resources["putInTheMiddle"] as System.Windows.Style;
                textRiskIndex.Text = riskIndexString;
                bigGrid.Children.Add(textRiskIndex);
                Grid.SetRow(textRiskIndex, row);
                Grid.SetColumn(textRiskIndex, 3);


                //Lägger till data i kollumn 4 i grid
                StackPanel actionStack = new StackPanel();
                bigGrid.Children.Add(actionStack);
                Grid.SetRow(actionStack, row);
                Grid.SetColumn(actionStack, 4);
                bigGrid.Children.Add(border5);
                Grid.SetRow(border5, row);
                Grid.SetColumn(border5, 4);
                StackPanel stackPrev = new StackPanel();
                StackPanel stackNonPrev = new StackPanel();
                actionStack.Children.Add(stackPrev);
                actionStack.Children.Add(stackNonPrev);
                addActionToGrid(risk, stackPrev, stackNonPrev);

                //Lägger till data i kollumn 5 i grid
                bigGrid.Children.Add(border6);
                Grid.SetRow(border6, row);
                Grid.SetColumn(border6, 5);
                // Default värde för mitigerade åtgärder är 0, så de ska bara visas om man aktivt ändrat i dem.
                if (risk.ConsequenceMitigated > 0 && risk.ProbabilityMitigated > 0)
                {
                    TextBlock newRiskIndex = new TextBlock();
                    newRiskIndex.Text = calculateRiskIndex(risk.ProbabilityMitigated, risk.ConsequenceMitigated);
                    newRiskIndex.FontWeight = FontWeights.Bold;
                    TextBlock probString = new TextBlock();
                    probString.Text = risk.getProbabilityChangeString();
                    TextBlock consString = new TextBlock();
                    consString.Text = risk.getConsequenceChangeString();
                    StackPanel remainingRiskStack = new StackPanel();
                    remainingRiskStack.Children.Add(newRiskIndex);
                    remainingRiskStack.Children.Add(probString);
                    remainingRiskStack.Children.Add(consString);
                    bigGrid.Children.Add(remainingRiskStack);
                    Grid.SetRow(remainingRiskStack, row);
                    Grid.SetColumn(remainingRiskStack, 5);
                    changeBackgroundColorOfBorder(border6, newRiskIndex.Text);
                }
            }
        }
        private void changeBackgroundColorOfBorder(Border border, string risk)
        {
            //Ändrar bakgrundsfärg beroende på risk
            if (risk.Equals("Ingen synbar risk")) border.Background = Brushes.White;
            if (risk.Equals("Låg risk")) border.Background = Brushes.Green;
            if (risk.Equals("Förhöjd risk")) border.Background = Brushes.Yellow;
            if (risk.Equals("Hög hög risk")) border.Background = Brushes.Orange;
            if (risk.Equals("Mycket hög risk")) border.Background = Brushes.Red;
        }

        private void addActionToGrid(Risk risk, StackPanel stackPrev, StackPanel stackNonPrev)
        {
            //Lägger till data i kollumn 4 i grid och placerar ut åtgärder efter typ
            TextBlock prevText = new TextBlock();
            prevText.Text = "Preventiva åtgärder";
            prevText.TextDecorations = TextDecorations.Underline;
            stackPrev.Children.Add(prevText);

            TextBlock nonPrevText = new TextBlock();
            nonPrevText.Text = "Direkta åtgärder";
            nonPrevText.TextDecorations = TextDecorations.Underline;
            stackNonPrev.Children.Add(nonPrevText);

            foreach (Action action in SelectedAction2)
            {
                if (action.FromRiskID == risk.Id)
                {
                    TextBlock textType = new TextBlock();
                    textType.Text = "- " + action.ActionContent;
                    textType.TextWrapping = TextWrapping.Wrap;
                    textType.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    if (action.IsPreventive == true) stackPrev.Children.Add(textType);
                    else stackNonPrev.Children.Add(textType);
                }
            }
        }


        private void addVerksamhet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //KOllar om Verksamheten redan är tillags med ID
                if (CurrentOperations.Any(n => n.Id == currentOp.Id))
                {
                    MessageBox.Show("Redan tillagd");
                }
                else
                {
                    //Kollar matchning med operation mot junctiontable som kollar matchning med id mot risk och lägger till en clone av alla matchande risker til selected risks
                    CurrentOperations.Add(currentOp.clone());
                    var matchingRisks = from opRi in OpRiskList
                                        join risk in RiskList on opRi.RiskId equals risk.Id
                                        where opRi.OpId == currentOp.Id
                                        select risk;
                    foreach (var risk in matchingRisks)
                    {
                        if (!SelectedRisks1.Any(n => n.Type.Equals(risk.Type)) && !SelectedRisks2.Any(n => n.Type.Equals(risk.Type)))
                        {
                            SelectedRisks1.Add(risk.clone());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Välj en verksamhet");
            }
            verksamhetControl.Items.Refresh();
            // Uppdaterar big grid med de nya valen
            updateBigGrid();
        }
        private void verksamhetCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //sätter currentOp till den verksamhet som valts i rullistan
            try
            {
                currentOp = verksamhetCB.SelectedItem as Operation;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void verksamhetControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            removeOperationn();
        }

        private void removeOperation_Click(object sender, RoutedEventArgs e)
        {
            removeOperationn();
        }

        private void removeOperationn()
        {
            //Tar bort operation från listan
            Operation op = (Operation)verksamhetControl.SelectedItem;
            CurrentOperations.Remove(op);
        }

        private void addSelectedRisk_Click(object sender, RoutedEventArgs e)
        {
            //Lägger över markerad risk i selectedrisk2
            Risk ri = (Risk)risksControl1.SelectedItem;
            if (ri == null)
            {

            }
            else
            {
                SelectedRisks1.Remove(ri);
                SelectedRisks2.Add(ri);
                updateBigGrid();
            }
        }

        private void addAllRisk_Click(object sender, RoutedEventArgs e)
        {
            //Lägger över alla risker i fönstret till selectedrisk2
            if (SelectedRisks1.Count.Equals(0))
            {
            }
            else
            {
                foreach (Risk ri in SelectedRisks1)
                {
                    SelectedRisks2.Add(ri);
                }
                SelectedRisks1.Clear();
                updateBigGrid();
            }
        }

        private void removeSelectedRisk_Click(object sender, RoutedEventArgs e)
        {
            //Byter lista på vald risk
            Risk ri = (Risk)risksControl2.SelectedItem;
            if (ri == null)
            {
            }
            else
            {
                SelectedRisks2.Remove(ri);
                SelectedRisks1.Add(ri);
                SelectedAction1.Clear();
                actionsControl1Preventive.Items.Clear();
                actionsControl1NonPreventive.Items.Clear();
                updateBigGrid();
            }
        }

        private void removeAllRisk_Click(object sender, RoutedEventArgs e)
        {
            //Byter lista på alla risker
            foreach (Risk ri in SelectedRisks2)
            {
                SelectedRisks1.Add(ri);
            }
            SelectedRisks2.Clear();
            updateBigGrid();
        }
        private void risksControl2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Uppdaterar selection1 när selection2 uppdateras
            SelectedAction1.Clear();
            Risk ri = (Risk)risksControl2.SelectedItem;
            if (ri == null)
            {
            }
            else
            {
                foreach (Action ac in ActionList)
                {
                    if (ac.FromRiskID == ri.Id)
                    {
                        SelectedAction1.Add(ac);
                    }
                }
                showActionsinActionControl1(actionsControl1Preventive, actionsControl1NonPreventive);
            }
        }

        private void showActionsinActionControl1(ListBox listBoxPreventive, ListBox listBoxNonPreventive)
        {
            //Visar alla actions som associeras med vald risk och lägger ut dem i två olika listboxes beroende på värdet i ispreventive
            listBoxPreventive.Items.Clear();
            listBoxNonPreventive.Items.Clear();
            foreach (Action ac in SelectedAction1)
            {
                if (ac.IsPreventive == true)
                {

                    addListBoxItemTemplate(ac, listBoxPreventive, true);
                }
                if (ac.IsPreventive == false)
                {
                    addListBoxItemTemplate(ac, listBoxNonPreventive, true);
                }
            }
            listBoxPreventive.UpdateLayout();
            listBoxNonPreventive.UpdateLayout();
        }
        private void addListBoxItemTemplate(Action action, ListBox listBox, bool addCheckbox)
        {
            //En item template för varje action i listboxen
            StackPanel stack = new StackPanel();
            if (addCheckbox)
            {
                CheckBox checkBox = new CheckBox();
                if (action.IsChecked) checkBox.IsChecked = true;
                checkBox.Checked += new RoutedEventHandler(actionCheckBoxChecked);
                checkBox.Unchecked += new RoutedEventHandler(actionCheckBoxChecked);
                stack.Children.Add(checkBox);
            }
            TextBlock text = new TextBlock();
            text.Text = action.ActionContent;

            stack.Orientation = Orientation.Horizontal;

            stack.Children.Add(text);
            ListBoxItem itemText = new ListBoxItem();
            itemText.Content = stack;
            listBox.Items.Add(itemText);
        }
        private void actionCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            //Lägger över action i selectedaction2 om man klickar i den.
            CheckBox check = (CheckBox)sender;
            StackPanel stack = (StackPanel)check.Parent;
            TextBlock block = (TextBlock)stack.Children[1];
            Action ac = SelectedAction1.Single(a => a.ActionContent == block.Text);
            if (check.IsChecked == true)
            {
                ac.IsChecked = true;
                SelectedAction2.Add(ac.clone());
            }
            else
            {
                Action act = SelectedAction2.SingleOrDefault(a => a.ActionContent == block.Text);
                if (act != null) SelectedAction2.Remove(act);
                ac.IsChecked = false;
            }
        }

        private void addActionButton_Click(object sender, RoutedEventArgs e)
        {
            // Lägger till en action i Databasen
            int fromRiskId = 0;
            try
            {
                Risk ri = (Risk)risksControl2.SelectedItem;
                fromRiskId = ri.Id;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Välj en risk att lägga till åtgärd till");
                return;
            }
            string actionContent = addActionTextBox.Text;
            bool isPreventive = (bool)addActionCheckBox.IsChecked;
            int actionId = 0;

            //Input validering. Avbryter metoden och ger ett felmeddelande om man är utanför värdena.
            if (!InputCheck.checkIfLengthIsOverInt(actionContent, 15)) return;
            if (!InputCheck.checkRiskIdIsNotZero(fromRiskId)) return;
            if (!InputCheck.checkIfTextIsRemotelyUnique(actionContent, SelectedAction1)) return;
            if (!InputCheck.checkIfLengthIsLessThanInt(actionContent, 150)) return;

            //SKapar ett action-objekt och lägger till lsitan
            Action action = new Action(actionId, actionContent, isPreventive, fromRiskId);
            addActionCheckBox.IsChecked = true;
            ActionList.Add(action);
            SelectedAction1.Add(action);
            SelectedAction2.Add(action.clone());
            addActionTextBox.Clear();

            showActionsinActionControl1(actionsControl1Preventive, actionsControl1NonPreventive);

            try
            {
                //Lägger in action i databasen
                FlightSafetyDB flightSafetyDB = new FlightSafetyDB();
                actionId = flightSafetyDB.storeItemInDbGetId(action.makeQuery());
                action.ActionID = actionId;
                MessageBox.Show(action.ActionContent + " tillagd!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }



        private void newRisk_Click(object sender, RoutedEventArgs e)
        {
            //Öppnar nytt fönster för riskinställningar
            NewRisk newRisk = new NewRisk(AllOperations, OpRiskList, RiskList, ActionList, CurrentOperations);
            newRisk.Show();

        }

        private void newOperation_Click(object sender, RoutedEventArgs e)
        {
            // Öppnar nytt fönster för verksamhetsinställningar
            NewOperationWindow newOperationWindow = new NewOperationWindow(AllOperations, OpRiskList, RiskList);
            newOperationWindow.Show();
        }

        private void changeActionButton_Click(object sender, RoutedEventArgs e)
        {
            //Öppnar nytt fönster för åtgärdsinställningar
            NewActionWindow newActionWindow = new NewActionWindow(RiskList, ActionList, SelectedRisks2);
            newActionWindow.Show();
        }

        private string calculateRiskIndex(int probability, int consequence)
        {
            //riskArray [prop, consequence] ger tillbaka ett nummervärde enligt nedanstående matris.
            int[,] riskArray = new int[11, 11];
            riskArray[1, 1] = 1;
            riskArray[2, 1] = 2;
            riskArray[3, 1] = 2;
            riskArray[4, 1] = 2;
            riskArray[5, 1] = 2;
            riskArray[6, 1] = 2;
            riskArray[7, 1] = 2;
            riskArray[8, 1] = 2;
            riskArray[9, 1] = 3;
            riskArray[10, 1] = 3;

            riskArray[1, 2] = 2;
            riskArray[2, 2] = 2;
            riskArray[3, 2] = 2;
            riskArray[4, 2] = 2;
            riskArray[5, 2] = 2;
            riskArray[6, 2] = 2;
            riskArray[7, 2] = 3;
            riskArray[8, 2] = 3;
            riskArray[9, 2] = 3;
            riskArray[10, 2] = 3;

            riskArray[1, 3] = 2;
            riskArray[2, 3] = 2;
            riskArray[3, 3] = 2;
            riskArray[4, 3] = 2;
            riskArray[5, 3] = 2;
            riskArray[6, 3] = 3;
            riskArray[7, 3] = 3;
            riskArray[8, 3] = 3;
            riskArray[9, 3] = 3;
            riskArray[10, 3] = 3;

            riskArray[1, 4] = 2;
            riskArray[2, 4] = 2;
            riskArray[3, 4] = 2;
            riskArray[4, 4] = 2;
            riskArray[5, 4] = 3;
            riskArray[6, 4] = 3;
            riskArray[7, 4] = 3;
            riskArray[8, 4] = 3;
            riskArray[9, 4] = 3;
            riskArray[10, 4] = 3;

            riskArray[1, 5] = 2;
            riskArray[2, 5] = 2;
            riskArray[3, 5] = 2;
            riskArray[4, 5] = 3;
            riskArray[5, 5] = 3;
            riskArray[6, 5] = 3;
            riskArray[7, 5] = 3;
            riskArray[8, 5] = 4;
            riskArray[9, 5] = 4;
            riskArray[10, 5] = 4;

            riskArray[1, 6] = 3;
            riskArray[2, 6] = 3;
            riskArray[3, 6] = 3;
            riskArray[4, 6] = 4;
            riskArray[5, 6] = 4;
            riskArray[6, 6] = 4;
            riskArray[7, 6] = 4;
            riskArray[8, 6] = 4;
            riskArray[9, 6] = 5;
            riskArray[10, 6] = 5;

            riskArray[1, 7] = 4;
            riskArray[2, 7] = 4;
            riskArray[3, 7] = 4;
            riskArray[4, 7] = 4;
            riskArray[5, 7] = 4;
            riskArray[6, 7] = 4;
            riskArray[7, 7] = 5;
            riskArray[8, 7] = 5;
            riskArray[9, 7] = 5;
            riskArray[10, 7] = 5;

            riskArray[1, 8] = 4;
            riskArray[2, 8] = 4;
            riskArray[3, 8] = 4;
            riskArray[4, 8] = 4;
            riskArray[5, 8] = 4;
            riskArray[6, 8] = 5;
            riskArray[7, 8] = 5;
            riskArray[8, 8] = 5;
            riskArray[9, 8] = 5;
            riskArray[10, 8] = 5;

            riskArray[1, 9] = 4;
            riskArray[2, 9] = 4;
            riskArray[3, 9] = 4;
            riskArray[4, 9] = 4;
            riskArray[5, 9] = 5;
            riskArray[6, 9] = 5;
            riskArray[7, 9] = 5;
            riskArray[8, 9] = 5;
            riskArray[9, 9] = 5;
            riskArray[10, 9] = 5;

            riskArray[1, 10] = 4;
            riskArray[2, 10] = 4;
            riskArray[3, 10] = 5;
            riskArray[4, 10] = 5;
            riskArray[5, 10] = 5;
            riskArray[6, 10] = 5;
            riskArray[7, 10] = 5;
            riskArray[8, 10] = 5;
            riskArray[9, 10] = 5;
            riskArray[10, 10] = 5;

            int riskIndexInt = riskArray[probability, consequence];
            string riskIndexString = "";
            if (riskIndexInt == 1) riskIndexString = "Ingen synbar risk";
            if (riskIndexInt == 2) riskIndexString = "Låg risk";
            if (riskIndexInt == 3) riskIndexString = "Förhöjd risk";
            if (riskIndexInt == 4) riskIndexString = "Hög hög risk";
            if (riskIndexInt == 5) riskIndexString = "Mycket hög risk";

            return riskIndexString;
        }

        private void mittigateRisk_Click(object sender, RoutedEventArgs e)
        {
            //Ger möjlighet att ge risken ny konsekvens och sannolikhet och uppdaterar big grid med det värdet
            int newCons = 0;
            int newProb = 0;
            try
            {
                Risk ri = (Risk)risksControl2.SelectedItem;
                try
                {
                    newProb = Convert.ToInt32(newProbTextBox.Text);
                    newCons = Convert.ToInt32(newConsTextBox.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                //Validerar inputen
                if (!InputCheck.checkIfNumberIsBetween1And10(newCons)) return;
                if (!InputCheck.checkIfNumberIsBetween1And10(newProb)) return;
                ri.ConsequenceMitigated = newCons;
                ri.ProbabilityMitigated = newProb;
                updateBigGrid();
            }
            catch (Exception c)
            {
                MessageBox.Show(c.Message);
            }
        }

        private void exportToWord_Click(object sender, RoutedEventArgs e)
        {
            //Funktion som  exporterar flygsäkerhetsanalysen till Word. Läggs i bin/debug. Under utveckling. Tanken är att skriva ut en grid likt den som syns i programmet
            Document doc = new Document();
            Spire.Doc.Section section = doc.AddSection();
            Spire.Doc.Documents.Paragraph Para = section.AddParagraph();
            Para.AppendText("Så långt har jag inte kommit! Men den skapar i alla " +
                "fall en wordfil med den här texten i :)");
            doc.SaveToFile("Flygsäkerhetsanalys.docx", FileFormat.Docx);
        }

        private void openHelp_Click(object sender, RoutedEventArgs e)
        {
            //Öppnar hjälpfönstret med förklaringar
            HelpWindow helpWindow = new HelpWindow();
            helpWindow.Show();
        }
    }
}
