Imports System.Collections.Generic
Imports System.Text

Namespace org.nlp.util

    ''' <summary>
    ''' Created by fangy on 13-12-13.
    ''' 分割和输出记号
    ''' </summary>
    Public Class Tokenizer

        '    private String[] tokens;
        '    int currentIndex;
        Private tokens As IList(Of String)
        Private tokenIter As IEnumerator(Of String)

        Public Sub New()
            tokens = New LinkedList(Of String)()
            'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
            tokenIter = tokens.GetEnumerator()
        End Sub

        ''' <summary>
        ''' 分割一段文本，得到一列记号 </summary>
        ''' <param name="text"> 文本 </param>
        ''' <param name="delim"> 分割符 </param>
        Public Sub New(text As String, delim As String)
            tokens = Arrays.asList(text.Split(delim, True))
            'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
            tokenIter = tokens.GetEnumerator()
        End Sub

        ''' <summary>
        ''' 获得记号的数量 </summary>
        ''' <returns> 数量 </returns>
        Public Overridable Function size() As Integer
            Return tokens.Count
        End Function

        ''' <summary>
        ''' 遍历记号时，查询是否还有记号未遍历 </summary>
        ''' <returns> 若还有记号未遍历，则返回true，否则返回false。 </returns>
        Public Overridable Function hasMoreTokens() As Boolean
            'JAVA TO C# CONVERTER CRACKED BY X-CRACKER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            Return tokenIter.hasNext()
        End Function

        ''' <summary>
        ''' 遍历记号时获得下一个之前未遍历的记号 </summary>
        ''' <returns> 记号 </returns>
        Public Overridable Function nextToken() As String

            'JAVA TO C# CONVERTER CRACKED BY X-CRACKER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            Return tokenIter.[next]()
        End Function

        ''' <summary>
        ''' 向原有记号序列的末尾添加一个记号 </summary>
        ''' <param name="token"> 待添加的记号 </param>
        Public Overridable Sub add(token As String)
            If ReferenceEquals(token, Nothing) Then
                Return
            End If

            tokens.Add(token)
        End Sub

        ''' <summary>
        ''' 以分割符连接记号并输出 </summary>
        ''' <param name="delim"> 分割符 </param>
        ''' <returns> 记号由分割符连接的字符串 </returns>
        Public Overridable Function ToString(delim As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            If tokens.Count < 1 Then
                Return sb.ToString()
            End If
            'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
            Dim tempTokenIter As IEnumerator(Of String) = tokens.GetEnumerator()
            'JAVA TO C# CONVERTER CRACKED BY X-CRACKER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            sb.Append(tempTokenIter.[next]())

            While tempTokenIter.MoveNext()
                sb.Append(" ").Append(tempTokenIter.Current)
            End While

            Return sb.ToString()
        End Function
    End Class
End Namespace
