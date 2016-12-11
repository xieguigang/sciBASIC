#Region "Microsoft.VisualBasic::b0591dd377aeed8bbba530aff66f71be, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Tools\WinForm\Extensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace Windows.Forms

    Public Module Extensions

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
