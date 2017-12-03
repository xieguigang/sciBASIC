#Region "Microsoft.VisualBasic::99ea9311acbf1bbb7dff15484418053a, ..\sciBASIC#\mime\text%yaml\TextParserCommon.vb"

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

Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Language

Namespace Grammar

    Partial Public Class YamlParser

        Public Property Position() As Integer
        Public Errors As New List(Of LineValue(Of String))()

        Dim ErrorStatck As New Stack(Of Integer)()
        Dim Input As ParserInput(Of Char)

        Public Sub New()
        End Sub

        Private Sub SetInput(input__1 As ParserInput(Of Char))
            Input = input__1
            Position = 0
        End Sub

        Private Function TerminalMatch(terminal As Char) As Boolean
            If Input.HasInput(_Position) Then
                Dim symbol As Char = Input.GetInputSymbol(_Position)
                Return terminal = symbol
            End If
            Return False
        End Function

        Private Function TerminalMatch(terminal As Char, pos As Integer) As Boolean
            If Input.HasInput(pos) Then
                Dim symbol As Char = Input.GetInputSymbol(pos)
                Return terminal = symbol
            End If
            Return False
        End Function

        Private Function MatchTerminal(terminal As Char, ByRef success As Boolean) As Char
            success = False
            If Input.HasInput(_Position) Then
                Dim symbol As Char = Input.GetInputSymbol(_Position)
                If terminal = symbol Then
                    _Position += 1
                    success = True
                End If
                Return symbol
            End If
            Return ControlChars.NullChar
        End Function

        Private Function MatchTerminalRange(start As Char, [end] As Char, ByRef success As Boolean) As Char
            success = False
            If Input.HasInput(_Position) Then
                Dim symbol As Char = Input.GetInputSymbol(_Position)
                If start <= symbol AndAlso symbol <= [end] Then
                    _Position += 1
                    success = True
                End If
                Return symbol
            End If
            Return ControlChars.NullChar
        End Function

        Private Function MatchTerminalSet(terminalSet As String, isComplement As Boolean, ByRef success As Boolean) As Char
            success = False
            If Input.HasInput(_Position) Then
                Dim symbol As Char = Input.GetInputSymbol(_Position)
                Dim match As Boolean = If(isComplement, terminalSet.IndexOf(symbol) = -1, terminalSet.IndexOf(symbol) > -1)
                If match Then
                    _Position += 1
                    success = True
                End If
                Return symbol
            End If
            Return ControlChars.NullChar
        End Function

        Private Function MatchTerminalString(terminalString As String, ByRef success As Boolean) As String
            Dim currrent_position As Integer = _Position
            For Each terminal As Char In terminalString
                MatchTerminal(terminal, success)
                If Not success Then
                    _Position = currrent_position
                    Return Nothing
                End If
            Next
            success = True
            Return terminalString
        End Function

        Private Function [Error](message As String) As Integer
            Errors += New LineValue(Of String) With {
                .Line = _Position,
                .value = message
            }
            Return Errors.Count
        End Function

        Private Sub ClearError(count As Integer)
            Errors.RemoveRange(count, Errors.Count - count)
        End Sub

        ''' <summary>
        ''' 获取得到解析的过程之中的错误消息
        ''' </summary>
        ''' <returns></returns>
        Public Function GetEorrorMessages() As String
            Dim text As New StringBuilder()
            For Each msg As LineValue(Of String) In Errors
                text.Append(Input.FormErrorMessage(msg.Line, msg.value))
                text.AppendLine()
            Next
            Return text.ToString()
        End Function
    End Class
End Namespace
