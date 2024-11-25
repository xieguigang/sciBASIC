#Region "Microsoft.VisualBasic::5672e89e52af8489ecb291099ed88b03, mime\text%yaml\1.2\TextParserCommon.vb"

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

    '   Total Lines: 131
    '    Code Lines: 99 (75.57%)
    ' Comment Lines: 16 (12.21%)
    '    - Xml Docs: 93.75%
    ' 
    '   Blank Lines: 16 (12.21%)
    '     File Size: 4.86 KB


    '     Class YamlParser
    ' 
    '         Properties: Position
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: [Error], BuildErrorMessages, GetErrorMessageText, MatchTerminal, MatchTerminalRange
    '                   MatchTerminalSet, MatchTerminalString, (+2 Overloads) TerminalMatch
    ' 
    '         Sub: ClearError, SetInput
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
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

        Private Sub SetInput(input As ParserInput(Of Char))
            Me.Input = input
            Me.Position = 0
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

        ''' <summary>
        ''' 如果当前位置的字符匹配<paramref name="terminal"/>那么位置会前进一个字符
        ''' 否则位置不会发生变化
        ''' </summary>
        ''' <param name="terminal"></param>
        ''' <param name="success"></param>
        ''' <returns></returns>
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
                .line = _Position,
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
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetErrorMessageText() As String
            Return BuildErrorMessages.JoinBy(vbCrLf)
        End Function

        ''' <summary>
        ''' 获取得到解析的过程之中的错误消息
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function BuildErrorMessages() As IEnumerable(Of String)
            For Each msg As LineValue(Of String) In Errors
                Yield Input.FormErrorMessage(msg.line, msg.value)
            Next
        End Function
    End Class
End Namespace
