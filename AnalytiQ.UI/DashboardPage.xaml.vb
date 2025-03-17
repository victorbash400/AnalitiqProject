Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Threading
Imports Microsoft.Web.WebView2.Wpf

Public Class DashboardPage
    Inherits Page

    Public Sub New()
        InitializeComponent()
        AddHandler Me.Loaded, AddressOf OnPageLoaded
    End Sub

    Private Sub OnPageLoaded(sender As Object, e As RoutedEventArgs)
        ' Ensure WebView2 is initialized asynchronously
        OverviewWebView2.EnsureCoreWebView2Async(Nothing).ContinueWith(Sub(task)
                                                                           If task.IsFaulted Then
                                                                               ' Use Dispatcher to show error message on UI thread
                                                                               Dispatcher.Invoke(Sub()
                                                                                                     MessageBox.Show("WebView2 failed to initialize: " &
                                                                                                                     task.Exception.Message,
                                                                                                                     "Initialization Error",
                                                                                                                     MessageBoxButton.OK,
                                                                                                                     MessageBoxImage.Error)
                                                                                                 End Sub)
                                                                           Else
                                                                               ' Use Dispatcher to set the source on the UI thread
                                                                               Dispatcher.Invoke(Sub()
                                                                                                     OverviewWebView2.Source = New Uri("https://www.google.com")
                                                                                                 End Sub)
                                                                           End If
                                                                       End Sub)
    End Sub
End Class