using NetworkEmulation.NetworkModel;
using NetworkEmulation.PacketModels;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace SingleNodeProcess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        INetworkNode mMainNode;
        double mDelay;
        double mTimeout;
        string mWorkspacePath;
        Random mRand = new Random();

        public MainWindow()
        {
            InitializeComponent();

            mDelay = 10;
            mTimeout = 30000;

            StatusComboBox.Items.Add("Main Node");
            StatusComboBox.SelectedIndex = 0;

            try
            {
                if (App.args != null && App.args.Length > 0)
                {
                    mWorkspacePath = App.args[2];
                    mDelay = double.Parse(App.args[3]);
                    mTimeout = double.Parse(App.args[4]);

                    SetValue(Window.TitleProperty, App.args[0] + ": " + App.args[1] + ", " + mWorkspacePath);
                    if (App.args[0] == "CoreNode")
                    {
                        mMainNode = new CoreNode(IPAddress.Parse(App.args[1]), mDelay);
                    }
                    else if (App.args[0] == "EndPoint")
                    {
                        mMainNode = new EndpointNode(IPAddress.Parse(App.args[1]), mTimeout, mDelay);
                        ((EndpointNode)mMainNode).OnFileTransferCompleted += MainWindow_OnFileTransferCompleted;
                        FileTransferControl.Visibility = System.Windows.Visibility.Visible;
                        Directory.CreateDirectory(mWorkspacePath + "/" + mMainNode.NodeAddress.ToString());
                    }

                    //Add connection for each other argument
                    for (int i = 5; i < App.args.Length; ++i)
                    {
                        
                        string[] properties = App.args[i].Split(';');

                        IPAddress src = IPAddress.Parse(properties[0]);
                        int portSrc = int.Parse(properties[1]);
                        IPAddress dst = IPAddress.Parse(properties[2]);
                        int portDst = int.Parse(properties[3]);
                        double drop = double.Parse(properties[4]);
                        double error = double.Parse(properties[5]);

                        mMainNode.AddConnectionPoint(new NodeConnection(src,
                            "localhost",
                            portSrc,
                            portDst,
                            new IPEndPoint(dst, portDst),
                            System.Net.Sockets.AddressFamily.InterNetwork,
                            drop,
                            error));
                        mMainNode.ConnectionPoints.Last().Value.OnPacketDropped += Link_OnPacketDropped;
                        mMainNode.ConnectionPoints.Last().Value.OnPacketError += Link_OnPacketError;
                        mMainNode.ConnectionPoints.Last().Value.OnPacketReceived += Link_OnPacketReceived;

                        StatusComboBox.Items.Add("Link (Port " + mMainNode.ConnectionPoints.Last().Value.LocalPort.ToString() + ")");
                    }

                    mMainNode.OnPacketReceived += mMainNode_OnPacketReceived;
                    mMainNode.OnRoutingProbeReceived += mMainNode_OnRoutingProbeReceived;

                    mMainNode.Listen();

                    StartButton.IsEnabled = false;
                    StopButton.IsEnabled = true;
                    SendRoutingProbeButton.IsEnabled = true;
                    BeginTransferButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error occurred while initializing node: " + ex.Message + " inner: " + ex.InnerException.Message, "Node Error");
            }
        }

        void Link_OnPacketReceived(object sender, OnPacketReceivedArgs e)
        {
            this.Dispatcher.Invoke(() =>
                {
                    string statusItem = StatusComboBox.SelectedItem.ToString();

                    foreach (INodeConnection c in mMainNode.ConnectionPoints.Values)
                    {
                        string name = "Link (Port " + c.LocalPort.ToString() + ")";
                        if (statusItem == name)
                        {
                            string message = "\n" + e.Time.ToString() + " - Packet RECEIVED - " +
                                "src: " + e.Packet.SourceAddress.ToString() +
                                ", dst: " + e.Packet.DestinationAddress.ToString() +
                                ", type: " + e.Packet.Type.ToString() +
                                ", size: " + e.Packet.PacketBytes.Length.ToString();

                            if (e.Packet is FileTransferPacket)
                            {
                                FileTransferPacket ftp = (FileTransferPacket)e.Packet;
                                message += ", code: " + ftp.Code.ToString() +
                                    ", index: " + ftp.BlockIndex.ToString() +
                                    ", transferId: " + ftp.TransferId.ToString();
                            }

                            StatusLogTextBox.AppendText(message);
                            break;
                        }
                    }
                });
        }

        void Link_OnPacketError(object sender, OnPacketReceivedArgs e)
        {
            this.Dispatcher.Invoke(() =>
                {
                    string statusItem = StatusComboBox.SelectedItem.ToString();

                    foreach (INodeConnection c in mMainNode.ConnectionPoints.Values)
                    {
                        string name = "Link (Port " + c.LocalPort.ToString() + ")";
                        if (statusItem == name)
                        {
                            string message = "\n" + e.Time.ToString() + " - Packet ERROR - " +
                                "src: " + e.Packet.SourceAddress.ToString() +
                                ", dst: " + e.Packet.DestinationAddress.ToString() +
                                ", type: " + e.Packet.Type.ToString() +
                                ", size: " + e.Packet.PacketBytes.Length.ToString();

                            if (e.Packet is FileTransferPacket)
                            {
                                FileTransferPacket ftp = (FileTransferPacket)e.Packet;
                                message += ", code: " + ftp.Code.ToString() +
                                    ", index: " + ftp.BlockIndex.ToString() +
                                    ", transferId: " + ftp.TransferId.ToString();
                            }

                            StatusLogTextBox.AppendText(message);
                            break;
                        }
                    }
                });
        }

        void Link_OnPacketDropped(object sender, OnPacketReceivedArgs e)
        {
            this.Dispatcher.Invoke(() =>
                {
                    string statusItem = StatusComboBox.SelectedItem.ToString();

                    foreach (INodeConnection c in mMainNode.ConnectionPoints.Values)
                    {
                        string name = "Link (Port " + c.LocalPort.ToString() + ")";
                        if (statusItem == name)
                        {
                            string message = "\n" + e.Time.ToString() + " - Packet DROPPED - " +
                                "src: " + e.Packet.SourceAddress.ToString() +
                                ", dst: " + e.Packet.DestinationAddress.ToString() +
                                ", type: " + e.Packet.Type.ToString() +
                                ", size: " + e.Packet.PacketBytes.Length.ToString();

                            if (e.Packet is FileTransferPacket)
                            {
                                FileTransferPacket ftp = (FileTransferPacket)e.Packet;
                                message += ", code: " + ftp.Code.ToString() +
                                    ", index: " + ftp.BlockIndex.ToString() +
                                    ", transferId: " + ftp.TransferId.ToString();
                            }

                            StatusLogTextBox.AppendText(message);
                            StatusLogTextBox.ScrollToEnd();
                            break;
                        }
                    }
                });
        }

        void mMainNode_OnPacketReceived(object sender, OnPacketReceivedArgs e)
        {
            this.Dispatcher.Invoke(() =>
                {
                    string statusItem = StatusComboBox.SelectedItem.ToString();

                    if (statusItem == "Main Node")
                    {
                        string message = "\n" + e.Time.ToString() + " - Packet RECEIVED - " +
                            "src: " + e.Packet.SourceAddress.ToString() +
                            ", dst: " + e.Packet.DestinationAddress.ToString() +
                            ", type: " + e.Packet.Type.ToString() +
                            ", size: " + e.Packet.PacketBytes.Length.ToString();

                        if (e.Packet is FileTransferPacket)
                        {
                            FileTransferPacket ftp = (FileTransferPacket)e.Packet;
                            message += ", code: " + ftp.Code.ToString() +
                                ", index: " + ftp.BlockIndex.ToString() +
                                ", transferId: " + ftp.TransferId.ToString();
                        }

                        StatusLogTextBox.AppendText(message);
                        StatusLogTextBox.ScrollToEnd();
                    }
                });
        }

        void mMainNode_OnRoutingProbeReceived(object sender, OnRoutingProbeReceivedArgs e)
        {
            this.Dispatcher.Invoke(() =>
                {
                    string statusItem = StatusComboBox.SelectedItem.ToString();

                    if (statusItem == "Main Node" && (bool)RoutingProbeCheckBox.IsChecked)
                    {
                        string message = "\n" + e.Time.ToString() + " - ROUTING PROBE - " +
                            "prev: " + e.PreviousAddress.ToString() +
                            ", current: " + e.CurrentAddress.ToString() +
                            ", next: " + e.NextAddress.ToString() +
                            ", id: " + e.ProbeId.ToString();

                        StatusLogTextBox.AppendText(message);
                        StatusLogTextBox.ScrollToEnd();
                    }
                });
        }

        void MainWindow_OnFileTransferCompleted(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
                {
                    string statusItem = StatusComboBox.SelectedItem.ToString();

                    if (statusItem == "Main Node")
                    {
                        string message = "\n" + DateTime.UtcNow.ToString() + " - File Transfer COMPLETED";

                        StatusLogTextBox.AppendText(message);
                        StatusLogTextBox.ScrollToEnd();
                    }
                });
        }

        private void BeginTransferButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            if (!string.IsNullOrEmpty(ofd.FileName))
            {
                string srcPath = ofd.FileName;
                
                string parentDirectory = System.IO.Path.GetDirectoryName(srcPath);
                IPAddress address = IPAddress.Parse(System.IO.Path.GetFileName(parentDirectory));

                if (address != mMainNode.NodeAddress)
                {
                    string saveTo = mWorkspacePath + "/" + mMainNode.NodeAddress.ToString() + "/" + System.IO.Path.GetFileName(ofd.FileName);
                    FileStream fs = new FileStream(saveTo, FileMode.Create, FileAccess.ReadWrite);

                    ((EndpointNode)mMainNode).RequestFileTransfer(address, srcPath, fs);
                    SourceFileTextBlock.Text = saveTo;
                }
                else
                {
                    System.Windows.MessageBox.Show("Cannot download a file from the same node!");
                }
            }

            
        }

        private void SendRoutingProbeButton_Click(object sender, RoutedEventArgs e)
        {
            mMainNode.Send(new RoutingProbePacket(mRand.Next(0, 5000)));
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            mMainNode.Listen();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            mMainNode.StopListen();
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Clear the log
            StatusLogTextBox.Text = string.Empty;
        }
    }
}
