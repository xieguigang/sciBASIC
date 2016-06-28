Imports System.IO
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports FirstFloor.ModernUI.Presentation
Imports FirstFloor.ModernUI.Windows.Controls
Imports Microsoft
'Imports Microsoft.VisualBasic.Windows.Forms
Imports Microsoft.Win32
'Imports Microsoft.VisualBasic.FileIO.SymLinker

Namespace Pages

    ''' <summary>
    ''' Interaction logic for FileSymbolic.xaml
    ''' </summary>
    Partial Public Class FileSymbolic
        Inherits UserControl
        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub Original_File(sender As Object, e As RoutedEventArgs)
            BrowsedFile.Text = "You haven't selected a source yet."

            Dim Browse As New OpenFileDialog() With {.Filter = "Microsoft VisualBasic Source Code(*.vb)|*.vb"}
            Browse.Title = "Browse for source file"
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

        Private Sub Symlink_Create(sender As Object, e As RoutedEventArgs)
            If BrowsedFile.Text = "You haven't selected a source yet." Then
                ModernDialog.ShowMessage("You didn't choose a source. Please do it.", "Oops!", MessageBoxButton.OK)
            Else
                Dim CatchException As Boolean = False
                Try



                Catch generatedExceptionName As Exception
                    CatchException = True
                End Try
                If CatchException = True Then
                    Dim OldColor As Color = AppearanceManager.Current.AccentColor
                    AppearanceManager.Current.AccentColor = Color.FromRgb(&HE5, &H14, &H0)
                    ModernDialog.ShowMessage("There was an error creating the symbolic link! Probably because you are trying to create a symbolic link in an non-NTFS drive.", "Oops!", MessageBoxButton.OK)
                    AppearanceManager.Current.AccentColor = OldColor
                Else
                    BrowsedFile.Text = "You haven't selected a source yet."
                    ModernDialog.ShowMessage("The symbolic link was created with success.", "Success!", MessageBoxButton.OK)
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
            '  VisualBasic.App.RunAsAdmin()
        End Sub
    End Class
End Namespace
