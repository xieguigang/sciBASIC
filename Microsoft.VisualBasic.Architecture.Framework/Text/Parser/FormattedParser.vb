Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq

Namespace Text

    ''' <summary>
    ''' Parser API for the well formatted documents.
    ''' </summary>
    Public Module FormattedParser

        ''' <summary>
        ''' String collection tokens by a certain delimiter string element.
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="isFlag">
        ''' 
        ''' </param>
        ''' <returns></returns>
        <Extension> Public Iterator Function FlagSplit(source As IEnumerable(Of String), isFlag As Func(Of String, Boolean)) As IEnumerable(Of String())
            Dim list As New List(Of String)

            ' >> .........
            ' ............
            ' ............
            ' >> .........
            ' >> .........

            ' 例如上面的一段文本，使用 >> 符号作为段落的分隔符，
            ' 则本函数会将上面的文本分为三行:
            ' >> .........
            ' ............
            ' ............
            ' 和
            ' >> .........
            ' 和
            ' >> .........

            For Each line As String In source
                If isFlag(line) Then
                    If list.Count > 0 Then
                        Yield list.ToArray
                        Call list.Clear()
                    End If
                End If

                Call list.Add(line)
            Next
        End Function

        ''' <summary>
        ''' Example as: ------- ------ -----    ------- ------ -----   ---- --  --------   ----------- 
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        <Extension> Public Function CrossFields(s As String) As Integer()
            Dim sps As String() = Regex.Matches(s, "\s+").ToArray
            Dim lens As String() = Regex.Matches(s, "-+").ToArray
            Dim fieldLens As New List(Of Integer)

            For i As Integer = 0 To lens.Length - 1
                fieldLens += sps(i).Length
                fieldLens += lens(i).Length
            Next

            Dim fields As Integer() = fieldLens.ToArray
            Return fields
        End Function

        ''' <summary>
        ''' Parsing a line of string into several fields fragments based on the fields length.
        ''' </summary>
        ''' <param name="s">The input text line.</param>
        ''' <param name="pos">The text length of each field property value.</param>
        ''' <returns></returns>
        <Extension> Public Function FieldParser(s As String, pos As Integer()) As String()
            Dim list As New List(Of String)
            Dim offset As Integer

            For Each len As Integer In pos.Take(pos.Length - 1)
                list += s.Substring(offset, len)  ' 起始的位置是根据域的长度逐步叠加的
                offset += len
            Next

            list += s.Substring(offset)

            Return list.ToArray
        End Function

        ''' <summary>
        ''' Condition for continue move the parser pointer.
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Delegate Function DoContinute(s As String) As Boolean

        ''' <summary>
        ''' Parsing the document head section from the document.
        ''' </summary>
        ''' <param name="buf"></param>
        ''' <param name="offset">
        ''' This function will returns the new offset value from this reference parameter.
        ''' (从这里向调用者返回偏移量)
        ''' </param>
        ''' <param name="__isHead">Condition for continue move the parser pointer to the next line.</param>
        ''' <returns></returns>
        <Extension>
        Public Function ReadHead(buf As String(), ByRef offset As Integer, __isHead As DoContinute) As String()
            Do While __isHead(buf.Read(offset))
            Loop

            Dim copy As String() = New String(offset - 1) {}
            Call Array.ConstrainedCopy(buf, Scan0, copy, Scan0, offset)
            Return copy
        End Function

        Public Function UntilBlank(s As String) As Boolean
            Return Not s.IsBlank
        End Function
    End Module
End Namespace