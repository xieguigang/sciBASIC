Imports FirstFloor.ModernUI.Presentation
Imports FirstFloor.ModernUI.Windows.Controls
Imports System.IO
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
'Imports Microsoft.VisualBasic.FileIO.SymLinker
Imports Microsoft
'Imports Microsoft.VisualBasic.Windows.Forms

Namespace Pages

    ''' <summary>
    ''' Interaction logic for FolderSymbolic.xaml
    ''' </summary>
    Partial Public Class FolderSymbolic
        Inherits UserControl
        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub Original_Folder(sender As Object, e As RoutedEventArgs)
            BrowsedFolder.Text = "You haven't selected a source yet."
            Symbolink.Text = "You haven't selected a symbolic link yet."
            Dim Browse As New System.Windows.Forms.FolderBrowserDialog()
            Browse.Description = "Choose the original folder where the symbolic link will refer:"
            Browse.RootFolder = System.Environment.SpecialFolder.Desktop
            Browse.ShowNewFolderButton = True
            Dim result As System.Windows.Forms.DialogResult = Browse.ShowDialog()
            If result = System.Windows.Forms.DialogResult.OK Then
                BrowsedFolder.Text = Browse.SelectedPath
            End If
        End Sub

        Private Sub Symbolink_Link(sender As Object, e As RoutedEventArgs)
            Dim Browse As New System.Windows.Forms.FolderBrowserDialog()
            Browse.Description = "Choose where the symbolic link will be:"
            Browse.RootFolder = System.Environment.SpecialFolder.Desktop
            Browse.ShowNewFolderButton = True
            Dim result As System.Windows.Forms.DialogResult = Browse.ShowDialog()
            If result = System.Windows.Forms.DialogResult.OK Then
                Symbolink.Text = Browse.SelectedPath
            End If
        End Sub

        Private Sub Symlink_Create(sender As Object, e As RoutedEventArgs)
            If BrowsedFolder.Text = "You haven't selected a source yet." Then
                ModernDialog.ShowMessage("You didn't choose a source. Please do it.", "Oops!", MessageBoxButton.OK)
            ElseIf Symbolink.Text = "You haven't selected a symbolic link yet." Then
                ModernDialog.ShowMessage("You didn't choose a symbolic link. Please do it.", "Oops!", MessageBoxButton.OK)
            Else
                Dim CatchException As Boolean = False
                Try
                    If Directory.Exists(Symbolink.Text) Then
                        Directory.Delete(Symbolink.Text, True)
                    End If
                    '   CreateDirectoryLink(Symbolink.Text, BrowsedFolder.Text)
                Catch generatedExceptionName As Exception
                    CatchException = True
                End Try
                If CatchException = True Then
                    Dim OldColor As Color = AppearanceManager.Current.AccentColor
                    AppearanceManager.Current.AccentColor = Color.FromRgb(&HE5, &H14, &H0)
                    ModernDialog.ShowMessage("There was an error creating the symbolic link! Probably because you are trying to create a symbolic link in an non-NTFS drive.", "Oops!", MessageBoxButton.OK)
                    AppearanceManager.Current.AccentColor = OldColor
                Else
                    ModernDialog.ShowMessage("The symbolic link was created with success.", "Success!", MessageBoxButton.OK)
                    BrowsedFolder.Text = "You haven't selected a source yet."
                    Symbolink.Text = "You haven't selected a symbolic link yet."
                End If
            End If
        End Sub

        Private Sub UserControl_Initialized(sender As Object, e As EventArgs)
            'If Not VistaSecurity.IsAdmin() Then
            '    NoAdmin.Visibility = System.Windows.Visibility.Visible
            '    Admin.Visibility = System.Windows.Visibility.Collapsed
            'End If
        End Sub

        Private Sub Admin_Rights(sender As Object, e As RoutedEventArgs)
            '   VisualBasic.App.RunAsAdmin()
        End Sub
    End Class
End Namespace
