#Region "Microsoft.VisualBasic::b2947820427deea80114643b87459140, sciBASIC#\vs_solutions\dev\LicenseMgr\LicenseMgr\Pages\Home.xaml.vb"

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

    '   Total Lines: 128
    '    Code Lines: 92
    ' Comment Lines: 7
    '   Blank Lines: 29
    '     File Size: 4.65 KB


    '     Class Home
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: __update, Add_Author_Click, AuthorAddCommon, copyright_TextChanged, license_brief_TextChanged
    '              license_title_TextChanged, Load_Click, Save_Click
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.CodeSign
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models

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

        Private Sub license_brief_TextChanged(sender As Object, e As TextChangedEventArgs) Handles license_brief.TextChanged
            LicenseInfoExtensions.info.Brief = license_brief.Text
        End Sub

        Private Sub license_title_TextChanged(sender As Object, e As TextChangedEventArgs) Handles license_title.TextChanged
            LicenseInfoExtensions.info.Title = license_title.Text
        End Sub

        Private Sub copyright_TextChanged(sender As Object, e As TextChangedEventArgs) Handles copyright.TextChanged
            LicenseInfoExtensions.info.Copyright = copyright.Text
        End Sub

        Private Sub Load_Click(sender As Object, e As RoutedEventArgs) Handles Load.Click
            Using file As New System.Windows.Forms.OpenFileDialog With {
                .Filter = "Xml Meta data(*.xml)|*.xml"
            }
                If file.ShowDialog = Forms.DialogResult.OK Then
                    LicenseInfoExtensions.info = file.FileName.LoadXml(Of LicenseInfo)

                    copyright.Text = info.Copyright
                    license_title.Text = info.Title
                    license_brief.Text = info.Brief

                    Call authors.Clear()
                    Call listBox.Items.Clear()
                    lh = 0

                    For Each author In info.Authors.SafeQuery
                        Dim name As TextBox = Nothing
                        Dim email As TextBox = Nothing

                        Call AuthorAddCommon(name, email)

                        If Not name Is Nothing Then
                            name.Text = author.name

                        End If
                        If Not email Is Nothing Then
                            email.Text = author.text
                        End If
                    Next
                End If
            End Using
        End Sub

        Dim authors As New List(Of KeyValuePair(Of TextBox, TextBox))

        Private Sub Save_Click(sender As Object, e As RoutedEventArgs) Handles Save.Click
            Using file As New System.Windows.Forms.SaveFileDialog With {
                .Filter = "Xml Meta data(*.xml)|*.xml"
            }
                If file.ShowDialog = Forms.DialogResult.OK Then
                    Call LicenseInfoExtensions.info.GetXml.SaveTo(file.FileName)
                End If
            End Using
        End Sub

        Private Sub Add_Author_Click(sender As Object, e As RoutedEventArgs) Handles Add_Author.Click
            Call AuthorAddCommon(Nothing, Nothing)
        End Sub

        Dim lh As Integer = 28

        Private Sub AuthorAddCommon(ByRef name As TextBox, ByRef email As TextBox)
            Dim canvas As New Canvas

            Call listBox.Items.Add(canvas)

            name = New TextBox With {.Width = 150, .Height = 23}
            email = New TextBox With {.Width = 300, .Height = 23}

            Call canvas.Children.Add(name)
            Call canvas.Children.Add(email)

            lh += 5

            Canvas.SetTop(name, lh)
            Canvas.SetTop(email, lh)
            Canvas.SetLeft(name, 0)
            Canvas.SetLeft(email, 160)

            lh += 23

            Call authors.Add(New KeyValuePair(Of TextBox, TextBox)(name, email))

            AddHandler name.TextChanged, Sub() Call __update()
            AddHandler email.TextChanged, Sub() Call __update()

            Call __update()
        End Sub

        ''' <summary>
        ''' 最后在这进行数据更新
        ''' </summary>
        Private Sub __update()
            info.Authors = LinqAPI.Exec(Of NamedValue) _
                                                       _
                () <= From author As KeyValuePair(Of TextBox, TextBox)
                      In Me.authors
                      Select New NamedValue With {
                          .name = author.Key.Text,
                          .text = author.Value.Text
                      }
        End Sub
    End Class
End Namespace
