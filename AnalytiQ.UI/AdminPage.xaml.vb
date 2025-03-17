Imports System.Windows
Imports System.Windows.Controls
Imports System.Collections.Generic
Imports System.Windows.Media ' For Brushes
Imports Microsoft.VisualBasic ' For InputBox

Namespace AnalytiQ.UI
    Public Class AdminPage
        Inherits Page

        Private _users As List(Of UserInfo)
        Private _currentUserRole As String
        Private _originalUsers As List(Of UserInfo)
        Private _currentTenantId As String

        Public Sub New()
            InitializeComponent()
            AddHandler Me.Loaded, AddressOf AdminPage_Loaded
        End Sub

        Private Sub AdminPage_Loaded(sender As Object, e As RoutedEventArgs)
            _currentTenantId = LoginPage.CurrentTenantId
            CheckUserAccess()
            AddHandler txtSearch.TextChanged, AddressOf TxtSearch_TextChanged
        End Sub

        Private Sub CheckUserAccess()
            If String.IsNullOrEmpty(_currentTenantId) OrElse Not LoginPage.IsCompanyRegistered(_currentTenantId) Then
                MessageBox.Show("No tenant authenticated. Please sign in.", "Authentication Error", MessageBoxButton.OK, MessageBoxImage.Error)
                NavigationService.Navigate(New LoginPage(New LoginWindow()))
                Return
            End If

            ' Simulate admin check - replace with Azure AD in production
            _currentUserRole = GetUserRoleForTenant(_currentTenantId, GetCurrentUserEmail())
            If _currentUserRole <> "Admin" Then
                MessageBox.Show("Access Denied: Only Administrators can access this page.", "Permission Error", MessageBoxButton.OK, MessageBoxImage.Error)
                NavigationService.Navigate(New MainWindow(_currentTenantId)) ' Assume MainWindow is Dashboard
            Else
                txtTenantInfo.Text = $"Tenant: {GetTenantName(_currentTenantId)} ({_currentTenantId})"
                LoadUsers()
            End If
        End Sub

        Private Function GetCurrentUserEmail() As String
            ' Simulate current user - in production, from Azure AD token
            Select Case _currentTenantId
                Case "TENANT-001" : Return "bob@acmecorp.com"
                Case "TENANT-999" : Return "alice@newco.com" ' First admin
                Case "DEV-TENANT-999" : Return "dev@analytiq.com"
                Case Else : Return ""
            End Select
        End Function

        Private Function GetUserRoleForTenant(tenantId As String, email As String) As String
            ' Simulate role lookup
            Select Case tenantId
                Case "TENANT-001"
                    If email = "bob@acmecorp.com" Then Return "Admin"
                Case "TENANT-999"
                    If email = "alice@newco.com" Then Return "Admin"
                    If email = "charlie@newco.com" Then Return "Standard"
                Case "DEV-TENANT-999"
                    If email = "dev@analytiq.com" Then Return "Admin"
            End Select
            Return "Standard"
        End Function

        Private Function GetTenantName(tenantId As String) As String
            Select Case tenantId
                Case "TENANT-001" : Return "AcmeCorp"
                Case "TENANT-999" : Return "NewCo"
                Case "DEV-TENANT-999" : Return "Dev Company"
                Case Else : Return "Unknown"
            End Select
        End Function

        Private Sub LoadUsers()
            _users = FetchUsersForTenant(_currentTenantId)
            _originalUsers = New List(Of UserInfo)(_users)
            PopulateUsersUI()
        End Sub

        Private Function FetchUsersForTenant(tenantId As String) As List(Of UserInfo)
            Dim usersList As New List(Of UserInfo)
            Select Case tenantId
                Case "TENANT-001"
                    usersList.Add(New UserInfo With {.Name = "Bob Johnson", .Email = "bob@acmecorp.com", .Role = "Admin"})
                    usersList.Add(New UserInfo With {.Name = "Jane Smith", .Email = "jane.smith@acmecorp.com", .Role = "Standard"})
                Case "TENANT-999"
                    usersList.Add(New UserInfo With {.Name = "Alice Brown", .Email = "alice@newco.com", .Role = "Admin"})
                    usersList.Add(New UserInfo With {.Name = "Charlie Davis", .Email = "charlie@newco.com", .Role = "Standard"})
                Case "DEV-TENANT-999"
                    usersList.Add(New UserInfo With {.Name = "Dev User", .Email = "dev@analytiq.com", .Role = "Admin"})
                Case Else
                    usersList.Add(New UserInfo With {.Name = "John Doe", .Email = "john.doe@company.com", .Role = "Admin"})
            End Select
            Return usersList
        End Function

        Private Sub PopulateUsersUI()
            Dim usersPanel As StackPanel = DirectCast(FindName("UsersPanel"), StackPanel)
            usersPanel.Children.Clear()

            For Each user In _users
                Dim userBorder As New Border() With {
                    .Style = DirectCast(FindResource("UserRowStyle"), Style)
                }
                Dim userRow As New Grid()
                userRow.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(2, GridUnitType.Star)})
                userRow.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(3, GridUnitType.Star)})
                userRow.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(1.5, GridUnitType.Star)})
                userRow.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(1, GridUnitType.Star)})

                Dim namePanel As New StackPanel() With {.Orientation = Orientation.Horizontal}
                Dim avatar As New Border() With {
                    .Width = 32, .Height = 32, .CornerRadius = New CornerRadius(16),
                    .Background = Brushes.AliceBlue, .Margin = New Thickness(0, 0, 10, 0)
                }
                Dim avatarText As New TextBlock() With {
                    .Text = If(user.Name.Length >= 2, user.Name.Substring(0, 2).ToUpper(), user.Name.ToUpper()),
                    .Foreground = Brushes.DarkGreen, .HorizontalAlignment = HorizontalAlignment.Center,
                    .VerticalAlignment = VerticalAlignment.Center, .FontWeight = FontWeights.SemiBold
                }
                avatar.Child = avatarText
                namePanel.Children.Add(avatar)
                Dim nameText As New TextBlock() With {
                    .Text = user.Name, .Style = DirectCast(FindResource("UserDataTextStyle"), Style)
                }
                namePanel.Children.Add(nameText)
                Grid.SetColumn(namePanel, 0)
                userRow.Children.Add(namePanel)

                Dim emailText As New TextBlock() With {
                    .Text = user.Email, .Style = DirectCast(FindResource("UserDataTextStyle"), Style)
                }
                Grid.SetColumn(emailText, 1)
                userRow.Children.Add(emailText)

                Dim roleCombo As New ComboBox() With {
                    .Style = DirectCast(FindResource("RoleComboBoxStyle"), Style),
                    .ItemsSource = New List(Of String) From {"Admin", "Standard", "Read-Only"},
                    .SelectedItem = user.Role
                }
                roleCombo.Tag = user
                AddHandler roleCombo.SelectionChanged, AddressOf RoleCombo_SelectionChanged
                Grid.SetColumn(roleCombo, 2)
                userRow.Children.Add(roleCombo)

                Dim removeButton As New Button() With {
                    .Style = DirectCast(FindResource("RemoveButtonStyle"), Style), .Tag = user
                }
                AddHandler removeButton.Click, AddressOf RemoveUser_Click
                Grid.SetColumn(removeButton, 3)
                userRow.Children.Add(removeButton)

                userBorder.Child = userRow
                usersPanel.Children.Add(userBorder)
            Next
        End Sub

        Private Sub RoleCombo_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim combo As ComboBox = DirectCast(sender, ComboBox)
            Dim user As UserInfo = DirectCast(combo.Tag, UserInfo)
            If combo.SelectedItem IsNot Nothing Then
                user.Role = combo.SelectedItem.ToString()
            End If
        End Sub

        Private Sub RemoveUser_Click(sender As Object, e As RoutedEventArgs)
            Dim button As Button = DirectCast(sender, Button)
            Dim user As UserInfo = DirectCast(button.Tag, UserInfo)
            Dim result As MessageBoxResult = MessageBox.Show($"Remove {user.Name} from {_currentTenantId}?", "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Warning)
            If result = MessageBoxResult.Yes Then
                _users.Remove(user)
                _originalUsers.Remove(user)
                RemoveUser(user)
                PopulateUsersUI()
                MessageBox.Show($"{user.Name} removed from {_currentTenantId}.", "User Removed", MessageBoxButton.OK, MessageBoxImage.Information)
            End If
        End Sub

        Private Sub RemoveUser(user As UserInfo)
            ' TODO: Implement Azure AD user removal for tenant
        End Sub

        Private Sub InviteEmployee_Click(sender As Object, e As RoutedEventArgs)
            Dim email As String = InputBox($"Enter email to invite to {_currentTenantId}:", "Invite Employee")
            If Not String.IsNullOrWhiteSpace(email) AndAlso email.Contains("@") Then
                SendInviteEmail(email)
                _users.Add(New UserInfo With {.Name = email.Split("@")(0), .Email = email, .Role = "Standard"})
                _originalUsers.Add(New UserInfo With {.Name = email.Split("@")(0), .Email = email, .Role = "Standard"})
                PopulateUsersUI()
                MessageBox.Show($"Invited {email} to {_currentTenantId}.", "Invite Sent", MessageBoxButton.OK, MessageBoxImage.Information)
            End If
        End Sub

        Private Sub SendInviteEmail(email As String)
            ' TODO: Implement Azure AD invitation for tenant
        End Sub

        Private Sub SaveChanges_Click(sender As Object, e As RoutedEventArgs)
            SaveUserChanges()
            MessageBox.Show($"User changes saved for {_currentTenantId}.", "Changes Saved", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub

        Private Sub SaveUserChanges()
            ' TODO: Save user roles to Azure AD or backend for tenant
            For Each user In _users
                Console.WriteLine($"Tenant: {_currentTenantId}, User: {user.Name}, Role: {user.Role}")
            Next
        End Sub

        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim searchText As String = txtSearch.Text.ToLower()
            If String.IsNullOrEmpty(searchText) Then
                _users = New List(Of UserInfo)(_originalUsers)
            Else
                _users = _originalUsers.Where(Function(u) u.Name.ToLower().Contains(searchText) OrElse u.Email.ToLower().Contains(searchText)).ToList()
            End If
            PopulateUsersUI()
        End Sub
    End Class

    Public Class UserInfo
        Public Property Name As String
        Public Property Email As String
        Public Property Role As String
    End Class
End Namespace