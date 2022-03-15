#Region "Microsoft.VisualBasic::e54a536d882ffc4bdd2631f41fa10f57, sciBASIC#\vs_solutions\dev\LicenseMgr\LicenseMgr\Pages\FolderSymbolic.xaml.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 75
    '    Code Lines: 54
    ' Comment Lines: 8
    '   Blank Lines: 13
    '     File Size: 3.22 KB


    '     Class FolderSymbolic
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Admin_Rights, Original_Folder, Symlink_Create, UserControl_Initialized
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports FirstFloor.ModernUI.Presentation
Imports FirstFloor.ModernUI.Windows.Controls
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio
Imports Microsoft.VisualBasic.Serialization.JSON

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

            Dim Browse As New System.Windows.Forms.FolderBrowserDialog()
            Browse.Description = "Choose the original folder where to apply this license info:"
            Browse.RootFolder = System.Environment.SpecialFolder.Desktop
            Browse.ShowNewFolderButton = True
            Dim result As System.Windows.Forms.DialogResult = Browse.ShowDialog()
            If result = System.Windows.Forms.DialogResult.OK Then
                BrowsedFolder.Text = Browse.SelectedPath
            End If
        End Sub

        Private Sub Symlink_Create(sender As Object, e As RoutedEventArgs)
            If BrowsedFolder.Text = "You haven't selected a source yet." Then
                ModernDialog.ShowMessage("You didn't choose a source. Please do it.", "Oops!", MessageBoxButton.OK)
            Else
                Try

                    Dim rootDir$ = BrowsedFolder.Text
                    Dim failures = CodeSign.LicenseMgr.Inserts(info, rootDir)

                    If failures.Length > 0 Then
                        Dim ex As New Exception("These files are write data failures!")
                        ex = New Exception(failures.GetJson)
                        Throw ex
                    End If

                    ModernDialog.ShowMessage("The source code license info applied success.", "Success!", MessageBoxButton.OK)
                    BrowsedFolder.Text = "You haven't selected a source yet."

                Catch ex As Exception

                    ex = New Exception(BrowsedFolder.Text)
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
            '   VisualBasic.App.RunAsAdmin()
        End Sub
    End Class
End Namespace
