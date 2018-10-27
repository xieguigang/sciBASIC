#Region "Microsoft.VisualBasic::29a9366500996532fe82cc1072f8675c, Microsoft.VisualBasic.Core\ApplicationServices\Tools\WinForm\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: AddFilesHistory, IsPrintableCharacter, ToChar
    ' 
    '         Sub: AddFileHistory, AddRowData, Clear, WriteLine
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Keyboard = System.Windows.Forms.Keys

Namespace Windows.Forms

    Public Module Extensions

        ReadOnly chars As New Dictionary(Of Keys, Char)

        <Extension>
        Public Function IsPrintableCharacter(key As Keys) As Boolean
            Return chars.ContainsKey(key)
        End Function

        <Extension>
        Public Function ToChar(key As Keys) As Char
            If Not key.IsPrintableCharacter Then
                Return Nothing
            Else
                Return chars(key)
            End If
        End Function

        Sub New()
            chars(Keyboard.A) = "a"
            chars(Keyboard.B) = "b"
            chars(Keyboard.C) = "c"
            chars(Keyboard.D) = "d"
            chars(Keyboard.E) = "e"
            chars(Keyboard.F) = "f"
            chars(Keyboard.G) = "g"
            chars(Keyboard.H) = "h"
            chars(Keyboard.I) = "i"
            chars(Keyboard.J) = "j"
            chars(Keyboard.K) = "k"
            chars(Keyboard.L) = "l"
            chars(Keyboard.M) = "m"
            chars(Keyboard.N) = "n"
            chars(Keyboard.O) = "o"
            chars(Keyboard.P) = "p"
            chars(Keyboard.Q) = "q"
            chars(Keyboard.R) = "r"
            chars(Keyboard.S) = "s"
            chars(Keyboard.T) = "t"
            chars(Keyboard.U) = "u"
            chars(Keyboard.V) = "v"
            chars(Keyboard.W) = "w"
            chars(Keyboard.X) = "x"
            chars(Keyboard.Y) = "y"
            chars(Keyboard.Z) = "z"
            chars(Keyboard.D0) = "0" : chars(Keyboard.NumPad0) = "0"
            chars(Keyboard.D0) = "1" : chars(Keyboard.NumPad0) = "1"
            chars(Keyboard.D0) = "2" : chars(Keyboard.NumPad0) = "2"
            chars(Keyboard.D0) = "3" : chars(Keyboard.NumPad0) = "3"
            chars(Keyboard.D0) = "4" : chars(Keyboard.NumPad0) = "4"
            chars(Keyboard.D0) = "5" : chars(Keyboard.NumPad0) = "5"
            chars(Keyboard.D0) = "6" : chars(Keyboard.NumPad0) = "6"
            chars(Keyboard.D0) = "7" : chars(Keyboard.NumPad0) = "7"
            chars(Keyboard.D0) = "8" : chars(Keyboard.NumPad0) = "8"
            chars(Keyboard.D0) = "9" : chars(Keyboard.NumPad0) = "9"
        End Sub

        <Extension>
        Public Sub AddRowData(view As DataGridView, name$, ParamArray values As Object())
            Call view.Rows.Add(values)
            Dim count = view.RowCount
            view.Rows(count - 1).HeaderCell.Value = name
        End Sub

        <Extension>
        Public Sub Clear(view As DataGridView)
            Call view.Columns.Clear()
            Call view.Rows.Clear()
        End Sub

        <Extension>
        Public Function AddFilesHistory(ByRef menu As ToolStripMenuItem, files As IEnumerable(Of String), invoke As Action(Of String), Optional formats As Func(Of String, String) = Nothing) As Boolean
            If formats Is Nothing Then
                formats = Function(path$) path$.FileName & $" ({Mid(path, 1, 30)}...)"
            End If

            For Each path$ In files.SafeQuery
                Dim file As New ToolStripMenuItem With {
                    .Text = formats(path),
                    .AutoToolTip = True,
                    .ToolTipText = path
                }

                AddHandler file.Click,
                    Sub(sender, arg)
                        Call invoke(path)
                    End Sub

                Call menu.DropDownItems.Add(file)
            Next

            Return True
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="files"></param>
        ''' <param name="path$"></param>
        ''' <param name="latestFirst">最近使用的文件被放在列表的头部，否则直接放在列表的尾部</param>
        ''' 
        <Extension>
        Public Sub AddFileHistory(ByRef files As List(Of String), path$, Optional latestFirst As Boolean = True)
            If files Is Nothing Then
                files = New List(Of String)
            End If

            Dim n As Integer = files.IndexOf(path)

            If n <> -1 Then
                Call files.RemoveAt(n)
            End If

            If latestFirst Then
                Call files.Insert(Scan0, path)
            Else
                Call files.Add(path)
            End If
        End Sub

        <Extension>
        Public Sub WriteLine(tb As TextBox, s$)
            Call tb.AppendText(s & vbCrLf)
        End Sub
    End Module
End Namespace
