' ============================================================================
' OMML.vb - 数学表达式树 → OMML (Office Math Markup Language) 序列化器
'
' 将 xml-netcore5 项目 MathML 命名空间解析产出的 MathExpression 表达式树
' （LambdaExpression.FromMathML 的结果）序列化为 Word 原生公式对象 XML：
'
'   MathML 字符串 --ContentBuilder.ParseXml--> MathExpression 树 --本模块--> m:oMath
'
' 结构映射：
'   SymbolExpression          -> m:r（Cambria Math 数学字体）
'   符号文本含 "_" / "^" 约定  -> m:sSub（下标，U_11 => U₁₁）/ m:sSup（上标，x^2 => x²）
'   BinaryExpression.divide   -> m:f 竖式分式（num/den）
'   BinaryExpression.power    -> m:sSup 上标
'   plus/minus/times/eq/...   -> 内联运算符 m:r
'   MathFunctionExpression    -> m:func（fName + m:d 括号定界参数）
'
' 嵌套括号策略：当子表达式的优先级低于父运算符（或同级且位于减/除/幂的右操作数）
' 时，用 m:d 圆括号定界符包裹，保证语义与书写习惯一致。
'
' 已知边界：Content MathML 的矩阵、n 元求和/积分上下限、分段函数等不在当前
' 表达式树的表达能力内，如需支持应先扩展 MathML 解析层再在本模块添加映射。
' ============================================================================

Imports System.Text
Imports Microsoft.VisualBasic.MIME.application.xml.MathML

