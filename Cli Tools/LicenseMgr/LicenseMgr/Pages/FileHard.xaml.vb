Imports FirstFloor.ModernUI.Presentation
Imports FirstFloor.ModernUI.Windows.Controls
Imports Microsoft.Win32
Imports System.IO
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
'Imports Microsoft.VisualBasic.Windows.Forms
'Imports Microsoft.VisualBasic.FileIO.SymLinker

Namespace Pages
    ''' <summary>
    ''' Interaction logic for FileHard.xaml
    ''' </summary>
    Partial Public Class FileHard
        Inherits UserControl
        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub Original_File(sender As Object, e As RoutedEventArgs)
            BrowsedFile.Text = "You haven't selected a source yet."
            HardLink.Text = "You haven't selected a hard link yet."
            Dim Browse As New OpenFileDialog()
            Browse.Title = "Browse..."
            Browse.InitialDirectory = "Desktop"
            Browse.CheckFileExists = True
            Browse.CheckPathExists = True
            Browse.ValidateNames = True
            Browse.Multiselect = False
            Dim FileSelected As Nullable(Of Boolean) = Browse.ShowDialog()
            If FileSelected = True Then
                BrowsedFile.Text = Browse.FileName
            End If
        End Sub

        Private Sub Hard_Link(sender As Object, e As RoutedEventArgs)
            Dim Save As New SaveFileDialog()
            Save.CheckPathExists = True
            Save.ValidateNames = True
            Dim FileSelected As Nullable(Of Boolean) = Save.ShowDialog()
            If FileSelected = True Then
                HardLink.Text = Save.FileName
            End If
        End Sub

        Private Sub HardLink_Create(sender As Object, e As RoutedEventArgs)
            'If BrowsedFile.Text = "You haven't selected a source yet." Then
            '    ModernDialog.ShowMessage("You didn't choose a source. Please do it.", "Oops!", MessageBoxButton.OK)
            'ElseIf HardLink.Text = "You haven't selected a hard link yet." Then
            '    ModernDialog.ShowMessage("You didn't choose a hard link. Please do it.", "Oops!", MessageBoxButton.OK)
            'Else
            '    If File.Exists(HardLink.Text) Then
            '        File.Delete(HardLink.Text)
            '    End If
            '    Dim Sucess As Boolean = CreateHardLink(HardLink.Text, BrowsedFile.Text, IntPtr.Zero)
            '    If Sucess = False Then
            '        Dim OldColor As Color = AppearanceManager.Current.AccentColor
            '        AppearanceManager.Current.AccentColor = Color.FromRgb(&HE5, &H14, &H0)
            '        If Not VistaSecurity.IsAdmin() Then
            '            ModernDialog.ShowMessage("There was an error creating the hard link! Probably because you are trying to create a hard link in an non-NTFS drive, you do not have the permissions to write here or you are creating links between drives. (You can restart the program as administrator at the home page)", "Oops!", MessageBoxButton.OK)
            '        Else
            '            ModernDialog.ShowMessage("There was an error creating the hard link! Probably because you are trying to create a hard link in an non-NTFS drive or you are creating links between drives.", "Oops!", MessageBoxButton.OK)
            '        End If
            '        AppearanceManager.Current.AccentColor = OldColor
            '    Else
            '        ModernDialog.ShowMessage("The hard link was created with success.", "Success!", MessageBoxButton.OK)
            '        BrowsedFile.Text = "You haven't selected a source yet."
            '        HardLink.Text = "You haven't selected a hard link yet."
            '    End If
            'End If
        End Sub
    End Class
End Namespace
