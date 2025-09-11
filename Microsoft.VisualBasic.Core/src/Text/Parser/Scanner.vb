Imports System.IO
Imports System.Text.RegularExpressions

Namespace Text.Parser

    ''' <summary>
    ''' 模拟Java Scanner类功能的VB.NET类。
    ''' 注意：此类未实现Java Scanner的所有方法，仅提供核心功能。
    ''' </summary>
    Public Class Scanner
        Implements IDisposable

        Private _reader As TextReader
        Private _delimiter As String = "\s+" ' 默认分隔符为空白符（正则表达式）
        Private _peekedLine As String = Nothing ' 用于 peek 下一行
        Private _currentTokens() As String = Nothing ' 当前行拆分后的令牌数组
        Private _currentTokenIndex As Integer = -1 ' 当前令牌索引

        ''' <summary>
        ''' 构造函数，基于TextReader创建VBNetScanner。
        ''' </summary>
        ''' <param name="reader">输入的TextReader对象</param>
        Public Sub New(reader As TextReader)
            _reader = reader
        End Sub

        ''' <summary>
        ''' 从控制台标准输入创建VBNetScanner。
        ''' </summary>
        Public Sub New()
            _reader = Console.In
        End Sub

        ''' <summary>
        ''' 从字符串创建VBNetScanner。
        ''' </summary>
        ''' <param name="input">输入的字符串</param>
        Public Sub New(input As String)
            _reader = New StringReader(input)
        End Sub

        ''' <summary>
        ''' 设置分隔符（支持正则表达式）。
        ''' </summary>
        ''' <param name="pattern">分隔符的正则表达式模式</param>
        ''' <returns>返回当前VBNetScanner实例，支持链式调用</returns>
        Public Function UseDelimiter(pattern As String) As Scanner
            _delimiter = pattern
            Return Me
        End Function

        ''' <summary>
        ''' 检查是否还有下一个令牌（使用当前分隔符）。
        ''' </summary>
        Public Function HasNext() As Boolean
            ' 如果当前行没有令牌或已读完，尝试读取下一行
            If _currentTokens Is Nothing OrElse _currentTokenIndex >= _currentTokens.Length - 1 Then
                If Not ReadNextLine() Then
                    Return False
                End If
            Else
                ' 当前行还有令牌
                Return True
            End If
            ' 检查新读取的行是否有令牌
            Return _currentTokens IsNot Nothing AndAlso _currentTokens.Length > 0
        End Function

        ''' <summary>
        ''' 读取下一个令牌（字符串）。
        ''' </summary>
        Public Function [Next]() As String
            If Not HasNext() Then
                Throw New InvalidOperationException("没有更多的令牌可读")
            End If

            _currentTokenIndex += 1
            Return _currentTokens(_currentTokenIndex)
        End Function

        ''' <summary>
        ''' 检查是否还有下一行。
        ''' </summary>
        Public Function HasNextLine() As Boolean
            If _peekedLine IsNot Nothing Then
                Return True
            End If

            _peekedLine = _reader.ReadLine()
            Return _peekedLine IsNot Nothing
        End Function

        ''' <summary>
        ''' 读取下一行内容。
        ''' </summary>
        Public Function NextLine() As String
            If _peekedLine IsNot Nothing Then
                Dim line As String = _peekedLine
                _peekedLine = Nothing
                ' 读取新行后，需要重置令牌状态
                _currentTokens = Nothing
                _currentTokenIndex = -1
                Return line
            End If

            Dim lineRead = _reader.ReadLine()
            ' 读取新行后，需要重置令牌状态
            _currentTokens = Nothing
            _currentTokenIndex = -1
            Return lineRead
        End Function

        ''' <summary>
        ''' 读取下一个整数。
        ''' </summary>
        Public Function NextInt() As Integer
            Dim token = [Next]()
            Dim result As Integer
            If Integer.TryParse(token, result) Then
                Return result
            End If
            Throw New FormatException($"令牌 '{token}' 无法转换为整数")
        End Function

        ''' <summary>
        ''' 检查是否还有下一个整数。
        ''' </summary>
        Public Function HasNextInt() As Boolean
            If Not HasNext() Then Return False

            '  peek 下一个令牌而不消耗它
            Dim nextTokenIndex = If(_currentTokenIndex + 1 < _currentTokens.Length, _currentTokenIndex + 1, 0)
            Dim token = _currentTokens(nextTokenIndex)
            Dim testInt As Integer
            Return Integer.TryParse(token, testInt)
        End Function

        ''' <summary>
        ''' 读取下一个双精度浮点数。
        ''' </summary>
        Public Function NextDouble() As Double
            Dim token = [Next]()
            Dim result As Double
            If Double.TryParse(token, result) Then
                Return result
            End If
            Throw New FormatException($"令牌 '{token}' 无法转换为双精度浮点数")
        End Function

        ''' <summary>
        ''' 检查是否还有下一个双精度浮点数。
        ''' </summary>
        Public Function HasNextDouble() As Boolean
            If Not HasNext() Then Return False

            '  peek 下一个令牌而不消耗它
            Dim nextTokenIndex = If(_currentTokenIndex + 1 < _currentTokens.Length, _currentTokenIndex + 1, 0)
            Dim token = _currentTokens(nextTokenIndex)
            Dim testDouble As Double
            Return Double.TryParse(token, testDouble)
        End Function

        ''' <summary>
        ''' 关闭扫描器并释放资源。
        ''' </summary>
        Public Sub Close()
            Dispose()
        End Sub

        ''' <summary>
        ''' 释放资源。
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            If _reader IsNot Nothing Then
                _reader.Dispose()
                _reader = Nothing
            End If
        End Sub

        ' --- 私有辅助方法 ---
        ''' <summary>
        ''' 尝试读取下一行并按分隔符拆分。
        ''' </summary>
        Private Function ReadNextLine() As Boolean
            Dim line As String = NextLine() ' 这会消耗一行并重置令牌状态
            If line Is Nothing Then
                _currentTokens = Nothing
                Return False
            End If

            ' 使用正则表达式拆分，以支持更复杂的分隔符
            _currentTokens = Regex.Split(line, _delimiter)
            ' 移除空字符串（例如由连续分隔符导致）
            _currentTokens = _currentTokens.Where(Function(s) Not String.IsNullOrEmpty(s)).ToArray()
            _currentTokenIndex = -1 ' 重置为第一个令牌之前
            Return _currentTokens.Length > 0
        End Function
    End Class
End Namespace