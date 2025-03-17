Imports System.Windows
Imports System.Windows.Controls
Imports Microsoft.Data.SqlClient
Imports System.Collections.ObjectModel
Imports System.Threading.Tasks

Namespace AnalytiQ.UI
    Partial Public Class SettingsPage
        Inherits Page

        ' Private fields
        Private _currentTenantId As String
        Private _productList As ObservableCollection(Of String)
        Private lblStatus As Label
        Private mainStackPanel As StackPanel

        ' Constructor
        Public Sub New()
            InitializeComponent()
            _productList = New ObservableCollection(Of String)()
            lstProducts.ItemsSource = _productList
            AddEventHandlers()

            ' Initialize status label
            lblStatus = New Label() With {
                .Foreground = New SolidColorBrush(Colors.Blue),
                .FontWeight = FontWeights.Bold,
                .FontSize = 13,
                .Margin = New Thickness(0, 8, 0, 8)
            }

            ' Add status label to the main StackPanel
            mainStackPanel = TryCast(TryCast(FindName("MainScrollViewer"), ScrollViewer)?.Content, StackPanel)
            If mainStackPanel IsNot Nothing Then
                mainStackPanel.Children.Insert(1, lblStatus)
            Else
                ShowError("Could not find main StackPanel to add status label.", New Exception("UI initialization failed"))
            End If

            ' Load settings on startup
            LoadSettingsAsync()
        End Sub

        ' Set up event handlers for buttons
        Private Sub AddEventHandlers()
            AddHandler btnAddProduct.Click, AddressOf AddProductAsync
            AddHandler btnRefreshTenantInfo.Click, AddressOf RefreshTenantInfoAsync
        End Sub

        ' Load settings asynchronously
        Private Async Sub LoadSettingsAsync()
            Try
                ShowStatusMessage("Initializing settings...")

                _currentTenantId = GetCurrentTenantId()
                If String.IsNullOrEmpty(_currentTenantId) Then
                    lblAuthWarning.Visibility = Visibility.Visible
                    pnlTenantSettings.IsEnabled = False
                    ShowStatusMessage("")
                    Return
                End If

                Await PopulateTenantInformationAsync()
                Await PopulateProductListAsync()
                ShowStatusMessage("Settings loaded successfully.")
            Catch ex As Exception
                ShowError("Error loading settings", ex)
                ShowStatusMessage("Error loading settings.")
            End Try
        End Sub

        ' Get the current tenant ID (placeholder implementation)
        Private Function GetCurrentTenantId() As String
            Try
                Dim tenantId As String = LoginPage.CurrentTenantId ' Assumes LoginPage has this property
                If String.IsNullOrEmpty(tenantId) Then
                    Return "DEV-TENANT-999" ' Default for testing
                End If
                Return tenantId
            Catch ex As Exception
                ShowError("Error getting tenant ID", ex)
                Return String.Empty
            End Try
        End Function

        ' Populate tenant information from the database
        Private Async Function PopulateTenantInformationAsync() As Task
            Try
                ShowStatusMessage("Loading tenant information...")
                txtTenantId.Text = _currentTenantId
                Dim connectionString As String = GetConnectionString()
                If String.IsNullOrEmpty(connectionString) Then
                    Return
                End If

                Await ExecuteWithRetryAsync(
                    Async Function()
                        Using conn As New SqlConnection(connectionString)
                            Await conn.OpenAsync()
                            Dim query As String = "SELECT CompanyName, AdminEmail FROM Tenants WHERE TenantID = @TenantID"
                            Using cmd As New SqlCommand(query, conn)
                                cmd.Parameters.AddWithValue("@TenantID", _currentTenantId)
                                Using reader = Await cmd.ExecuteReaderAsync()
                                    If Await reader.ReadAsync() Then
                                        Await Application.Current.Dispatcher.InvokeAsync(
                                            Sub()
                                                txtTenantName.Text = reader.GetString(0)
                                                txtAdminEmail.Text = reader.GetString(1)
                                            End Sub)
                                    End If
                                End Using
                            End Using
                        End Using
                        Return True
                    End Function,
                    "Loading tenant information")

            Catch ex As Exception
                ShowError("Error populating tenant information", ex)
            End Try
        End Function

        ' Populate the product list from the database
        Private Async Function PopulateProductListAsync() As Task
            Try
                ShowStatusMessage("Loading product list...")

                Await Application.Current.Dispatcher.InvokeAsync(
                    Sub()
                        _productList.Clear()
                    End Sub)

                Dim connectionString As String = GetConnectionString()
                If String.IsNullOrEmpty(connectionString) Then
                    Return
                End If

                Dim tempProductList As New List(Of String)()

                Await ExecuteWithRetryAsync(
                    Async Function()
                        Using conn As New SqlConnection(connectionString)
                            Await conn.OpenAsync()
                            Dim query As String = "SELECT ProductName FROM OrganizationProducts WHERE TenantID = @TenantID"
                            Using cmd As New SqlCommand(query, conn)
                                cmd.Parameters.AddWithValue("@TenantID", _currentTenantId)
                                Using reader = Await cmd.ExecuteReaderAsync()
                                    While Await reader.ReadAsync()
                                        tempProductList.Add(reader.GetString(0))
                                    End While
                                End Using
                            End Using
                        End Using

                        Await Application.Current.Dispatcher.InvokeAsync(
                            Sub()
                                For Each product In tempProductList
                                    _productList.Add(product)
                                Next
                            End Sub)

                        Return True
                    End Function,
                    "Loading product list")

            Catch ex As Exception
                ShowError("Error loading product list", ex)
            End Try
        End Function

        ' Add a new product
        Private Async Sub AddProductAsync(sender As Object, e As RoutedEventArgs)
            Try
                Dim newProduct As String = txtNewProduct.Text.Trim()
                If String.IsNullOrEmpty(newProduct) Then
                    MessageBox.Show("Please enter a product name.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return
                End If

                ShowStatusMessage("Adding product...")
                btnAddProduct.IsEnabled = False

                Dim connectionString As String = GetConnectionString()
                If String.IsNullOrEmpty(connectionString) Then
                    btnAddProduct.IsEnabled = True
                    Return
                End If

                Dim success As Boolean = Await ExecuteWithRetryAsync(
                    Async Function()
                        Using conn As New SqlConnection(connectionString)
                            Await conn.OpenAsync()

                            ' Check for duplicates
                            Dim checkQuery As String = "SELECT COUNT(*) FROM OrganizationProducts WHERE TenantID = @TenantID AND ProductName = @ProductName"
                            Using checkCmd As New SqlCommand(checkQuery, conn)
                                checkCmd.Parameters.AddWithValue("@TenantID", _currentTenantId)
                                checkCmd.Parameters.AddWithValue("@ProductName", newProduct)
                                Dim count As Integer = Convert.ToInt32(Await checkCmd.ExecuteScalarAsync())
                                If count > 0 Then
                                    Await Application.Current.Dispatcher.InvokeAsync(
                                        Sub()
                                            MessageBox.Show("Product already exists.", "Duplicate Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                                        End Sub)
                                    Return False
                                End If
                            End Using

                            ' Insert new product
                            Dim insertQuery As String = "INSERT INTO OrganizationProducts (TenantID, ProductName, CreatedDate) VALUES (@TenantID, @ProductName, GETUTCDATE())"
                            Using cmd As New SqlCommand(insertQuery, conn)
                                cmd.Parameters.AddWithValue("@TenantID", _currentTenantId)
                                cmd.Parameters.AddWithValue("@ProductName", newProduct)
                                Await cmd.ExecuteNonQueryAsync()
                            End Using
                        End Using
                        Return True
                    End Function,
                    "Adding product")

                If success Then
                    Await PopulateProductListAsync()
                    txtNewProduct.Text = ""
                    MessageBox.Show("Product added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                End If

            Catch ex As Exception
                ShowError("Error adding product", ex)
            Finally
                btnAddProduct.IsEnabled = True
                ShowStatusMessage("")
            End Try
        End Sub

        ' Handle remove product button click
        Private Sub RemoveProduct_Click(sender As Object, e As RoutedEventArgs)
            Dim button As Button = TryCast(sender, Button)
            If button IsNot Nothing Then
                Dim productName As String = TryCast(button.Tag, String)
                If Not String.IsNullOrEmpty(productName) Then
                    RemoveProductAsync(productName)
                End If
            End If
        End Sub

        ' Remove a product asynchronously
        Private Async Sub RemoveProductAsync(productName As String)
            Try
                If String.IsNullOrEmpty(productName) Then
                    MessageBox.Show("No product specified.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return
                End If

                Dim result As MessageBoxResult = MessageBox.Show(
                    $"Are you sure you want to remove the product: {productName}?",
                    "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Question)
                If result <> MessageBoxResult.Yes Then Return

                ShowStatusMessage("Removing product...")

                Dim connectionString As String = GetConnectionString()
                If String.IsNullOrEmpty(connectionString) Then
                    Return
                End If

                Dim success As Boolean = Await ExecuteWithRetryAsync(
                    Async Function()
                        Using conn As New SqlConnection(connectionString)
                            Await conn.OpenAsync()
                            Dim query As String = "DELETE FROM OrganizationProducts WHERE TenantID = @TenantID AND ProductName = @ProductName"
                            Using cmd As New SqlCommand(query, conn)
                                cmd.Parameters.AddWithValue("@TenantID", _currentTenantId)
                                cmd.Parameters.AddWithValue("@ProductName", productName)
                                Dim rowsAffected As Integer = Await cmd.ExecuteNonQueryAsync()
                                If rowsAffected = 0 Then
                                    Await Application.Current.Dispatcher.InvokeAsync(
                                        Sub()
                                            MessageBox.Show("Product not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                                        End Sub)
                                    Return False
                                End If
                            End Using
                        End Using
                        Return True
                    End Function,
                    "Removing product")

                If success Then
                    Await PopulateProductListAsync()
                    MessageBox.Show("Product removed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                End If
            Catch ex As Exception
                ShowError("Error removing product", ex)
            Finally
                ShowStatusMessage("")
            End Try
        End Sub

        ' Refresh tenant information
        Private Async Sub RefreshTenantInfoAsync(sender As Object, e As RoutedEventArgs)
            Try
                btnRefreshTenantInfo.IsEnabled = False
                ShowStatusMessage("Refreshing tenant information...")

                Await PopulateTenantInformationAsync()

                MessageBox.Show("Tenant information refreshed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
            Catch ex As Exception
                ShowError("Error refreshing tenant information", ex)
            Finally
                btnRefreshTenantInfo.IsEnabled = True
                ShowStatusMessage("")
            End Try
        End Sub

        ' Execute database operations with retry logic
        Private Async Function ExecuteWithRetryAsync(Of T)(operation As Func(Of Task(Of T)), operationName As String) As Task(Of T)
            Dim maxRetries As Integer = 3
            Dim retryDelay As Integer = 2000 ' milliseconds
            Dim retryCount As Integer = 0

            While retryCount < maxRetries
                Try
                    retryCount += 1
                    ShowStatusMessage($"{operationName}... (Attempt {retryCount}/{maxRetries})")
                    Return Await operation()
                Catch ex As SqlException
                    Dim isTransient As Boolean = (ex.Number = -2 OrElse ex.Number = 53 OrElse
                                                  ex.Number = 258 OrElse ex.Number = 4060 OrElse
                                                  ex.Number = -1 OrElse ex.Number = 10060 OrElse
                                                  ex.Message.Contains("timeout") OrElse
                                                  ex.Message.Contains("not currently available"))

                    If Not isTransient OrElse retryCount >= maxRetries Then
                        Throw New Exception($"Failed to {operationName.ToLower()} after {maxRetries} attempts", ex)
                    End If

                    ShowStatusMessage($"Connection attempt {retryCount} failed. Retrying in {retryDelay / 1000} seconds...")
                End Try

                Await Task.Delay(retryDelay)
                retryDelay *= 2 ' Exponential backoff
            End While

            Throw New Exception($"Unexpected error during {operationName.ToLower()}")
        End Function

        ' Get database connection string (hardcoded for simplicity)
        Private Function GetConnectionString() As String
            Return "Server=tcp:analytiq-sql-299.database.windows.net,1433;Initial Catalog=AnalytiQDB;Persist Security Info=False;User ID=victorbash;Password=Director48@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
        End Function

        ' Show error messages
        Private Sub ShowError(message As String, ex As Exception)
            Application.Current.Dispatcher.Invoke(
                Sub()
                    MessageBox.Show($"{message}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Sub)
        End Sub

        ' Update status message on UI
        Private Sub ShowStatusMessage(message As String)
            Application.Current.Dispatcher.Invoke(
                Sub()
                    If lblStatus IsNot Nothing Then
                        lblStatus.Content = message
                    End If
                End Sub)
        End Sub
    End Class
End Namespace