''' <summary>
''' OMML（Office Math Markup Language）序列化器。
''' </summary>
Public Module OmmlBuilder

    ' 数学公式统一使用 Cambria Math 字体（Word 内置数学字体，保证公式渲染一致）
    Private Const MathFont As String = "Cambria Math"

    ''' <summary>
    ''' 将 LambdaExpression（MathML 解析结果）序列化为 &lt;m:oMath&gt; 片段。
    ''' 注意：lambda 的 bvar 参数列表在无函数名的情况下无法正确表达，
    ''' 此处仅输出表达式体。
    ''' </summary>
    ''' <param name="lambda">MathML 解析得到的 lambda 表达式。</param>
    ''' <param name="fontSizePt">公式字号（磅）；0 表示不指定（继承文档默认样式）。</param>
    Public Function ToOmml(lambda As LambdaExpression, Optional fontSizePt As Double = 0) As String
        If lambda Is Nothing OrElse lambda.lambda Is Nothing Then
            Throw New ArgumentException("Empty math expression: the MathML content has no apply body.")
        End If

        Return ToOmml(lambda.lambda, fontSizePt)
    End Function

    ''' <summary>将表达式树序列化为 &lt;m:oMath&gt; 片段。</summary>
    Public Function ToOmml(exp As MathExpression, Optional fontSizePt As Double = 0) As String
        Dim sb As New StringBuilder()
        Call WriteExpression(exp, sb, 0, False, fontSizePt)
        Return sb.ToString()
    End Function

    ' ========================================================================
    ' 表达式分派
    ' ========================================================================

    Private Sub WriteExpression(exp As MathExpression,
                                sb As StringBuilder,
                                parentPrec As Integer,
                                isRightOperand As Boolean,
                                fontSizePt As Double)

        If TypeOf exp Is BinaryExpression Then
            Call WriteBinary(DirectCast(exp, BinaryExpression), sb, parentPrec, isRightOperand, fontSizePt)
        ElseIf TypeOf exp Is SymbolExpression Then
            Call WriteSymbol(DirectCast(exp, SymbolExpression), sb, fontSizePt)
        ElseIf TypeOf exp Is MathFunctionExpression Then
            Call WriteFunction(DirectCast(exp, MathFunctionExpression), sb, fontSizePt)
        Else
            Throw New NotImplementedException($"Unsupported math expression node: {exp?.GetType().Name}")
        End If
    End Sub

    ' ========================================================================
    ' 运算符与优先级
    ' ========================================================================

    ''' <summary>
    ''' 运算符优先级：关系符(0) &lt; 加减(1) &lt; 乘除(2) &lt; 幂(3)。
    ''' </summary>
    Private Function precedence([operator] As String) As Integer
        Select Case [operator]
            Case "eq", "leq", "geq", "neq" : Return 0
            Case "plus", "minus" : Return 1
            Case "times", "divide" : Return 2
            Case "power" : Return 3
            Case Else : Return 1
        End Select
    End Function

    ''' <summary>运算符的显示符号（Unicode 数学符号）。</summary>
    Private Function operatorSymbol([operator] As String) As String
        Select Case [operator]
            Case "plus" : Return "+"
            Case "minus" : Return "−"    ' U+2212 数学减号
            Case "times" : Return "×"
            Case "eq" : Return "="
            Case "leq" : Return "≤"
            Case "geq" : Return "≥"
            Case "neq" : Return "≠"
            Case Else : Return [operator]
        End Select
    End Function

    ''' <summary>
    ''' 判断子表达式是否需要圆括号定界（m:d）：
    ''' 子优先级低于父运算符，或同级且位于减/除/幂的右操作数（非结合运算）时需要。
    ''' </summary>
    Private Function needsParens(child As MathExpression, parentOp As String, isRightOperand As Boolean) As Boolean
        Dim bin = TryCast(child, BinaryExpression)

        If bin Is Nothing Then
            ' 函数应用与符号原子不需要括号
            Return False
        End If

        Dim childPrec As Integer = precedence(bin.operator)
        Dim parentPrec As Integer = precedence(parentOp)

        If childPrec < parentPrec Then
            Return True
        End If

        If childPrec = parentPrec Then
            If parentOp = "minus" OrElse parentOp = "divide" Then
                ' (a - b) - c 可省略括号；a - (b - c) 必须保留
                Return isRightOperand
            ElseIf parentOp = "power" Then
                ' 幂的底数/指数为同级幂时加括号：(a^b)^c
                Return True
            End If
        End If

        Return False
    End Function

    ' ========================================================================
    ' 各节点类型的 OMML 输出
    ' ========================================================================

    ''' <summary>二元运算：divide → 竖式分式 m:f；power → 上标 m:sSup；其余 → 内联运算符 run。</summary>
    Private Sub WriteBinary(b As BinaryExpression,
                            sb As StringBuilder,
                            parentPrec As Integer,
                            isRightOperand As Boolean,
                            fontSizePt As Double)

        ' 除法：竖式分式（m:num / m:den 自带分组语义，操作数无需括号）
        If b.operator = "divide" Then
            sb.Append("<m:f><m:num>")
            Call WriteExpression(b.applyleft, sb, 0, False, fontSizePt)
            sb.Append("</m:num><m:den>")
            Call WriteExpression(b.applyright, sb, 0, False, fontSizePt)
            sb.Append("</m:den></m:f>")
            Return
        End If

        ' 幂：上标结构 m:sSup
        If b.operator = "power" Then
            sb.Append("<m:sSup><m:e>")
            If needsParens(b.applyleft, b.operator, False) Then
                Call WriteDelimited(b.applyleft, sb, fontSizePt)
            Else
                Call WriteExpression(b.applyleft, sb, precedence(b.operator), False, fontSizePt)
            End If
            sb.Append("</m:e><m:sup>")
            Call WriteExpression(b.applyright, sb, 0, False, fontSizePt)
            sb.Append("</m:sup></m:sSup>")
            Return
        End If

        ' 内联二元/关系运算符：left op right
        Dim prec As Integer = precedence(b.operator)

        If needsParens(b.applyleft, b.operator, False) Then
            Call WriteDelimited(b.applyleft, sb, fontSizePt)
        Else
            Call WriteExpression(b.applyleft, sb, prec, False, fontSizePt)
        End If

        Call sb.Append(MathRun(operatorSymbol(b.operator), fontSizePt))

        If needsParens(b.applyright, b.operator, True) Then
            Call WriteDelimited(b.applyright, sb, fontSizePt)
        Else
            Call WriteExpression(b.applyright, sb, prec, True, fontSizePt)
        End If
    End Sub

    ''' <summary>
    ''' 符号/数值 run。符号文本支持上下标约定：
    '''   "U_11" -> m:sSub（U₁₁）；"x^2" -> m:sSup（x²）。
    ''' 仅拆分第一个分隔符（下/上标内容内部不再递归拆分）。
    ''' </summary>
    Private Sub WriteSymbol(sym As SymbolExpression, sb As StringBuilder, fontSizePt As Double)
        Dim text As String = If(sym?.text, "")
        Dim iSub As Integer = text.IndexOf("_"c)
        Dim iSup As Integer = text.IndexOf("^"c)
        Dim iBaseSep As Integer = -1
        Dim isSub As Boolean = False

        If iSub >= 0 AndAlso (iSup < 0 OrElse iSub < iSup) Then
            iBaseSep = iSub
            isSub = True
        ElseIf iSup >= 0 Then
            iBaseSep = iSup
            isSub = False
        End If

        If iBaseSep <= 0 OrElse iBaseSep = text.Length - 1 Then
            ' 无上下标（或分隔符位于边界，按字面输出）
            Call sb.Append(MathRun(text, fontSizePt))
            Return
        End If

        Dim baseText As String = text.Substring(0, iBaseSep)
        Dim scriptText As String = text.Substring(iBaseSep + 1)

        If isSub Then
            sb.Append("<m:sSub><m:e>")
            Call sb.Append(MathRun(baseText, fontSizePt))
            sb.Append("</m:e><m:sub>")
            Call sb.Append(MathRun(scriptText, fontSizePt))
            sb.Append("</m:sub></m:sSub>")
        Else
            sb.Append("<m:sSup><m:e>")
            Call sb.Append(MathRun(baseText, fontSizePt))
            sb.Append("</m:e><m:sup>")
            Call sb.Append(MathRun(scriptText, fontSizePt))
            sb.Append("</m:sup></m:sSup>")
        End If
    End Sub

    ''' <summary>
    ''' 数学函数应用：m:func（fName + 圆括号定界的参数列表）。
    ''' 多参数（如 max/min）由 m:d 的多个 m:e 子元素承载，Word 自动以逗号分隔。
    ''' </summary>
    Private Sub WriteFunction(f As MathFunctionExpression, sb As StringBuilder, fontSizePt As Double)
        sb.Append("<m:func><m:fName>")
        Call sb.Append(MathRun(If(f.name, "f"), fontSizePt))
        sb.Append("</m:fName><m:e><m:d>")

        If f.parameters Is Nothing OrElse f.parameters.Length = 0 Then
            Call sb.Append(MathRun("", fontSizePt))
        Else
            For Each p As MathExpression In f.parameters
                sb.Append("<m:e>")
                Call WriteExpression(p, sb, 0, False, fontSizePt)
                sb.Append("</m:e>")
            Next
        End If

        sb.Append("</m:d></m:e></m:func>")
    End Sub

    ''' <summary>用圆括号定界符（m:d）包裹子表达式。</summary>
    Private Sub WriteDelimited(exp As MathExpression, sb As StringBuilder, fontSizePt As Double)
        sb.Append("<m:d><m:e>")
        Call WriteExpression(exp, sb, 0, False, fontSizePt)
        sb.Append("</m:e></m:d>")
    End Sub

    ' ========================================================================
    ' 数学 run
    ' ========================================================================

    ''' <summary>
    ''' 数学 run：&lt;m:r&gt;&lt;w:rPr&gt;Cambria Math 字体&lt;/w:rPr&gt;&lt;m:t&gt;text&lt;/m:t&gt;&lt;/m:r&gt;。
    ''' </summary>
    ''' <param name="text">数学文本（内部已做 XML 转义）。</param>
    ''' <param name="fontSizePt">字号（磅）；0 表示不指定（继承文档默认）。</param>
    Private Function MathRun(text As String, fontSizePt As Double) As String
        Dim sz As String = ""

        If fontSizePt > 0 Then
            Dim halfPoints As Integer = CInt(fontSizePt * 2)
            sz = $"<w:sz w:val=""{halfPoints}""/><w:szCs w:val=""{halfPoints}""/>"
        End If

        Return $"<m:r><w:rPr><w:rFonts w:ascii=""{MathFont}"" w:hAnsi=""{MathFont}""/>{sz}</w:rPr>" &
               $"<m:t xml:space=""preserve"">{XEsc(If(text, ""))}</m:t></m:r>"
    End Function

    ''' <summary>XML 文本转义。</summary>
    Private Function XEsc(text As String) As String
        If String.IsNullOrEmpty(text) Then Return ""
        Return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("""", "&quot;")
    End Function

End Module
