#Region "Microsoft.VisualBasic::cc6253b3d85fc3c3087c8f639f213181, vs_solutions\dev\LicenseMgr\LicenseMgr\Pages\FileSymbolic.xaml.vb"

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
