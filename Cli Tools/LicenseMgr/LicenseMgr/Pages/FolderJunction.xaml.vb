Imports FirstFloor.ModernUI.Presentation
Imports FirstFloor.ModernUI.Windows.Controls
Imports System.Windows
Imports System.Windows.Forms
Imports System.Windows.Media
'Imports Microsoft.VisualBasic.Windows.Forms
'Imports Microsoft.VisualBasic.FileIO.SymLinker

Namespace Pages
    ''' <summary>
    ''' Interaction logic for FolderJunction.xaml
    ''' </summary>
    Partial Public Class FolderJunction
        Inherits System.Windows.Controls.UserControl
        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub Original_Folder(sender As Object, e As RoutedEventArgs)
            BrowsedFolder.Text = "You haven't selected a source yet."
            JunctionPoint.Text = "You haven't selected a junction point yet."
            Dim Browse As New FolderBrowserDialog()
            Browse.Description = "Choose the original folder where the junction point will refer:"
            Browse.RootFolder = System.Environment.SpecialFolder.Desktop
            Browse.ShowNewFolderButton = True
            Dim result As DialogResult = Browse.ShowDialog()
            If result = DialogResult.OK Then
                BrowsedFolder.Text = Browse.SelectedPath
            End If
        End Sub

        Private Sub Junction_Folder(sender As Object, e As RoutedEventArgs)
            Dim Browse As New FolderBrowserDialog()
            Browse.Description = "Choose where the junction point will be:"
            Browse.RootFolder = System.Environment.SpecialFolder.Desktop
            Browse.ShowNewFolderButton = True
            Dim result As DialogResult = Browse.ShowDialog()
            If result = DialogResult.OK Then
                JunctionPoint.Text = Browse.SelectedPath
            End If
        End Sub

        Private Sub Junction_Create(sender As Object, e As RoutedEventArgs)
            'If BrowsedFolder.Text = "You haven't selected a source yet." Then
            '    ModernDialog.ShowMessage("You didn't choose a source. Please do it.", "Oops!", MessageBoxButton.OK)
            'ElseIf JunctionPoint.Text = "You haven't selected a junction point yet." Then
            '    ModernDialog.ShowMessage("You didn't choose a junction point. Please do it.", "Oops!", MessageBoxButton.OK)
            'Else
            '    Dim CatchException As Boolean = False
            '    Try
            '        FileIO.SymLinker.JunctionPoint.Create(JunctionPoint.Text, BrowsedFolder.Text, True)
            '    Catch generatedExceptionName As Exception
            '        CatchException = True
            '    End Try
            '    If CatchException = True Then
            '        Dim OldColor As Color = AppearanceManager.Current.AccentColor
            '        AppearanceManager.Current.AccentColor = Color.FromRgb(&HE5, &H14, &H0)
            '        If Not VistaSecurity.IsAdmin() Then
            '            ModernDialog.ShowMessage("There was an error creating the junction point! Probably because you are trying to create a junction point in an non-NTFS drive, you do not have the permissions to write here or you are creating junction points between drives. (You can restart the program as administrator at the home page)", "Oops!", MessageBoxButton.OK)
            '        Else
            '            ModernDialog.ShowMessage("There was an error creating the junction point! Probably because you are trying to create a junction point in an non-NTFS drive or you are creating junction points between drives.", "Oops!", MessageBoxButton.OK)
            '        End If
            '        AppearanceManager.Current.AccentColor = OldColor
            '    Else
            '        ModernDialog.ShowMessage("The junction point was created with success.", "Success!", MessageBoxButton.OK)
            '        BrowsedFolder.Text = "You haven't selected a source yet."
            '        JunctionPoint.Text = "You haven't selected a junction point yet."
            '    End If
            'End If
        End Sub
    End Class
End Namespace
