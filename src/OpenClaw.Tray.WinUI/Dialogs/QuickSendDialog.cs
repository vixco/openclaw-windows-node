using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using OpenClaw.Shared;
using OpenClawTray.Helpers;
using OpenClawTray.Services;
using System;
using System.Threading.Tasks;
using WinUIEx;

namespace OpenClawTray.Dialogs;

/// <summary>
/// Quick send dialog for sending messages to OpenClaw.
/// </summary>
public sealed class QuickSendDialog : WindowEx
{
    private readonly OpenClawGatewayClient _client;
    private readonly TextBox _messageTextBox;
    private readonly Button _sendButton;
    private readonly TextBlock _statusText;
    private bool _isSending;

    public QuickSendDialog(OpenClawGatewayClient client, string? prefillMessage = null)
    {
        _client = client;
        
        // Window setup
        Title = "Settings — OpenClaw Tray";
        this.SetWindowSize(400, 200);
        this.CenterOnScreen();
        this.SetIcon(IconHelper.GetStatusIconPath(ConnectionStatus.Connected));
        
        // Apply Acrylic backdrop for transient dialog feel
        SystemBackdrop = new DesktopAcrylicBackdrop();
        
        // Build UI programmatically (simple dialog)
        var root = new StackPanel
        {
            Spacing = 12,
            Padding = new Thickness(24)
        };

        var header = new TextBlock
        {
            Text = "📤 Quick Send",
            Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"]
        };
        root.Children.Add(header);

        _messageTextBox = new TextBox
        {
            PlaceholderText = "Type your message...",
            AcceptsReturn = false,
            Text = prefillMessage ?? ""
        };
        _messageTextBox.KeyDown += OnKeyDown;
        root.Children.Add(_messageTextBox);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        _statusText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0)
        };
        buttonPanel.Children.Add(_statusText);

        var cancelButton = new Button { Content = "Cancel" };
        cancelButton.Click += (s, e) => Close();
        buttonPanel.Children.Add(cancelButton);

        _sendButton = new Button
        {
            Content = "Send",
            Style = (Style)Application.Current.Resources["AccentButtonStyle"]
        };
        _sendButton.Click += OnSendClick;
        buttonPanel.Children.Add(_sendButton);

        root.Children.Add(buttonPanel);

        Content = root;

        // Focus the text box when shown
        Activated += (s, e) => _messageTextBox.Focus(FocusState.Programmatic);
    }

    private async void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == global::Windows.System.VirtualKey.Enter && !_isSending)
        {
            e.Handled = true;
            await SendMessageAsync();
        }
        else if (e.Key == global::Windows.System.VirtualKey.Escape)
        {
            Close();
        }
    }

    private async void OnSendClick(object sender, RoutedEventArgs e)
    {
        await SendMessageAsync();
    }

    private async Task SendMessageAsync()
    {
        var message = _messageTextBox.Text?.Trim();
        if (string.IsNullOrEmpty(message)) return;

        _isSending = true;
        _sendButton.IsEnabled = false;
        _messageTextBox.IsEnabled = false;
        _statusText.Text = "Sending...";

        try
        {
            await _client.SendChatMessageAsync(message);
            Logger.Info($"Quick send: {message}");
            new ToastContentBuilder()
                .AddText("Message Sent")
                .AddText("Your message was sent to OpenClaw.")
                .Show();
            Close();
        }
        catch (Exception ex)
        {
            Logger.Error($"Quick send failed: {ex.Message}");
            _statusText.Text = "❌ Failed";
            _sendButton.IsEnabled = true;
            _messageTextBox.IsEnabled = true;
            _isSending = false;
        }
    }

    public new void ShowAsync()
    {
        Activate();
    }
}
