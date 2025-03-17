Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.IO
Imports System.Threading.Tasks
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports Microsoft.Win32
Imports Microsoft.Data.SqlClient

Namespace AnalytiQ.UI
    Public Class DataUploadPage
        Inherits Page
        Implements INotifyPropertyChanged

        ' Fields
        Private _recentUploads As ObservableCollection(Of UploadItem)
        Private _autoCategorize As Boolean = True
        Private _extractSentiments As Boolean = True
        Private _generateSummary As Boolean = False
        Private _selectedDocumentType As String = "Customer Feedback"
        Private _selectedFiles As New List(Of String)
        Private _currentTenantId As String
        Private _selectedProduct As String
        Private _isLoading As Boolean = False
        Private _loadingMessage As String = "Loading..."
        Private _uploadProgress As Integer = 0
        Private _uploadingFileCount As Integer = 0
        Private _totalFilesToUpload As Integer = 0

        ' INotifyPropertyChanged Implementation
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Protected Sub OnPropertyChanged(propertyName As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub

        ' Properties for UI Binding
        Public Property IsLoading As Boolean
            Get
                Return _isLoading
            End Get
            Set(value As Boolean)
                _isLoading = value
                OnPropertyChanged("IsLoading")
            End Set
        End Property

        Public Property LoadingMessage As String
            Get
                Return _loadingMessage
            End Get
            Set(value As String)
                _loadingMessage = value
                OnPropertyChanged("LoadingMessage")
            End Set
        End Property

        Public Property UploadProgress As Integer
            Get
                Return _uploadProgress
            End Get
            Set(value As Integer)
                _uploadProgress = value
                OnPropertyChanged("UploadProgress")
            End Set
        End Property

        ' Constructor
        Public Sub New()
            InitializeComponent()
            Me.DataContext = Me
            _currentTenantId = LoginPage.CurrentTenantId ' Assume this is set from LoginPage
            _recentUploads = New ObservableCollection(Of UploadItem)()
            PopulateSampleUploads()
            Me.UploadsListView.ItemsSource = _recentUploads
            SetupEventHandlers()
            LoadPageDataAsync()
        End Sub

        ' Load Page Data Asynchronously
        Private Async Sub LoadPageDataAsync()
            Try
                IsLoading = True
                LoadingMessage = "Checking tenant access..."
                Await Task.Delay(100) ' Brief delay for UI feedback

                If Not CheckTenantAccess() Then
                    Return
                End If

                LoadingMessage = "Loading products..."
                Await LoadProductsAsync()
                UpdateUploadButtonState()
            Catch ex As Exception
                ShowError("Error initializing page: " & ex.Message)
            Finally
                IsLoading = False
            End Try
        End Sub

        ' Check Tenant Access
        Private Function CheckTenantAccess() As Boolean
            If String.IsNullOrEmpty(_currentTenantId) Then
                Application.Current.Dispatcher.Invoke(Sub()
                                                          MessageBox.Show("No tenant selected. Please log in again.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error)
                                                          NavigationService.Navigate(New Uri("pack://application:,,,/UI/LoginPage.xaml"))
                                                      End Sub)
                Return False
            Else
                txtTenantInfo.Text = $"Tenant: {GetTenantName(_currentTenantId)}"
                Return True
            End If
        End Function

        ' Load Products Asynchronously
        Private Async Function LoadProductsAsync() As Task
            Try
                Dim products As List(Of String) = Await Task.Run(Function()
                                                                     Dim connectionString As String = "Server=tcp:analytiq-sql-299.database.windows.net,1433;Initial Catalog=AnalytiQDB;Persist Security Info=False;User ID=victorbash;Password=Director48@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
                                                                     Using conn As New SqlConnection(connectionString)
                                                                         conn.Open()
                                                                         Dim query As String = "SELECT ProductName FROM OrganizationProducts WHERE TenantID = @TenantID"
                                                                         Using cmd As New SqlCommand(query, conn)
                                                                             cmd.Parameters.AddWithValue("@TenantID", _currentTenantId)
                                                                             Using reader As SqlDataReader = cmd.ExecuteReader()
                                                                                 Dim productList As New List(Of String)
                                                                                 While reader.Read()
                                                                                     productList.Add(reader.GetString(0))
                                                                                 End While
                                                                                 Return productList
                                                                             End Using
                                                                         End Using
                                                                     End Using
                                                                 End Function)

                ProductComboBox.Items.Clear()
                For Each product In products
                    ProductComboBox.Items.Add(New ComboBoxItem() With {
                        .Content = product,
                        .Foreground = Brushes.White
                    })
                Next

                If ProductComboBox.Items.Count > 0 Then
                    ProductComboBox.SelectedIndex = 0
                    _selectedProduct = CType(ProductComboBox.SelectedItem, ComboBoxItem).Content.ToString()
                Else
                    ShowWarning("No products found for your store. Add some in Settings first!")
                    UploadButton.IsEnabled = False
                End If
            Catch ex As Exception
                ShowError("Error loading products: " & ex.Message)
                UploadButton.IsEnabled = False
            End Try
        End Function

        ' Event Handlers Setup
        Private Sub SetupEventHandlers()
            AddHandler Me.UploadArea.Drop, AddressOf UploadArea_Drop
            AddHandler Me.UploadArea.DragOver, AddressOf UploadArea_DragOver
            AddHandler Me.SelectFilesButton.Click, AddressOf SelectFilesButton_Click
            AddHandler Me.UploadButton.Click, AddressOf UploadButton_Click
            AddHandler Me.ClearButton.Click, AddressOf ClearButton_Click
            AddHandler Me.ViewAllButton.Click, AddressOf ViewAllButton_Click
            AddHandler Me.DocumentTypeComboBox.SelectionChanged, AddressOf DocumentTypeComboBox_SelectionChanged
            AddHandler Me.AutoCategorizeCheckBox.Checked, AddressOf ProcessingOption_Changed
            AddHandler Me.AutoCategorizeCheckBox.Unchecked, AddressOf ProcessingOption_Changed
            AddHandler Me.ExtractSentimentsCheckBox.Checked, AddressOf ProcessingOption_Changed
            AddHandler Me.ExtractSentimentsCheckBox.Unchecked, AddressOf ProcessingOption_Changed
            AddHandler Me.GenerateSummaryCheckBox.Checked, AddressOf ProcessingOption_Changed
            AddHandler Me.GenerateSummaryCheckBox.Unchecked, AddressOf ProcessingOption_Changed
            AddHandler Me.ProductComboBox.SelectionChanged, AddressOf ProductComboBox_SelectionChanged
        End Sub

        ' Product Selection Changed
        Private Sub ProductComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim comboBox As ComboBox = CType(sender, ComboBox)
            Dim selectedItem As ComboBoxItem = CType(comboBox.SelectedItem, ComboBoxItem)
            _selectedProduct = If(selectedItem?.Content?.ToString(), Nothing)
            UpdateUploadButtonState()
        End Sub

        ' Update Upload Button State
        Private Sub UpdateUploadButtonState()
            UploadButton.IsEnabled = _selectedFiles.Count > 0 AndAlso Not String.IsNullOrEmpty(_selectedProduct) AndAlso Not IsLoading
        End Sub

        ' Document Type Changed
        Private Sub DocumentTypeComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim comboBox As ComboBox = CType(sender, ComboBox)
            Dim selectedItem As ComboBoxItem = CType(comboBox.SelectedItem, ComboBoxItem)
            If selectedItem IsNot Nothing Then
                _selectedDocumentType = selectedItem.Content.ToString()
            End If
        End Sub

        ' Processing Options Changed
        Private Sub ProcessingOption_Changed(sender As Object, e As RoutedEventArgs)
            _autoCategorize = AutoCategorizeCheckBox.IsChecked.Value
            _extractSentiments = ExtractSentimentsCheckBox.IsChecked.Value
            _generateSummary = GenerateSummaryCheckBox.IsChecked.Value
        End Sub

        ' Get Tenant Name (Placeholder)
        Private Function GetTenantName(tenantId As String) As String
            Return "Your Store Name" ' Replace with actual lookup if needed
        End Function

        ' Populate Sample Uploads
        Private Sub PopulateSampleUploads()
            _recentUploads.Add(New UploadItem("feedback_2025.pdf", "Mar 14, 2025 10:30 AM", "Processed", "#2C9E4B"))
            _recentUploads.Add(New UploadItem("reviews_mar25.xlsx", "Mar 14, 2025 9:15 AM", "Processed", "#2C9E4B"))
        End Sub

        ' Drag and Drop Handlers
        Private Sub UploadArea_Drop(sender As Object, e As DragEventArgs)
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                Dim files = CType(e.Data.GetData(DataFormats.FileDrop), String())
                For Each file In files
                    _selectedFiles.Add(file)
                    _recentUploads.Insert(0, New UploadItem(Path.GetFileName(file), DateTime.Now.ToString("MMM dd, yyyy hh:mm tt"), "Pending", "#9E9E9E"))
                Next
                UpdateUploadButtonState()
            End If
            e.Handled = True
        End Sub

        Private Sub UploadArea_DragOver(sender As Object, e As DragEventArgs)
            e.Effects = If(e.Data.GetDataPresent(DataFormats.FileDrop), DragDropEffects.Copy, DragDropEffects.None)
            e.Handled = True
        End Sub

        ' Select Files Button Click
        Private Sub SelectFilesButton_Click(sender As Object, e As RoutedEventArgs)
            Dim openFileDialog As New OpenFileDialog With {
                .Multiselect = True,
                .Filter = "All Files (*.*)|*.*"
            }
            If openFileDialog.ShowDialog() = True Then
                For Each file In openFileDialog.FileNames
                    _selectedFiles.Add(file)
                    _recentUploads.Insert(0, New UploadItem(Path.GetFileName(file), DateTime.Now.ToString("MMM dd, yyyy hh:mm tt"), "Pending", "#9E9E9E"))
                Next
                UpdateUploadButtonState()
            End If
        End Sub

        ' Upload Button Click
        Private Async Sub UploadButton_Click(sender As Object, e As RoutedEventArgs)
            If _selectedFiles.Count = 0 Then
                ShowWarning("Please select at least one file to upload.")
                Return
            End If

            If String.IsNullOrEmpty(_selectedProduct) Then
                ShowWarning("Please select a product before uploading.")
                Return
            End If

            Try
                IsLoading = True
                UploadProgress = 0
                _totalFilesToUpload = _selectedFiles.Count
                _uploadingFileCount = 0
                Dim batchId As String = DateTime.Now.ToString("yyyyMMddHHmmss")

                ToggleUploadControls(False)

                For Each file In _selectedFiles.ToList()
                    _uploadingFileCount += 1
                    LoadingMessage = $"Uploading file {_uploadingFileCount} of {_totalFilesToUpload}: {Path.GetFileName(file)}"
                    Await UploadToAzureAsync(file, _selectedProduct, batchId)
                    UploadProgress = CInt((_uploadingFileCount / _totalFilesToUpload) * 100)
                Next

                LoadingMessage = "Upload complete!"
                Await Task.Delay(1000)
                _selectedFiles.Clear()
                UpdateUploadButtonState()
            Catch ex As Exception
                ShowError("Unexpected error during upload: " & ex.Message)
            Finally
                IsLoading = False
                ToggleUploadControls(True)
            End Try
        End Sub

        ' Toggle Upload Controls
        Private Sub ToggleUploadControls(enabled As Boolean)
            UploadButton.IsEnabled = enabled AndAlso (_selectedFiles.Count > 0 AndAlso Not String.IsNullOrEmpty(_selectedProduct))
            SelectFilesButton.IsEnabled = enabled
            ClearButton.IsEnabled = enabled
        End Sub

        ' Upload to Azure Asynchronously
        Private Async Function UploadToAzureAsync(filePath As String, productName As String, batchId As String) As Task
            Try
                Dim apiUrl As String = "https://analytiq-api.azurewebsites.net/api/FileUpload/upload"
                Dim tenantId As String = _currentTenantId
                Dim fileName As String = Path.GetFileName(filePath)

                UpdateFileStatus(fileName, "Uploading", "#FF9800")

                Using fileStream As New FileStream(filePath, FileMode.Open, FileAccess.Read)
                    Using client As New Net.Http.HttpClient()
                        Using form As New Net.Http.MultipartFormDataContent()
                            Dim fileContent As New Net.Http.StreamContent(fileStream)
                            fileContent.Headers.ContentType = New Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream")
                            form.Add(fileContent, "file", fileName)
                            form.Add(New Net.Http.StringContent(tenantId), "tenantId")
                            form.Add(New Net.Http.StringContent(batchId), "batchId")
                            form.Add(New Net.Http.StringContent(productName), "productName")

                            UpdateFileStatus(fileName, "Processing", "#FF9800")

                            Dim response As Net.Http.HttpResponseMessage = Await client.PostAsync(apiUrl, form)
                            If response.IsSuccessStatusCode Then
                                Dim responseBody As String = Await response.Content.ReadAsStringAsync()
                                Console.WriteLine($"Upload successful: {responseBody}")
                                UpdateFileStatus(fileName, "Processed", "#2C9E4B")
                            Else
                                Dim errorResponse As String = Await response.Content.ReadAsStringAsync()
                                Throw New Exception($"Server responded with {response.StatusCode}: {errorResponse}")
                            End If
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                Console.WriteLine($"Upload error for {Path.GetFileName(filePath)}: {ex.Message}")
                UpdateFileStatus(Path.GetFileName(filePath), "Failed", "#FF5252")
                Application.Current.Dispatcher.Invoke(Sub()
                                                          MessageBox.Show($"Failed to upload {Path.GetFileName(filePath)}: {ex.Message}", "Upload Error", MessageBoxButton.OK, MessageBoxImage.Error)
                                                      End Sub)
            End Try
        End Function

        ' Update File Status (Thread-Safe)
        Private Sub UpdateFileStatus(fileName As String, status As String, color As String)
            Application.Current.Dispatcher.Invoke(Sub()
                                                      Dim uploadItem = _recentUploads.FirstOrDefault(Function(x) x.FileName = fileName)
                                                      If uploadItem IsNot Nothing Then
                                                          uploadItem.StatusText = status
                                                          uploadItem.StatusColor = color
                                                      End If
                                                  End Sub)
        End Sub

        ' Clear Button Click
        Private Sub ClearButton_Click(sender As Object, e As RoutedEventArgs)
            _selectedFiles.Clear()
            UpdateUploadButtonState()
        End Sub

        ' View All Button Click (Placeholder)
        Private Sub ViewAllButton_Click(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("This would show all uploads! Not built yet.", "Info", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub

        ' Helper Methods
        Private Sub ShowWarning(message As String)
            Application.Current.Dispatcher.Invoke(Sub()
                                                      MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                                                  End Sub)
        End Sub

        Private Sub ShowError(message As String)
            Application.Current.Dispatcher.Invoke(Sub()
                                                      MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                                                  End Sub)
        End Sub
    End Class

    ' UploadItem Class with INotifyPropertyChanged
    Public Class UploadItem
        Implements INotifyPropertyChanged

        Private _fileName As String
        Private _uploadTime As String
        Private _statusText As String
        Private _statusColor As String

        Public Property FileName As String
            Get
                Return _fileName
            End Get
            Set(value As String)
                _fileName = value
                OnPropertyChanged("FileName")
            End Set
        End Property

        Public Property UploadTime As String
            Get
                Return _uploadTime
            End Get
            Set(value As String)
                _uploadTime = value
                OnPropertyChanged("UploadTime")
            End Set
        End Property

        Public Property StatusText As String
            Get
                Return _statusText
            End Get
            Set(value As String)
                _statusText = value
                OnPropertyChanged("StatusText")
                OnPropertyChanged("StatusBackground")
            End Set
        End Property

        Public Property StatusColor As String
            Get
                Return _statusColor
            End Get
            Set(value As String)
                _statusColor = value
                OnPropertyChanged("StatusColor")
            End Set
        End Property

        Public ReadOnly Property StatusBackground As String
            Get
                Return If(StatusText = "Processed", "#1A2C1A", "#2A2A2A")
            End Get
        End Property

        Public Sub New(fileName As String, uploadTime As String, statusText As String, statusColor As String)
            Me.FileName = fileName
            Me.UploadTime = uploadTime
            Me.StatusText = statusText
            Me.StatusColor = statusColor
        End Sub

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Protected Sub OnPropertyChanged(propertyName As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub
    End Class
End Namespace