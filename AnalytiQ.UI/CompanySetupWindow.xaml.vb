Imports System.Windows
Imports System.Windows.Controls

Namespace AnalytiQ.UI
    Public Class CompanySetupWindow
        Inherits Window

        Private _tenantId As String
        Private _adminEmail As String

        Public Sub New(tenantId As String, adminEmail As String)
            InitializeComponent()
            _tenantId = tenantId
            _adminEmail = adminEmail
        End Sub

        Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
            txtTenantId.Text = _tenantId
            txtAdminEmail.Text = _adminEmail
            txtCompanyName.Text = ""

            If LoginPage.IsCompanyRegistered(_tenantId) Then
                NavigateToDashboard()
                Return
            End If

            cboIndustry.SelectedIndex = 0
            ValidateForm(Nothing, Nothing)

            ' Skip button is always visible
            btnSkipRegistration.Visibility = Visibility.Visible
        End Sub

        Private Sub ShowErrorMessage(message As String)
            txtErrorMessage.Text = message
            errorContainer.Visibility = Visibility.Visible
        End Sub

        Private Sub ValidateForm(sender As Object, e As RoutedEventArgs)
            Dim isValid As Boolean = True
            errorContainer.Visibility = Visibility.Collapsed

            If String.IsNullOrWhiteSpace(txtCompanyName.Text) Then
                isValid = False
                ShowErrorMessage("Company Name is required.")
            ElseIf cboIndustry.SelectedItem Is Nothing Then
                isValid = False
                ShowErrorMessage("Please select an industry.")
            ElseIf Not chkConfirm.IsChecked.GetValueOrDefault(False) Then
                isValid = False
                ShowErrorMessage("Please confirm you are an authorized representative.")
            End If

            btnRegister.IsEnabled = isValid
        End Sub

        Private Sub Register_Click(sender As Object, e As RoutedEventArgs)
            Try
                If LoginPage.IsCompanyRegistered(_tenantId) Then
                    ShowErrorMessage("This company is already registered.")
                    NavigateToDashboard()
                    Return
                End If

                Dim companyInfo As New CompanyInfo With {
                    .TenantId = _tenantId,
                    .CompanyName = txtCompanyName.Text,
                    .Industry = DirectCast(cboIndustry.SelectedItem, ComboBoxItem).Content.ToString(),
                    .AdminEmail = _adminEmail
                }

                SaveCompanyDetails(companyInfo)
                LoginPage.SetTenantId(_tenantId)
                NavigateToDashboard()
            Catch ex As Exception
                ShowErrorMessage($"Error registering company: {ex.Message}")
            End Try
        End Sub

        Private Sub SkipRegistration_Click(sender As Object, e As RoutedEventArgs)
            LoginPage.SetTenantId(_tenantId)
            NavigateToDashboard()
        End Sub

        Private Sub SaveCompanyDetails(companyInfo As CompanyInfo)
            System.Diagnostics.Debug.WriteLine($"Registered: {companyInfo.TenantId}, {companyInfo.CompanyName}")
            ' Actual database save code would go here
        End Sub

        Private Sub NavigateToDashboard()
            Dim mainWindow As New MainWindow(_tenantId)
            mainWindow.Show()
            Me.Close()
        End Sub
    End Class

    Public Class CompanyInfo
        Public Property TenantId As String
        Public Property CompanyName As String
        Public Property Industry As String
        Public Property AdminEmail As String
    End Class
End Namespace