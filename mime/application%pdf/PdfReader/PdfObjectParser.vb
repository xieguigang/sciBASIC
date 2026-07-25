' ============================================================================
'  PdfObjectParser.vb  -  递归下降对象解析器
'  ----------------------------------------------------------------------------
'  基于词法器输出的 Token，递归构造 PDF 对象树。
'  关键点：
'    - 数字后若紧跟 "num gen R" 则构造 PdfReference
'    - 字典 << ... >> 之后若紧跟 stream 关键字，则读取流二进制数据
'    - 流的 /Length 若为直接数字则按长度读取；否则扫描至 endstream
' ============================================================================

Public Class PdfObjectParser
    Private ReadOnly _lexer As PdfLexer

    Public Sub New(lexer As PdfLexer)
        _lexer = lexer
    End Sub

    ''' <summary>从当前词法位置解析一个 PDF 对象。</summary>
    Public Function ParseObject() As PdfObject
        Dim token = _lexer.NextToken()
        Return ParseObjectFromToken(token)
    End Function

    Private Function ParseObjectFromToken(token As PdfToken) As PdfObject
        Select Case token.Type
            Case PdfTokenType.Number
                ' 可能是引用：num gen R
                Dim savedPos = _lexer.Position
                Dim nextTok = _lexer.NextToken()
                If nextTok.Type = PdfTokenType.Number Then
                    Dim thirdTok = _lexer.NextToken()
                    If thirdTok.Type = PdfTokenType.Keyword AndAlso thirdTok.TextValue = "R" Then
                        Return New PdfReference(CInt(token.NumberValue), CInt(nextTok.NumberValue))
                    End If
                    ' 不是引用，回退
                    _lexer.Position = savedPos
                    Return New PdfNumber(token.NumberValue)
                Else
                    _lexer.Position = savedPos
                    Return New PdfNumber(token.NumberValue)
                End If

            Case PdfTokenType.Name
                Return New PdfName(token.TextValue)

            Case PdfTokenType.LiteralString, PdfTokenType.HexString
                Return New PdfString(token.TextValue, token.ByteValue)

            Case PdfTokenType.DictOpen
                Return ParseDictionary()

            Case PdfTokenType.ArrayOpen
                Return ParseArray()

            Case PdfTokenType.Keyword
                Select Case token.TextValue
                    Case "true" : Return New PdfBoolean(True)
                    Case "false" : Return New PdfBoolean(False)
                    Case "null" : Return PdfNull.Instance
                End Select
                Return PdfNull.Instance

            Case Else
                Return PdfNull.Instance
        End Select
    End Function

    ''' <summary>解析字典 &lt;&lt; ... &gt;&gt;，若紧跟 stream 则返回 PdfStream。</summary>
    Private Function ParseDictionary() As PdfObject
        Dim dict As New PdfDictionary()
        Do
            Dim token = _lexer.NextToken()
            If token.Type = PdfTokenType.DictClose Then
                ' 检查是否紧跟 stream
                Dim peek = _lexer.NextToken()
                If peek.Type = PdfTokenType.Stream Then
                    Dim data = ReadStreamBytes(dict)
                    ' 跳过 endstream
                    SkipToEndStream()
                    Return New PdfStream(dict, data)
                Else
                    _lexer.Position = CInt(peek.Position)
                End If
                Exit Do
            End If
            If token.Type = PdfTokenType.EOF Then Exit Do
            If token.Type <> PdfTokenType.Name Then Exit Do
            Dim name = token.TextValue
            Dim value = ParseObject()
            dict.Add(name, value)
        Loop
        Return dict
    End Function

    ''' <summary>读取流二进制数据：优先使用 /Length，否则扫描 endstream。</summary>
    Private Function ReadStreamBytes(dict As PdfDictionary) As Byte()
        Dim lengthObj = dict.Get("Length")
        If TypeOf lengthObj Is PdfNumber Then
            Dim len = CInt(DirectCast(lengthObj, PdfNumber).Value)
            Dim data = _lexer.ReadStreamData(len)
            ' 校验：若读取后位置不在 endstream 附近，回退到扫描模式
            Return data
        ElseIf TypeOf lengthObj Is PdfReference Then
            ' /Length 为引用，解析时对象表尚未建立，回退到扫描模式
            Return _lexer.ReadStreamDataScan()
        Else
            Return _lexer.ReadStreamDataScan()
        End If
    End Function

    Private Sub SkipToEndStream()
        Dim tok = _lexer.NextToken()
        While tok.Type <> PdfTokenType.EndStream AndAlso tok.Type <> PdfTokenType.EOF
            tok = _lexer.NextToken()
        End While
    End Sub

    Private Function ParseArray() As PdfArray
        Dim arr As New PdfArray()
        Do
            Dim token = _lexer.NextToken()
            If token.Type = PdfTokenType.ArrayClose Then Exit Do
            If token.Type = PdfTokenType.EOF Then Exit Do
            Dim obj = ParseObjectFromToken(token)
            arr.Add(obj)
        Loop
        Return arr
    End Function

End Class

