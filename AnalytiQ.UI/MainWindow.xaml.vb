Imports System.Windows.Controls
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports Microsoft.Web.WebView2.Core
Imports System.Threading.Tasks
Imports System.Windows
Imports System.Windows.Input
Imports System.Net.Http
Imports Newtonsoft.Json
Imports AnalytiQ.UI.AnalytiQ.UI ' Keeping your namespace

Public Class MainWindow
    Private _isReportLoaded As Boolean = False
    Private _currentView As String = "Dashboard"
    Private _tenantId As String
    Private _fadeInAnimation As DoubleAnimation
    Private _fadeOutAnimation As DoubleAnimation
    Private _httpClient As New HttpClient()
    Private _reportId As String = "8ce30df4-68cb-4ae8-8af4-92cfbdcaef88"
    Private _functionUrl As String = "https://powerbiembedfunctionvb.azurewebsites.net/api/GetEmbedToken?code=R909Z-IgsOYddxeMd-YNxOTi7N_m-yzTepDLDrGmSpHaAzFuqpPO-w=="
    Private _embedUrl As String = String.Empty
    Private _embedToken As String = String.Empty
    Private _tokenExpiration As DateTime

    ' Default constructor
    Public Sub New()
        InitializeComponent()
        SetupWindow()
    End Sub

    ' Constructor with tenantId
    Public Sub New(tenantId As String)
        InitializeComponent()
        _tenantId = tenantId
        SetupWindow()
    End Sub

    ' Common setup logic
    Private Async Sub SetupWindow()
        ' Initialize animations
        _fadeInAnimation = New DoubleAnimation(0, 1, New Duration(TimeSpan.FromSeconds(0.25))) With {
            .EasingFunction = New CubicEase() With {.EasingMode = EasingMode.EaseOut}
        }
        _fadeOutAnimation = New DoubleAnimation(1, 0, New Duration(TimeSpan.FromSeconds(0.15))) With {
            .EasingFunction = New CubicEase() With {.EasingMode = EasingMode.EaseIn}
        }

        ' Set up WebView event handlers
        AddHandler PowerBIWebView.NavigationCompleted, AddressOf WebView_NavigationCompleted
        AddHandler PowerBIWebView.CoreWebView2InitializationCompleted, AddressOf WebView_CoreWebView2InitializationCompleted
        AddHandler PowerBIWebView.WebMessageReceived, AddressOf WebView_WebMessageReceived

        ' Check WebView2 Runtime
        If Not Await CheckWebView2Runtime() Then Return

        ' Fetch embed token and initialize report
        Await FetchEmbedTokenAsync()
        LoadReport() ' Default to Overview page on startup

        ' Update UI
        PageTitle.Text = "Dashboard"
        If _tenantId IsNot Nothing Then
            Title = $"AnalytiQ - Tenant: {_tenantId}"
        End If
        SyncButtonStates()
        LoadingContainer.Visibility = Visibility.Visible
        LoadingContainer.Visibility = Visibility.Collapsed

        ' Handle window state changes for maximize/restore icon
        AddHandler Me.StateChanged, AddressOf WindowStateChanged
        Me.WindowState = WindowState.Normal

        ' Enable DevTools for debugging
        If PowerBIWebView.CoreWebView2 IsNot Nothing Then
            PowerBIWebView.CoreWebView2.OpenDevToolsWindow()
        End If
    End Sub

    ' Check WebView2 Runtime availability
    Private Async Function CheckWebView2Runtime() As Task(Of Boolean)
        Try
            Dim version = CoreWebView2Environment.GetAvailableBrowserVersionString()
            Console.WriteLine($"WebView2 Runtime version: {version}")
            Return True
        Catch ex As Exception
            Console.WriteLine("WebView2 Runtime not found.")
            ShowErrorPage("WebView2 Runtime not found. Please install WebView2 Runtime.")
            Return False
        End Try
    End Function

    ' Fetch embed token from Azure Function with tenant filtering and RLS
    Private Async Function FetchEmbedTokenAsync() As Task
        Try
            If String.IsNullOrEmpty(_tenantId) Then
                Throw New Exception("Tenant ID is required for report filtering.")
            End If

            Dim useRls As Boolean = True ' Enable RLS by default
            Dim url As String = $"{_functionUrl}&reportId={_reportId}&tenantId={Uri.EscapeDataString(_tenantId)}&useRls={useRls.ToString().ToLower()}"
            Console.WriteLine($"Fetching embed token from: {url}")
            Dim response = Await _httpClient.GetStringAsync(url)
            Dim embedData = JsonConvert.DeserializeObject(Of EmbedTokenResponse)(response)

            _embedUrl = embedData.EmbedUrl
            _embedToken = embedData.EmbedToken
            _tokenExpiration = embedData.TokenExpiration

            Console.WriteLine($"Embed Token Fetched: {_embedToken}, Embed URL: {_embedUrl}, Expires: {_tokenExpiration}")
        Catch ex As Exception
            ShowErrorPage($"Failed to fetch embed token from Azure Function: {ex.Message}")
            Console.WriteLine($"Error fetching embed token: {ex.Message}")
        End Try
    End Function

    ' Load the Power BI report with specific page
    Private Async Sub LoadReport(Optional pageName As String = Nothing)
        ' If pageName is not provided, determine based on current view
        If pageName Is Nothing Then
            pageName = If(_currentView = "Dashboard", "Overview", "Deep Analytics")
        End If

        Try
            LoadingContainer.Visibility = Visibility.Visible
            LoadingContainer.Visibility = Visibility.Collapsed
            MainFrame.Visibility = Visibility.Collapsed
            PowerBIWebView.Visibility = Visibility.Visible

            ' Check if token is expired or not fetched
            If String.IsNullOrEmpty(_embedToken) OrElse _tokenExpiration < DateTime.UtcNow Then
                Console.WriteLine("Token expired or not fetched. Refreshing...")
                Await FetchEmbedTokenAsync()
            End If

            ' Ensure WebView2 is initialized
            Await PowerBIWebView.EnsureCoreWebView2Async(Nothing)
            If PowerBIWebView.CoreWebView2 Is Nothing Then
                ShowErrorPage("WebView2 Core initialization failed.")
                Console.WriteLine("WebView2 Core initialization failed.")
                Return
            End If

            ' Enable console logging for debugging
            PowerBIWebView.CoreWebView2.Settings.AreDevToolsEnabled = True

            ' Construct HTML with embedded report using Power BI JavaScript API
            Dim htmlContent As String = $"
            <html>
            <head>
                <title>Power BI Report</title>
                <script src='https://cdn.jsdelivr.net/npm/powerbi-client@2.18.6/dist/powerbi.min.js'></script>
                <style>
                    body {{ margin:0; padding:0; overflow:hidden; height:100vh; width:100vw; }}
                    #reportContainer {{ height:100%; width:100%; }}
                </style>
            </head>
            <body>
                <div id='reportContainer'></div>
                <script>
                    // Get Power BI models
                    const models = window['powerbi-client'].models;
                    
                    // Embed configuration
                    const config = {{
                        type: 'report',
                        tokenType: models.TokenType.Embed,
                        accessToken: '{_embedToken}',
                        embedUrl: '{_embedUrl}',
                        id: '{_reportId}',
                        permissions: models.Permissions.Read,
                        settings: {{
                            navContentPaneEnabled: true,
                            filterPaneEnabled: true
                        }}
                    }};
                    
                    // Embed the report
                    const reportContainer = document.getElementById('reportContainer');
                    const report = powerbi.embed(reportContainer, config);
                    
                    // Handle events
                    report.on('loaded', function() {{
                        console.log('Report loaded');
                        window.chrome.webview.postMessage('ReportLoaded');
                        
                        // Get all pages first to confirm the page exists
                        report.getPages()
                            .then(function(pages) {{
                                console.log('Available pages:', pages.map(p => p.name));
                                
                                // Find the page by name (case insensitive)
                                const targetPage = pages.find(p => 
                                    p.name.toLowerCase() === '{pageName.ToLower()}' || 
                                    p.displayName.toLowerCase() === '{pageName.ToLower()}'
                                );
                                
                                if (targetPage) {{
                                    return report.setPage(targetPage.name);
                                }} else {{
                                    console.error('Page not found: {pageName}');
                                    window.chrome.webview.postMessage('ReportError: Page not found - {pageName}');
                                    return Promise.reject('Page not found');
                                }}
                            }})
                            .then(function() {{
                                console.log('Successfully navigated to page: {pageName}');
                            }})
                            .catch(function(error) {{
                                console.error('Error navigating to page:', error);
                                window.chrome.webview.postMessage('ReportError: ' + error);
                            }});
                    }});
                    
                    report.on('error', function(event) {{
                        console.error('Error embedding report:', event.detail);
                        window.chrome.webview.postMessage('ReportError: ' + JSON.stringify(event.detail));
                    }});
                </script>
            </body>
            </html>"

            Console.WriteLine($"Navigating WebView2 to embed report on page: {pageName}...")
            PowerBIWebView.NavigateToString(htmlContent)

            ' Set a timeout to check if report loaded
            Await Task.Delay(10000) ' 10 seconds timeout
            If Not _isReportLoaded Then
                Console.WriteLine("Report loading timeout - trying to refresh")
                Await FetchEmbedTokenAsync()
                PowerBIWebView.Reload()
            End If
        Catch ex As Exception
            ShowErrorPage($"Error loading report: {ex.Message}")
            Console.WriteLine($"Error in LoadReport: {ex.Message}")
        End Try
    End Sub

    ' Navigate to a specific report page after loading
    Private Async Function NavigateToReportPage(pageName As String) As Task
        If PowerBIWebView.CoreWebView2 Is Nothing OrElse Not _isReportLoaded Then
            Console.WriteLine("Cannot navigate: WebView not initialized or report not loaded")
            Return
        End If

        Try
            ' Use JavaScript to navigate to the specific page with better error handling
            Dim script As String = $"
                const report = powerbi.get(document.getElementById('reportContainer'));
                if (!report) {{
                    console.error('Report not found in container');
                    window.chrome.webview.postMessage('ReportError: Report not found in container');
                    return;
                }}
                
                report.getPages()
                    .then(pages => {{
                        console.log('Available pages:', pages.map(p => p.name));
                        const targetPage = pages.find(p => 
                            p.name.toLowerCase() === '{pageName.ToLower()}' || 
                            p.displayName.toLowerCase() === '{pageName.ToLower()}'
                        );
                        
                        if (targetPage) {{
                            return report.setPage(targetPage.name);
                        }} else {{
                            throw new Error('Page not found: {pageName}');
                        }}
                    }})
                    .then(() => console.log('Page changed to: {pageName}'))
                    .catch(err => {{
                        console.error('Error changing page:', err);
                        window.chrome.webview.postMessage('ReportError: ' + err.message);
                    }});
            "

            Await PowerBIWebView.CoreWebView2.ExecuteScriptAsync(script)
            Console.WriteLine($"Executed script to navigate to page: {pageName}")
        Catch ex As Exception
            Console.WriteLine($"Error navigating to report page: {ex.Message}")
        End Try
    End Function

    ' List all available pages in the report for debugging
    Private Async Function ListReportPages() As Task
        If PowerBIWebView.CoreWebView2 Is Nothing OrElse Not _isReportLoaded Then
            Console.WriteLine("Cannot list pages: WebView not initialized or report not loaded")
            Return
        End If

        Try
            Dim script As String = "
                const report = powerbi.get(document.getElementById('reportContainer'));
                if (!report) {
                    console.error('Report not found');
                    return 'Report not found';
                }
                
                return report.getPages()
                    .then(pages => {
                        return JSON.stringify(pages.map(p => ({name: p.name, displayName: p.displayName})));
                    })
                    .catch(err => {
                        console.error('Error getting pages:', err);
                        return 'Error: ' + err.message;
                    });
            "

            Dim result = Await PowerBIWebView.CoreWebView2.ExecuteScriptAsync(script)
            Console.WriteLine($"Available report pages: {result}")
        Catch ex As Exception
            Console.WriteLine($"Error listing report pages: {ex.Message}")
        End Try
    End Function

    ' Handle messages from WebView2
    Private Sub WebView_WebMessageReceived(sender As Object, e As CoreWebView2WebMessageReceivedEventArgs)
        Dim message As String = e.TryGetWebMessageAsString()
        Console.WriteLine($"Message from WebView2: {message}")

        If message = "ReportLoaded" Then
            _isReportLoaded = True
            Dispatcher.Invoke(Async Sub()
                                  PowerBIWebView.Visibility = Visibility.Visible
                                  LoadingContainer.Visibility = Visibility.Visible
                                  LoadingContainer.Visibility = Visibility.Collapsed
                                  Console.WriteLine("Report loaded event received")

                                  ' List available pages to help with debugging
                                  Await ListReportPages()

                                  ' Now try to navigate to the desired page
                                  Await NavigateToReportPage(If(_currentView = "Dashboard", "Overview", "Deep Analytics"))
                              End Sub)
        ElseIf message.StartsWith("ReportError") OrElse message.StartsWith("ScriptError") Then
            Dispatcher.Invoke(Sub()
                                  ShowErrorPage(message)
                                  Console.WriteLine($"Error event received: {message}")
                              End Sub)
        End If
    End Sub

    ' Window dragging
    Private Sub WindowMouseDown(sender As Object, e As MouseButtonEventArgs)
        If e.ChangedButton = MouseButton.Left Then
            Me.DragMove()
        End If
    End Sub

    ' Update maximize/restore icon based on window state
    Private Sub WindowStateChanged(sender As Object, e As EventArgs)
        If WindowState = WindowState.Maximized Then
            MaximizeIcon.Text = "🗗" ' Restore icon
        Else
            MaximizeIcon.Text = "🗖" ' Maximize icon
        End If
    End Sub

    ' Window control button handlers
    Private Sub BtnMinimize_Click(sender As Object, e As RoutedEventArgs)
        Me.WindowState = WindowState.Minimized
    End Sub

    Private Sub BtnMaximize_Click(sender As Object, e As RoutedEventArgs)
        If Me.WindowState = WindowState.Maximized Then
            Me.WindowState = WindowState.Normal
        Else
            Me.WindowState = WindowState.Maximized
        End If
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub

    Private Sub WebView_CoreWebView2InitializationCompleted(sender As Object, e As CoreWebView2InitializationCompletedEventArgs)
        If Not e.IsSuccess Then
            ShowErrorPage("WebView2 initialization failed. Please ensure WebView2 Runtime is installed.")
            Console.WriteLine($"WebView2 initialization failed: {e.InitializationException}")
        Else
            Console.WriteLine("WebView2 initialized successfully.")
        End If
    End Sub

    Private Sub WebView_NavigationCompleted(sender As Object, e As CoreWebView2NavigationCompletedEventArgs)
        If Not e.IsSuccess Then
            ShowErrorPage($"Failed to load report: {e.WebErrorStatus}")
            Console.WriteLine($"Navigation failed: {e.WebErrorStatus}")
            Return
        End If
        Console.WriteLine("Navigation completed successfully.")
    End Sub

    Private Sub Navigation_Click(sender As Object, e As RoutedEventArgs)
        Dim clickedButton As Button = DirectCast(sender, Button)
        Dim buttonName As String = clickedButton.Name
        Dim newView As String = String.Empty
        Dim pageName As String = Nothing

        ' Return if clicking the already selected button
        If (buttonName.Contains("Dashboard") AndAlso _currentView = "Dashboard") OrElse
           (buttonName.Contains("Analytics") AndAlso _currentView = "Analytics") OrElse
           (buttonName.Contains("DataUpload") AndAlso _currentView = "DataUpload") OrElse
           (buttonName.Contains("Settings") AndAlso _currentView = "Settings") OrElse
           (buttonName.Contains("Admin") AndAlso _currentView = "Admin") Then
            Return
        End If

        ResetAllButtons()

        If buttonName.Contains("Dashboard") Then
            newView = "Dashboard"
            pageName = "Overview"
            PageTitle.Text = "Dashboard"
            SelectButton(btnDashboard)
        ElseIf buttonName.Contains("Analytics") Then
            newView = "Analytics"
            pageName = "Deep Analytics"
            PageTitle.Text = "Analytics"
            SelectButton(btnAnalytics)
        ElseIf buttonName.Contains("DataUpload") Then
            newView = "DataUpload"
            LoadFramePage(New DataUploadPage(), "Data Upload")
            SelectButton(btnDataUpload)
        ElseIf buttonName.Contains("Settings") Then
            newView = "Settings"
            LoadFramePage(New SettingsPage(), "Settings")
            SelectButton(btnSettings)
        ElseIf buttonName.Contains("Admin") Then
            newView = "Admin"
            LoadFramePage(New AdminPage(), "Admin")
            SelectButton(btnAdmin)
        End If

        _currentView = newView

        ' Load the report with the specific page if Dashboard or Analytics
        If newView = "Dashboard" OrElse newView = "Analytics" Then
            LoadReport(pageName)
        End If

        AnimatePageTransition()
    End Sub

    Private Sub LoadFramePage(page As Page, title As String)
        LoadingContainer.Visibility = Visibility.Visible
        LoadingContainer.Visibility = Visibility.Collapsed
        PageTitle.Text = title
        MainFrame.Navigate(page)
        MainFrame.Visibility = Visibility.Visible
        PowerBIWebView.Visibility = Visibility.Collapsed
    End Sub

    Private Sub ResetAllButtons()
        btnDashboard.Tag = ""
        btnAnalytics.Tag = ""
        btnDataUpload.Tag = ""
        btnSettings.Tag = ""
        btnAdmin.Tag = ""
    End Sub

    Private Sub SelectButton(button As Button)
        button.Tag = "Selected"
    End Sub

    Private Sub SyncButtonStates()
        ResetAllButtons()
        SelectButton(If(_currentView = "Dashboard", btnDashboard,
                       If(_currentView = "Analytics", btnAnalytics,
                          If(_currentView = "DataUpload", btnDataUpload,
                             If(_currentView = "Settings", btnSettings, btnAdmin)))))
    End Sub

    Private Sub AnimatePageTransition()
        Dim target As FrameworkElement = If(PowerBIWebView.Visibility = Visibility.Visible, PowerBIWebView, MainFrame)
        Dim parentBorder = target.ParentOfType(Of Border)()
        If parentBorder IsNot Nothing Then
            parentBorder.BeginAnimation(OpacityProperty, _fadeInAnimation)
        End If
    End Sub

    Private Sub ShowErrorPage(errorMessage As String)
        LoadingContainer.Visibility = Visibility.Visible
        LoadingContainer.Visibility = Visibility.Collapsed
        MainFrame.Visibility = Visibility.Visible
        PowerBIWebView.Visibility = Visibility.Collapsed
        MainFrame.Navigate(New ErrorPage(errorMessage))
    End Sub

    Public Class ErrorPage
        Inherits Page

        Public Sub New(errorMessage As String)
            Dim errorPanel As New StackPanel()
            errorPanel.VerticalAlignment = VerticalAlignment.Center
            errorPanel.HorizontalAlignment = HorizontalAlignment.Center

            Dim errorIcon As New TextBlock()
            errorIcon.Text = "⚠️"
            errorIcon.FontSize = 48
            errorIcon.HorizontalAlignment = HorizontalAlignment.Center
            errorIcon.Margin = New Thickness(0, 0, 0, 20)

            Dim errorTitle As New TextBlock()
            errorTitle.Text = "Error Loading Content"
            errorTitle.FontSize = 24
            errorTitle.FontWeight = FontWeights.Bold
            errorTitle.HorizontalAlignment = HorizontalAlignment.Center
            errorTitle.Margin = New Thickness(0, 0, 0, 10)

            Dim errorText As New TextBlock()
            errorText.Text = errorMessage
            errorText.TextWrapping = TextWrapping.Wrap
            errorText.HorizontalAlignment = HorizontalAlignment.Center
            errorText.Margin = New Thickness(0, 0, 0, 20)

            Dim retryButton As New Button()
            retryButton.Content = "Retry"
            retryButton.Padding = New Thickness(15, 8, 15, 8)
            retryButton.HorizontalAlignment = HorizontalAlignment.Center
            AddHandler retryButton.Click, AddressOf Retry_Click

            errorPanel.Children.Add(errorIcon)
            errorPanel.Children.Add(errorTitle)
            errorPanel.Children.Add(errorText)
            errorPanel.Children.Add(retryButton)

            Me.Content = errorPanel
        End Sub

        Private Sub Retry_Click(sender As Object, e As RoutedEventArgs)
            If MainWindow.Instance._currentView = "Dashboard" Then
                MainWindow.Instance.LoadReport("Overview")
            ElseIf MainWindow.Instance._currentView = "Analytics" Then
                MainWindow.Instance.LoadReport("Deep Analytics")
            ElseIf MainWindow.Instance._currentView = "DataUpload" Then
                MainWindow.Instance.LoadFramePage(New DataUploadPage(), "Data Upload")
            ElseIf MainWindow.Instance._currentView = "Settings" Then
                MainWindow.Instance.LoadFramePage(New SettingsPage(), "Settings")
            ElseIf MainWindow.Instance._currentView = "Admin" Then
                MainWindow.Instance.LoadFramePage(New AdminPage(), "Admin")
            End If
        End Sub
    End Class

    Public Shared ReadOnly Property Instance As MainWindow
        Get
            Return DirectCast(Application.Current.MainWindow, MainWindow)
        End Get
    End Property

    ' Class to deserialize embed token response
    Private Class EmbedTokenResponse
        Public Property EmbedUrl As String
        Public Property EmbedToken As String
        Public Property TokenExpiration As DateTime
    End Class
End Class

Module VisualTreeExtensions
    <Runtime.CompilerServices.Extension()>
    Public Function ParentOfType(Of T As DependencyObject)(element As DependencyObject) As T
        Dim parent As DependencyObject = VisualTreeHelper.GetParent(element)
        If parent Is Nothing Then Return Nothing
        If TypeOf parent Is T Then Return DirectCast(parent, T)
        Return parent.ParentOfType(Of T)()
    End Function
End Module