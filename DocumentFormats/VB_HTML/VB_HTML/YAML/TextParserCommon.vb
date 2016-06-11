Imports System.Collections.Generic
Imports System.Text

Namespace YAML.Grammar

    Partial Public Class YamlParser

        Public Property Position() As Integer

        Private Input As ParserInput(Of Char)

        Public Errors As New List(Of KeyValuePair(Of Integer, String))()
        Private ErrorStatck As New Stack(Of Integer)()

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
            Errors.Add(New KeyValuePair(Of Integer, String)(_Position, message))
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
            For Each msg As KeyValuePair(Of Integer, String) In Errors
                text.Append(Input.FormErrorMessage(msg.Key, msg.Value))
                text.AppendLine()
            Next
            Return text.ToString()
        End Function
    End Class
End Namespace
