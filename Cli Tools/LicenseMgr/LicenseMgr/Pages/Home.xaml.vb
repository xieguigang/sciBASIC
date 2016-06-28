#Region "d0f18c495c38e4bc4da385a58db95615, ..\LicenseMgr\Pages\Home.xaml.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Windows
Imports System.Windows.Controls
Imports Microsoft.VisualBasic.Linq

'Imports Microsoft.VisualBasic.Windows.Forms

Namespace Pages

    ''' <summary>
    ''' Interaction logic for Home.xaml
    ''' </summary>
    Partial Public Class Home
        Inherits UserControl
        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub Admin_Rights(sender As Object, e As RoutedEventArgs)
            '   VistaSecurity.RestartElevated()
        End Sub

        Private Sub UAC_Initialized(sender As Object, e As EventArgs)
            'If VistaSecurity.IsAdmin() Then
            '    UAC.Visibility = System.Windows.Visibility.Collapsed
            '    UACNote.Visibility = System.Windows.Visibility.Collapsed
            'End If
        End Sub

        Private Sub license_brief_TextChanged(sender As Object, e As TextChangedEventArgs) Handles license_brief.TextChanged
            LicenseInfo.info.Brief = license_brief.Text
        End Sub

        Private Sub license_title_TextChanged(sender As Object, e As TextChangedEventArgs) Handles license_title.TextChanged
            LicenseInfo.info.Title = license_title.Text
        End Sub

        Private Sub copyright_TextChanged(sender As Object, e As TextChangedEventArgs) Handles copyright.TextChanged
            LicenseInfo.info.Copyright = copyright.Text
        End Sub

        Private Sub Load_Click(sender As Object, e As RoutedEventArgs) Handles Load.Click
            Using file As New System.Windows.Forms.OpenFileDialog With {.Filter = "Xml Meta data(*.xml)|*.xml"}
                If file.ShowDialog = Forms.DialogResult.OK Then
                    LicenseInfo.info = file.FileName.LoadXml(Of SoftwareToolkits.LicenseInfo)

                    copyright.Text = info.Copyright
                    license_title.Text = info.Title
                    license_brief.Text = info.Brief

                    For Each author In info.Authors.SafeQuery
                        Dim name As TextBox = Nothing
                        Dim email As TextBox = Nothing

                        Call AuthorAddCommon(name, email)

                        If Not name Is Nothing Then
                            name.Text = author.Name

                        End If
                        If Not email Is Nothing Then
                            email.Text = author.x
                        End If
                    Next
                End If
            End Using
        End Sub

        Private Sub Save_Click(sender As Object, e As RoutedEventArgs) Handles Save.Click
            Using file As New System.Windows.Forms.SaveFileDialog With {.Filter = "Xml Meta data(*.xml)|*.xml"}
                If file.ShowDialog = Forms.DialogResult.OK Then
                    Call LicenseInfo.info.GetXml.SaveTo(file.FileName)
                End If
            End Using
        End Sub

        Private Sub Add_Author_Click(sender As Object, e As RoutedEventArgs) Handles Add_Author.Click
            Call AuthorAddCommon(Nothing, Nothing)
        End Sub

        Private Sub AuthorAddCommon(ByRef name As TextBox, ByRef email As TextBox)

        End Sub
    End Class
End Namespace

