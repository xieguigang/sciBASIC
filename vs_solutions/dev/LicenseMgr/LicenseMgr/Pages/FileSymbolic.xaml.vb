#Region "Microsoft.VisualBasic::cc6253b3d85fc3c3087c8f639f213181, vs_solutions\dev\LicenseMgr\LicenseMgr\Pages\FileSymbolic.xaml.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class FileSymbolic
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Admin_Rights, Original_File, Symlink_Create, UserControl_Initialized
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports FirstFloor.ModernUI.Presentation
Imports FirstFloor.ModernUI.Windows.Controls
Imports Microsoft
Imports Microsoft.VisualBasic.ApplicationServices
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

            Dim Browse As New OpenFileDialog() With {
                .Filter = "Microsoft VisualBasic Source Code(*.vb)|*.vb"
            }
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
                Try
                    Dim rootDir = BrowsedFile.Text.ParentPath

                    Development.LicenseMgr.Insert(BrowsedFile.Text, info, rootDir)
                    BrowsedFile.Text = "You haven't selected a source yet."
                    ModernDialog.ShowMessage("License information applied success.", "Success!", MessageBoxButton.OK)
                Catch ex As Exception
                    ex = New Exception(BrowsedFile.Text, ex)

                    Call Microsoft.VisualBasic.App.LogException(ex)

                    Dim OldColor As Color = AppearanceManager.Current.AccentColor
                    AppearanceManager.Current.AccentColor = Color.FromRgb(&HE5, &H14, &H0)
                    ModernDialog.ShowMessage(ex.ToString, "Oops!", MessageBoxButton.OK)
                    AppearanceManager.Current.AccentColor = OldColor
                End Try
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
