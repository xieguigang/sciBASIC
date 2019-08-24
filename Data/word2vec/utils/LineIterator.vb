Imports System.Collections.Generic

Namespace org.nlp.util

    ''' <summary>
    ''' Created by fangy on 13-12-19.
    ''' 输入流的行迭代器
    ''' 改造自org.apache.commons.io。LineIterator
    ''' </summary>
    Public Class LineIterator
        Implements IEnumerator(Of String)

        ''' <summary>
        ''' 缓存输入流 </summary>
        Private ReadOnly bufferedReader As StreamReader
        ''' <summary>
        ''' 当前读取的行 </summary>
        Private cachedLine As String
        ''' <summary>
        ''' 标识输入流是否已经读入完. </summary>
        Private finished As Boolean = False

        ''' <summary>
        ''' 构造函数
        ''' </summary>
        ''' <param name="reader"> 将要读取的输入流，不能为null </param>
        ''' <exceptioncref="IllegalArgumentException"> 当reader为null时抛出 </exception>
        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public LineIterator(final Reader reader) throws IllegalArgumentException
        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: 'final' parameters are not available in .NET:
        Public Sub New(ByVal reader As Reader)
            If reader Is Nothing Then
                Throw New ArgumentException("输入流不可为null")
            End If

            If TypeOf reader Is StreamReader Then
                bufferedReader = CType(reader, StreamReader)
            Else
                bufferedReader = New StreamReader(reader)
            End If
        End Sub

        '-----------------------------------------------------------------------
        ''' <summary>
        ''' 标识输入流中是否还有行可供读入，如果程序产生了<code>IOException</code>，
        ''' close()将会被调用，以关闭输入流，并抛出<code>IllegalStateException</code>。
        ''' </summary>
        ''' <returns> 若还有行可供读入，则返回{@code true}，否则返回{@code false} </returns>
        ''' <exceptioncref="IllegalStateException"> 当有IO异常产生时 </exception>
        Public Overridable Function hasNext() As Boolean
            If Not ReferenceEquals(cachedLine, Nothing) Then
                Return True
            ElseIf finished Then
                Return False
            Else

                Try

                    While True
                        Dim line As String = bufferedReader.ReadLine()

                        If ReferenceEquals(line, Nothing) Then
                            finished = True
                            Return False
                        ElseIf isValidLine(line) Then
                            cachedLine = line
                            Return True
                        End If
                    End While

                Catch ioe As IOException
                    close()
                    Throw New InvalidOperationException(ioe)
                End Try
            End If
        End Function

        ''' <summary>
        ''' 验证字符串，这里的实现是直接返回true </summary>
        ''' <param name="line">  待验证的字符串行 </param>
        ''' <returns> 符合条件的字符串返回 {@code true}，否则返回{@code false} </returns>
        Protected Friend Overridable Function isValidLine(ByVal line As String) As Boolean
            Return True
        End Function

        ''' <summary>
        ''' 从 <code>Reader</code> 中读取一行.
        ''' </summary>
        ''' <returns> 输入流中的下一行 </returns>
        ''' <exceptioncref="NoSuchElementException"> 没有行可读入时抛出 </exception>
        Public Overridable Function [next]() As String
            Return nextLine()
        End Function

        ''' <summary>
        ''' 从 <code>Reader</code> 中读取一行
        ''' </summary>
        ''' <returns> 从输入流中读取的一行 </returns>
        ''' <exceptioncref="NoSuchElementException"> 如果没有行可读入 </exception>
        Public Overridable Function nextLine() As String
            If Not hasNext() Then
                Throw New NoSuchElementException("No more lines")
            End If

            Dim currentLine = cachedLine
            cachedLine = Nothing
            Return currentLine
        End Function

        ''' <summary>
        ''' 关闭<code>Reader</code>
        ''' 如果你只想读取一个大文件的头几行，那么这个函数可以
        ''' 帮助你关闭输入流。如果没有调用close函数，那么
        ''' <code>Reader</code>将保持打开的状态。这一方法可以
        ''' 安全地多次调用。
        ''' </summary>
        Public Overridable Sub close()
            finished = True

            Try
                bufferedReader.Close()
            Catch e As IOException
                Console.WriteLine(e.ToString())
                Console.Write(e.StackTrace)
            End Try

            cachedLine = Nothing
        End Sub

        ''' <summary>
        ''' 不支持的操作
        ''' </summary>
        ''' <exceptioncref="UnsupportedOperationException"> 每次调用都会抛出 </exception>
        Public Overridable Sub remove()
            Throw New NotSupportedException("Remove unsupported on LineIterator")
        End Sub

        '-----------------------------------------------------------------------
        ''' <summary>
        ''' 关闭迭代器中的输入流，检查是否为null，忽略异常
        ''' </summary>
        ''' <param name="iterator">  将要被关闭的迭代器 </param>
        Public Shared Sub closeQuietly(ByVal iterator As LineIterator)
            If iterator IsNot Nothing Then
                iterator.close()
            End If
        End Sub
    End Class
End Namespace
