Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Navigation
Imports System.Threading.Tasks

Namespace AnalytiQ.UI
    Partial Public Class LoginPage
        Inherits Page

        ' Development mode flag
        Private Const IsDevelopmentMode As Boolean = True

        ' Shared fields for tenant tracking
        ' Modified to have only 3 pre-registered tenants
        Private Shared _registeredTenants As New List(Of String) From {"TENANT-001", "TENANT-003", "TENANT-005"} ' Pre-registered tenants
        Private Shared _currentTenantId As String = String.Empty

        ' Instance field for parent window
        Private _parentWindow As LoginWindow

        Public Sub New(parentWindow As LoginWindow)
            InitializeComponent()
            _parentWindow = parentWindow
            CheckAutoLogin()
            If IsDevelopmentMode Then
                btnSkipLogin.Visibility = Visibility.Visible
            End If
        End Sub

        ''' <summary>
        ''' Checks if user can auto-login based on existing Tenant ID
        ''' </summary>
        Private Sub CheckAutoLogin()
            If Not String.IsNullOrEmpty(_currentTenantId) AndAlso IsCompanyRegistered(_currentTenantId) Then
                NavigateToMainWindow(_currentTenantId)
            End If
        End Sub

        ''' <summary>
        ''' Handles Sign In button click
        ''' </summary>
        Private Async Sub SignIn_Click(sender As Object, e As RoutedEventArgs)
            errorContainer.Visibility = Visibility.Collapsed
            btnSignIn.IsEnabled = False
            loadingIndicator.Visibility = Visibility.Visible
            txtErrorMessage.Text = "Checking with Microsoft..."

            If String.IsNullOrWhiteSpace(txtEmail.Text) OrElse txtPassword.Password.Length = 0 Then
                ShowErrorMessage("Please enter both email and password.")
                loadingIndicator.Visibility = Visibility.Collapsed
                btnSignIn.IsEnabled = True
                Return
            End If

            Try
                Await Task.Delay(1500) ' Simulate Microsoft popup delay
                Dim tenantId = Await SimulateAzureLogin(txtEmail.Text, txtPassword.Password)
                If Not String.IsNullOrEmpty(tenantId) Then
                    _currentTenantId = tenantId
                    If IsCompanyRegistered(_currentTenantId) Then
                        NavigateToMainWindow(_currentTenantId)
                    Else
                        ShowCompanySetupWindow(_currentTenantId)
                    End If
                Else
                    ShowErrorMessage("Please use a valid work or school account.")
                End If
            Catch ex As Exception
                ShowErrorMessage($"Login failed: {ex.Message}")
            Finally
                loadingIndicator.Visibility = Visibility.Collapsed
                btnSignIn.IsEnabled = True
            End Try
        End Sub

        ''' <summary>
        ''' Simulates Azure AD authentication with test emails
        ''' </summary>
        Private Async Function SimulateAzureLogin(email As String, password As String) As Task(Of String)
            Await Task.Delay(500) ' Fake server response time
            Select Case email.ToLower()
                Case "bob@acmecorp.com"
                    If password = "Pass123" Then Return "TENANT-001"
                Case "alice@newco.com"
                    If password = "New456" Then Return "TENANT-002"
                Case "charlie@newco.com"
                    If password = "Charlie789" Then Return "TENANT-003"
                Case "diana@retailgiant.com"
                    If password = "Diana789" Then Return "TENANT-004"
                Case "ethan@techhub.io"
                    If password = "Ethan456" Then Return "TENANT-005"
                Case "frank@foodies.com"
                    If password = "Frank123" Then Return "TENANT-006"
                Case Else
                    If email.EndsWith("@gmail.com") OrElse email.EndsWith("@yahoo.com") Then
                        Return "" ' Block personal emails for "realism"
                    End If
            End Select
            Return "" ' Wrong creds or unknown email
        End Function


        ''' <summary>
        ''' Handles Skip Login button (dev only)
        ''' </summary>
        Private Sub SkipLogin_Click(sender As Object, e As RoutedEventArgs)
            _currentTenantId = "DEV-TENANT-999"
            If IsCompanyRegistered(_currentTenantId) Then
                NavigateToMainWindow(_currentTenantId)
            Else
                ShowCompanySetupWindow(_currentTenantId)
            End If
        End Sub

        ''' <summary>
        ''' Handles Forgot Password link
        ''' </summary>
        Private Sub ForgotPassword_Click(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Password recovery is not yet implemented.", "Information", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub

        ''' <summary>
        ''' Handles Contact Support link
        ''' </summary>
        Private Sub ContactSupport_Click(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Please contact support at support@analytiq.com or call 1-800-ANALYTQ.",
                          "Contact Support", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub

        ''' <summary>
        ''' Navigates to MainWindow
        ''' </summary>
        Private Sub NavigateToMainWindow(tenantId As String)
            Dim mainWindow As New MainWindow(tenantId)
            mainWindow.Show()
            _parentWindow.Close()
        End Sub

        ''' <summary>
        ''' Shows Company Setup Window
        ''' </summary>
        Private Sub ShowCompanySetupWindow(tenantId As String)
            Dim setupWindow As New CompanySetupWindow(tenantId, txtEmail.Text)
            setupWindow.Show()
            _parentWindow.Close()
        End Sub

        ''' <summary>
        ''' Displays error message
        ''' </summary>
        Private Sub ShowErrorMessage(message As String)
            txtErrorMessage.Text = message
            errorContainer.Visibility = Visibility.Visible
        End Sub

        ''' <summary>
        ''' Checks if a company is registered (shared for other classes)
        ''' </summary>
        Public Shared Function IsCompanyRegistered(tenantId As String) As Boolean
            Return _registeredTenants.Contains(tenantId)
        End Function

        ''' <summary>
        ''' Public property for current Tenant ID
        ''' </summary>
        Public Shared ReadOnly Property CurrentTenantId As String
            Get
                Return _currentTenantId
            End Get
        End Property

        ''' <summary>
        ''' Sets the current Tenant ID and registers it if new
        ''' </summary>
        Public Shared Sub SetTenantId(tenantId As String)
            _currentTenantId = tenantId
            If Not _registeredTenants.Contains(tenantId) Then
                _registeredTenants.Add(tenantId)
            End If
        End Sub
    End Class
End Namespace