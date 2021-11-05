using NetworkEmulation.NetworkModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NetworkEmulatorApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double delay = 0;
        private double drop = 0.01;
        private double error = 0.01;
        double window = 10000;
        double timeout = 60000;

        public INetworkEmulator Emulator;
        public MainWindow()
        {
            
            InitializeComponent();

            SimulationDelayTextBox.Text = "0";
            DropProbabilityTextBox.Text = "0.01";
            ErrorProbabilityTextBox.Text = "0.01";
            RetransmitWindowTextBox.Text = "10000";
            ConnectionTimeoutTextBox.Text = "60000";
        }

        private void WorkspacePathBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            //nw.BuildDemoNetwork(fbd.SelectedPath);
            WorspacePathTextBlock.Text = fbd.SelectedPath;
            StartButton.IsEnabled = true;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            SimulationDelayTextBox.IsEnabled = false;
            DropProbabilityTextBox.IsEnabled = false;
            ErrorProbabilityTextBox.IsEnabled = false;
            RetransmitWindowTextBox.IsEnabled = false;
            ConnectionTimeoutTextBox.IsEnabled = false;

            StartDemoSimulation(WorspacePathTextBlock.Text);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;

            SimulationDelayTextBox.IsEnabled = true;
            DropProbabilityTextBox.IsEnabled = true;
            ErrorProbabilityTextBox.IsEnabled = true;
            RetransmitWindowTextBox.IsEnabled = true;
            ConnectionTimeoutTextBox.IsEnabled = true;
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            delay = double.Parse(SimulationDelayTextBox.Text);
            drop = double.Parse(DropProbabilityTextBox.Text);
            error = double.Parse(ErrorProbabilityTextBox.Text);
            window = int.Parse(RetransmitWindowTextBox.Text);
            timeout = int.Parse(ConnectionTimeoutTextBox.Text);
        }

        private void StartDemoSimulation(string workspace)
        {
            //Emulator = new LocalNetworkEmulator(workspace);

            IPAddress addr1 = new IPAddress(1);
            IPAddress addr2 = new IPAddress(2);
            IPAddress addr3 = new IPAddress(3);
            IPAddress addr4 = new IPAddress(4);

            IPAddress ep1 = new IPAddress(5);
            IPAddress ep2 = new IPAddress(6);
            IPAddress ep3 = new IPAddress(7);
            IPAddress ep4 = new IPAddress(8);

            //Emulator.AddNode(new CoreNode(addr1, delay));
            //Emulator.AddNode(new CoreNode(addr2, delay));
            //Emulator.AddNode(new CoreNode(addr3, delay));
            //Emulator.AddNode(new CoreNode(addr4, delay));

            //Emulator.AddEndNode(new EndpointNode(ep1, window, delay));
            //Emulator.AddEndNode(new EndpointNode(ep2, window, delay));
            //Emulator.AddEndNode(new EndpointNode(ep3, window, delay));
            //Emulator.AddEndNode(new EndpointNode(ep4, window, delay));

            ////Fully connected network
            //Emulator.AddLink(addr1, 1000, addr2, 1001, drop, error);
            //Emulator.AddLink(addr1, 1002, addr3, 1003, drop, error);
            //Emulator.AddLink(addr1, 1004, addr4, 1005, drop, error);

            //Emulator.AddLink(addr2, 1006, addr3, 1007, drop, error);
            //Emulator.AddLink(addr2, 1008, addr4, 1009, drop, error);

            //Emulator.AddLink(addr3, 1010, addr4, 1011, drop, error);

            ////Add endpoint links
            //Emulator.AddLink(ep1, 1100, addr1, 1101, drop, error);
            //Emulator.AddLink(ep2, 1102, addr2, 1103, drop, error);
            //Emulator.AddLink(ep3, 1104, addr3, 1105, drop, error);
            //Emulator.AddLink(ep4, 1106, addr4, 1107, drop, error);

            //foreach (IEndpointNode ep in Emulator.EndNodes)
            //{
            //    foreach (INodeConnection connection in ep)
            //    {

            //    }
            //    StartEndpointNode(ep, workspace);
            //}

            //foreach (ICoreNode cn in Emulator.Nodes)
            //{
            //    StartCoreNode(cn, workspace);
            //}

            //List<string> connections = new List<string>();
            //connections.Add(ep1.ToString() + ";" +
            //    "1100;" + ep2.ToString() + ";" +
            //    "1101;" + drop.ToString() + ";" + error.ToString());
            //StartEndpointNode(ep1, connections, workspace);

            //connections = new List<string>();
            //connections.Add(ep2.ToString() + ";" +
            //    "1101;" + ep1.ToString() + ";" +
            //    "1100;" + drop.ToString() + ";" + error.ToString());
            //StartEndpointNode(ep2, connections, workspace);

            //Endpoints
            List<string> connections = new List<string>();
            connections.Add(ep1.ToString() + ";" +
                "1100;" + addr1.ToString() + ";" +
                "1101;" + drop.ToString() + ";" + error.ToString());
            StartEndpointNode(ep1, connections, workspace);

            connections = new List<string>();
            connections.Add(ep2.ToString() + ";" +
                "1102;" + addr2.ToString() + ";" +
                "1103;" + drop.ToString() + ";" + error.ToString());
            StartEndpointNode(ep2, connections, workspace);

            connections = new List<string>();
            connections.Add(ep3.ToString() + ";" +
                "1104;" + addr3.ToString() + ";" +
                "1105;" + drop.ToString() + ";" + error.ToString());
            StartEndpointNode(ep3, connections, workspace);

            connections = new List<string>();
            connections.Add(ep4.ToString() + ";" +
                "1106;" + addr4.ToString() + ";" +
                "1107;" + drop.ToString() + ";" + error.ToString());
            StartEndpointNode(ep4, connections, workspace);

            //Core nodes
            connections = new List<string>();
            connections.Add(addr1.ToString() + ";" +
                "1000;" + addr2.ToString() + ";" +
                "1001;" + drop.ToString() + ";" + error.ToString());
            connections.Add(addr1.ToString() + ";" +
                "1002;" + addr3.ToString() + ";" +
                "1003;" + drop.ToString() + ";" + error.ToString());
            connections.Add(addr1.ToString() + ";" +
                "1004;" + addr4.ToString() + ";" +
                "1005;" + drop.ToString() + ";" + error.ToString());
            connections.Add(addr1.ToString() + ";" +
                "1101;" + ep1.ToString() + ";" +
                "1100;" + drop.ToString() + ";" + error.ToString());
            StartCoreNode(addr1, connections, workspace);

            connections = new List<string>();
            connections.Add(addr2.ToString() + ";" +
                "1001;" + addr1.ToString() + ";" +
                "1000;" + drop.ToString() + ";" + error.ToString());
            connections.Add(addr2.ToString() + ";" +
                "1006;" + addr3.ToString() + ";" +
                "1007;" + drop.ToString() + ";" + error.ToString());
            connections.Add(addr2.ToString() + ";" +
                "1008;" + addr4.ToString() + ";" +
                "1009;" + drop.ToString() + ";" + error.ToString());
            connections.Add(addr2.ToString() + ";" +
                "1103;" + ep2.ToString() + ";" +
                "1102;" + drop.ToString() + ";" + error.ToString());
            StartCoreNode(addr2, connections, workspace);

            connections = new List<string>();
            connections.Add(addr3.ToString() + ";" +
                "1003;" + addr1.ToString() + ";" +
                "1002;" + drop.ToString() + ";" + error.ToString());
            connections.Add(addr3.ToString() + ";" +
                "1007;" + addr2.ToString() + ";" +
                "1006;" + drop.ToString() + ";" + error.ToString());
            connections.Add(addr3.ToString() + ";" +
                "1010;" + addr4.ToString() + ";" +
                "1011;" + drop.ToString() + ";" + error.ToString());
            connections.Add(addr3.ToString() + ";" +
                "1105;" + ep3.ToString() + ";" +
                "1104;" + drop.ToString() + ";" + error.ToString());
            StartCoreNode(addr3, connections, workspace);

            connections = new List<string>();
            connections.Add(addr4.ToString() + ";" +
                "1005;" + addr1.ToString() + ";" +
                "1004;" + drop.ToString() + ";" + error.ToString());
            connections.Add(addr4.ToString() + ";" +
                "1009;" + addr2.ToString() + ";" +
                "1008;" + drop.ToString() + ";" + error.ToString());
            connections.Add(addr4.ToString() + ";" +
                "1011;" + addr3.ToString() + ";" +
                "1010;" + drop.ToString() + ";" + error.ToString());
            connections.Add(addr4.ToString() + ";" +
                "1107;" + ep4.ToString() + ";" +
                "1106;" + drop.ToString() + ";" + error.ToString());
            StartCoreNode(addr4, connections, workspace);
        }

        private void StartEndpointNode(IPAddress address, List<string> connections, string workspace)
        {
            string args = "EndPoint " +
                address.ToString() + " " +
                workspace + " " +
                delay + " " + 
                window;

            foreach (string c in connections)
            {
                args += " " + c;
            }

            Process process = new Process();
            process.StartInfo.FileName = "SingleNodeProcess.exe";
            process.StartInfo.Arguments = args;
            process.Start();
        }

        private void StartCoreNode(IPAddress address, List<string> connections, string workspace)
        {
            string args = "CoreNode " +
                address.ToString() + " " +
                workspace + " " +
                delay + " " +
                window;

            foreach (string c in connections)
            {
                args += " " + c;
            }

            Process process = new Process();
            process.StartInfo.FileName = "SingleNodeProcess.exe";
            process.StartInfo.Arguments = args;
            process.Start();
        }
    }
}
