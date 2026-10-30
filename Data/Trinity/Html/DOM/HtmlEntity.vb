Imports System.Collections.Generic
Imports System.Text

Namespace Html

    ''' <summary>
    ''' HTML 字符实体的编解码模块
    ''' </summary>
    ''' <remarks>
    ''' 支持常见的命名实体(例如 ``&amp;amp;`` ``&amp;nbsp;``)、十进制数值实体(``&amp;#183;``)
    ''' 以及十六进制数值实体(``&amp;#x00B7;``)。
    ''' </remarks>
    Public Module HtmlEntity

        ''' <summary>
        ''' 常见的命名实体表
        ''' </summary>
        Private ReadOnly entities As New Dictionary(Of String, String)(StringComparer.Ordinal) From {
            {"amp", "&"}, {"lt", "<"}, {"gt", ">"}, {"quot", """"}, {"apos", "'"},
            {"nbsp", ChrW(160)}, {"iexcl", ChrW(161)}, {"cent", ChrW(162)}, {"pound", ChrW(163)},
            {"curren", ChrW(164)}, {"yen", ChrW(165)}, {"brvbar", ChrW(166)}, {"sect", ChrW(167)},
            {"copy", ChrW(169)}, {"ordf", ChrW(170)}, {"laquo", ChrW(171)}, {"not", ChrW(172)},
            {"reg", ChrW(174)}, {"macr", ChrW(175)}, {"deg", ChrW(176)}, {"plusmn", ChrW(177)},
            {"sup2", ChrW(178)}, {"sup3", ChrW(179)}, {"acute", ChrW(180)}, {"micro", ChrW(181)},
            {"para", ChrW(182)}, {"middot", ChrW(183)}, {"cedil", ChrW(184)}, {"sup1", ChrW(185)},
            {"ordm", ChrW(186)}, {"raquo", ChrW(187)}, {"frac14", ChrW(188)}, {"frac12", ChrW(189)},
            {"frac34", ChrW(190)}, {"iquest", ChrW(191)}, {"times", ChrW(215)}, {"divide", ChrW(247)},
            {"bull", ChrW(8226)}, {"hellip", ChrW(8230)}, {"prime", ChrW(8242)}, {"Prime", ChrW(8243)},
            {"oline", ChrW(8254)}, {"frasl", ChrW(8260)}, {"weierp", ChrW(8472)}, {"image", ChrW(8465)},
            {"real", ChrW(8476)}, {"trade", ChrW(8482)}, {"alefsym", ChrW(8501)}, {"larr", ChrW(8592)},
            {"uarr", ChrW(8593)}, {"rarr", ChrW(8594)}, {"darr", ChrW(8595)}, {"harr", ChrW(8596)},
            {"crarr", ChrW(8629)}, {"lArr", ChrW(8656)}, {"uArr", ChrW(8657)}, {"rArr", ChrW(8658)},
            {"dArr", ChrW(8659)}, {"hArr", ChrW(8660)}, {"forall", ChrW(8704)}, {"part", ChrW(8706)},
            {"exist", ChrW(8707)}, {"empty", ChrW(8709)}, {"nabla", ChrW(8711)}, {"isin", ChrW(8712)},
            {"notin", ChrW(8713)}, {"ni", ChrW(8715)}, {"prod", ChrW(8719)}, {"sum", ChrW(8721)},
            {"minus", ChrW(8722)}, {"lowast", ChrW(8727)}, {"radic", ChrW(8730)}, {"prop", ChrW(8733)},
            {"infin", ChrW(8734)}, {"ang", ChrW(8736)}, {"and", ChrW(8743)}, {"or", ChrW(8744)},
            {"cap", ChrW(8745)}, {"cup", ChrW(8746)}, {"int", ChrW(8747)}, {"there4", ChrW(8756)},
            {"sim", ChrW(8764)}, {"cong", ChrW(8773)}, {"asymp", ChrW(8776)}, {"ne", ChrW(8800)},
            {"equiv", ChrW(8801)}, {"le", ChrW(8804)}, {"ge", ChrW(8805)}, {"sub", ChrW(8834)},
            {"sup", ChrW(8835)}, {"nsub", ChrW(8836)}, {"sube", ChrW(8838)}, {"supe", ChrW(8839)},
            {"oplus", ChrW(8853)}, {"otimes", ChrW(8855)}, {"perp", ChrW(8869)}, {"sdot", ChrW(8901)},
            {"lceil", ChrW(8968)}, {"rceil", ChrW(8969)}, {"lfloor", ChrW(8970)}, {"rfloor", ChrW(8971)},
            {"lang", ChrW(9001)}, {"rang", ChrW(9002)}, {"loz", ChrW(9674)}, {"spades", ChrW(9824)},
            {"clubs", ChrW(9827)}, {"hearts", ChrW(9829)}, {"diams", ChrW(9830)},
            {"mdash", ChrW(8212)}, {"ndash", ChrW(8211)},
            {"lsquo", ChrW(8216)}, {"rsquo", ChrW(8217)}, {"sbquo", ChrW(8218)},
            {"ldquo", ChrW(8220)}, {"rdquo", ChrW(8221)}, {"bdquo", ChrW(8222)},
            {"dagger", ChrW(8224)}, {"Dagger", ChrW(8225)}, {"permil", ChrW(8240)},
            {"lsaquo", ChrW(8249)}, {"rsaquo", ChrW(8250)}, {"euro", ChrW(8364)},
            {"alpha", ChrW(945)}, {"beta", ChrW(946)}, {"gamma", ChrW(947)}, {"delta", ChrW(948)},
            {"epsilon", ChrW(949)}, {"zeta", ChrW(950)}, {"eta", ChrW(951)}, {"theta", ChrW(952)},
            {"iota", ChrW(953)}, {"kappa", ChrW(954)}, {"lambda", ChrW(955)}, {"mu", ChrW(956)},
            {"nu", ChrW(957)}, {"xi", ChrW(958)}, {"omicron", ChrW(959)}, {"pi", ChrW(960)},
            {"rho", ChrW(961)}, {"sigmaf", ChrW(962)}, {"sigma", ChrW(963)}, {"tau", ChrW(964)},
            {"upsilon", ChrW(965)}, {"phi", ChrW(966)}, {"chi", ChrW(967)}, {"psi", ChrW(968)},
            {"omega", ChrW(969)}, {"thetasym", ChrW(977)}, {"upsih", ChrW(978)}, {"piv", ChrW(982)},
            {"Alpha", ChrW(913)}, {"Beta", ChrW(914)}, {"Gamma", ChrW(915)}, {"Delta", ChrW(916)},
            {"Epsilon", ChrW(917)}, {"Zeta", ChrW(918)}, {"Eta", ChrW(919)}, {"Theta", ChrW(920)},
            {"Iota", ChrW(921)}, {"Kappa", ChrW(922)}, {"Lambda", ChrW(923)}, {"Mu", ChrW(924)},
            {"Nu", ChrW(925)}, {"Xi", ChrW(926)}, {"Omicron", ChrW(927)}, {"Pi", ChrW(928)},
            {"Rho", ChrW(929)}, {"Sigma", ChrW(931)}, {"Tau", ChrW(932)}, {"Upsilon", ChrW(933)},
            {"Phi", ChrW(934)}, {"Chi", ChrW(935)}, {"Psi", ChrW(936)}, {"Omega", ChrW(937)}
        }

        ''' <summary>
        ''' 将文本之中的 HTML 字符实体解码为对应的字符
        ''' </summary>
        ''' <param name="text">可能包含字符实体的文本</param>
        ''' <returns>解码之后的文本；无法识别的实体会被原样的保留下来</returns>
        Public Function Decode(text As String) As String
            If String.IsNullOrEmpty(text) Then
                Return ""
            End If
            If text.IndexOf("&"c) < 0 Then
                Return text
            End If

            Dim sb As New StringBuilder()
            Dim i As Integer = 0
            Dim n As Integer = text.Length

            While i < n
                Dim ch As Char = text(i)

                If ch <> "&"c Then
                    Call sb.Append(ch)
                    i += 1
                    Continue While
                End If

                Dim semi As Integer = text.IndexOf(";"c, i + 1)

                ' 实体名不会太长，超过这个长度就不认为是一个实体了
                If semi < 0 OrElse semi - i > 12 Then
                    Call sb.Append(ch)
                    i += 1
                    Continue While
                End If

                Dim body As String = text.Substring(i + 1, semi - i - 1)
                Dim decoded As String = Nothing

                If body.Length > 0 AndAlso body(0) = "#"c Then
                    decoded = DecodeNumeric(body)
                ElseIf Not entities.TryGetValue(body, decoded) Then
                    decoded = Nothing
                End If

                If decoded Is Nothing Then
                    Call sb.Append(ch)
                    i += 1
                Else
                    Call sb.Append(decoded)
                    i = semi + 1
                End If
            End While

            Return sb.ToString()
        End Function

        ''' <summary>
        ''' 解码数值形式的字符实体，例如 ``&amp;#183;`` 或者 ``&amp;#x00B7;``
        ''' </summary>
        Private Function DecodeNumeric(body As String) As String
            If body.Length < 2 Then
                Return Nothing
            End If

            Dim code As Integer

            Try
                If body(1) = "x"c OrElse body(1) = "X"c Then
                    code = Convert.ToInt32(body.Substring(2), 16)
                Else
                    code = Convert.ToInt32(body.Substring(1), 10)
                End If
            Catch ex As Exception
                Return Nothing
            End Try

            If code <= 0 OrElse code > &H10FFFF Then
                Return Nothing
            End If
            If code >= &HD800 AndAlso code <= &HDFFF Then
                ' 单独的代理项字符不是一个合法的 unicode 码点
                Return ""
            End If

            Return Char.ConvertFromUtf32(code)
        End Function

        ''' <summary>
        ''' 将文本之中的 HTML 保留字符编码为字符实体
        ''' </summary>
        ''' <param name="text">原始文本</param>
        ''' <returns>编码之后的文本，可以安全的嵌入到 HTML 文档之中</returns>
        Public Function Encode(text As String) As String
            If String.IsNullOrEmpty(text) Then
                Return ""
            End If

            Dim sb As New StringBuilder()

            For Each ch As Char In text
                Select Case ch
                    Case "&"c
                        Call sb.Append("&amp;")
                    Case "<"c
                        Call sb.Append("&lt;")
                    Case ">"c
                        Call sb.Append("&gt;")
                    Case """"c
                        Call sb.Append("&quot;")
                    Case "'"c
                        Call sb.Append("&#39;")
                    Case Else
                        Call sb.Append(ch)
                End Select
            Next

            Return sb.ToString()
        End Function
    End Module

End Namespace
