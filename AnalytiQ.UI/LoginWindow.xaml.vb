Imports System.Windows

Namespace AnalytiQ.UI
    Public Class LoginWindow
        Inherits Window

        Public Sub New()
            InitializeComponent()
            LoginFrame.Navigate(New LoginPage(Me)) ' Pass the window reference to LoginPage
        End Sub
    End Class
End Namespace