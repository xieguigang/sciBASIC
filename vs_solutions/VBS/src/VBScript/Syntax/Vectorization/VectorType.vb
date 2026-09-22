Imports System.Text.RegularExpressions

Namespace Script

    ''' <summary>
    ''' 向量化改写期所支持的数值基元类型。
    ''' </summary>
    ''' <remarks>
    ''' 枚举取值的大小顺序即 VB 的数值提升顺序(``Short &lt; Integer &lt; Long &lt; Single &lt; Double``),
    ''' 因此「取公共类型」可以直接实现为取枚举值的较大者。
    ''' <see cref="Unknown"/> 表示无法确定(非数值类型、未知标识符、改写器不支持的数值类型)。
    ''' </remarks>
    Public Enum NumericKind
        Unknown = 0
        [Short] = 1
        [Integer] = 2
        [Long] = 3
        [Single] = 4
        [Double] = 5
    End Enum

    ''' <summary>
    ''' 向量化改写期的轻量类型描述: 只区分「数值元素类型」与「是否一维数值数组」两件事。
    ''' </summary>
    ''' <remarks>
    ''' 引擎不做完整语义分析(不解析 #include 程序集、不建立 SemanticModel),
    ''' 只依据脚本自身的声明做浅层推断, 因此这里只需要描述用于**发射决策**的最小信息。
    ''' </remarks>
    Public Structure ValueTypeInfo

        ''' <summary>元素(或标量)的数值类型; <see cref="NumericKind.Unknown"/> 表示无法确定</summary>
        Public ReadOnly Kind As NumericKind

        ''' <summary>是否为一维数值数组(即"数值向量")</summary>
        Public ReadOnly IsVector As Boolean

        ''' <summary>
        ''' 元素(或标量)的**具名类型**(简单名); 只有非数值类型才会有值, 例如
        ''' <c>Class CLRObjectType</c> 的数组得到 <c>ElementName="CLRObjectType"</c>。
        ''' </summary>
        ''' <remarks>
        ''' <c>@</c> 数组投影运算符需要它: 先由 <see cref="ObjectMemberTable"/> 按类型名查到成员表,
        ''' 才能知道 <c>list@x</c> 投影出来的元素是什么类型。
        ''' </remarks>
        Public ReadOnly ElementName As String

        ''' <param name="kind">数值类型; 非数值类型传 <see cref="NumericKind.Unknown"/></param>
        ''' <param name="isVector">是否为一维数组</param>
        ''' <param name="elementName">具名类型(简单名), 没有则为 <c>Nothing</c></param>
        Public Sub New(kind As NumericKind, isVector As Boolean, Optional elementName As String = Nothing)
            Me.Kind = kind
            Me.IsVector = isVector
            Me.ElementName = elementName
        End Sub

        ''' <summary>是否为可参与向量化的数值向量</summary>
        Public ReadOnly Property IsNumericVector As Boolean
            Get
                Return IsVector AndAlso Kind <> NumericKind.Unknown
            End Get
        End Property

        ''' <summary>是否为可参与向量化的数值标量</summary>
        Public ReadOnly Property IsNumericScalar As Boolean
            Get
                Return (Not IsVector) AndAlso Kind <> NumericKind.Unknown
            End Get
        End Property

        ''' <summary>
        ''' 是否为"可具名的对象向量"—— 即 <c>@</c> 数组投影运算符可能作用的接收者。
        ''' </summary>
        ''' <remarks>
        ''' 这里只做**形状**判断(一维数组 + 非数值 + 元素类型有名字), 不查询成员表;
        ''' 「该类型确实声明了所请求的成员」由调用方的
        ''' <see cref="ObjectMemberTable.TryGetMember"/> 把关。
        ''' </remarks>
        Public ReadOnly Property IsObjectVector As Boolean
            Get
                Return IsVector AndAlso Kind = NumericKind.Unknown AndAlso Not String.IsNullOrEmpty(ElementName)
            End Get
        End Property

        ''' <summary>是否为"确定类型"的取值(数值已知, 或者具名类型已知)</summary>
        Public ReadOnly Property IsKnown As Boolean
            Get
                Return Kind <> NumericKind.Unknown OrElse Not String.IsNullOrEmpty(ElementName)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Dim name As String = If(String.IsNullOrEmpty(ElementName), VectorType.DisplayName(Kind), ElementName)

            Return name & If(IsVector, "()", "")
        End Function
    End Structure

    ''' <summary>
    ''' 向量化改写期的类型模型与 VB 逐元素类型提升规则。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 这里的规则用于决定「一个向量表达式展开之后的结果元素类型」,
    ''' 从而保证改写结果与「把同一个表达式逐元素地用标量 VB 语法算一遍」完全等价:
    ''' <list type="table">
    ''' <item>
    '''   <term><c>+ - * Mod</c></term>
    '''   <description>取两个操作数的公共数值类型(``Double &gt; Single &gt; Long &gt; Integer &gt; Short``);</description>
    ''' </item>
    ''' <item>
    '''   <term><c>/</c></term>
    '''   <description>除 <c>Single / Single</c> 仍为 <c>Single</c> 之外, 结果一律提升为 <c>Double</c>;</description>
    ''' </item>
    ''' <item>
    '''   <term><c>^</c></term>
    '''   <description>无论操作数为何类型, 结果恒为 <c>Double</c>;</description>
    ''' </item>
    ''' <item>
    '''   <term><c>\</c></term>
    '''   <description>仅适用于整型; 含 <c>Long</c> 得 <c>Long</c>, 否则得 <c>Integer</c>。</description>
    ''' </item>
    ''' </list>
    ''' </para>
    ''' <para>
    ''' 已知的轻微偏差: VB 之中 <c>Short \ Short</c> 的结果是 <c>Short</c>,
    ''' 而这里统一提升为 <c>Integer</c>(运行时的 <c>SimdIntegerDivide</c> 只提供 Integer/Long 形态)。
    ''' 该偏差只影响元素类型的静态声明, 不会改变数值结果。
    ''' </para>
    ''' </remarks>
    Public Module VectorType

        ''' <summary>无法确定类型时的显示名</summary>
        Public Const UnknownName As String = "?"

        ''' <summary>
        ''' VB 类型别名 → <see cref="NumericKind"/>(大小写不敏感)。
        ''' </summary>
        ''' <remarks>
        ''' 只登记运行时 <c>Microsoft.VisualBasic.Math.SIMD</c> 真正提供了内核的类型;
        ''' <c>Byte</c>/<c>SByte</c>/<c>UShort</c>/<c>UInteger</c>/<c>ULong</c>/<c>Decimal</c>
        ''' 一律不登记(推断为 <see cref="NumericKind.Unknown"/>), 从而不会被改写,
        ''' 保证「宁可漏改, 不可改错」。
        ''' </remarks>
        Private ReadOnly Aliases As New Dictionary(Of String, NumericKind)(StringComparer.OrdinalIgnoreCase) From {
            {"short", NumericKind.Short},
            {"int16", NumericKind.Short},
            {"integer", NumericKind.Integer},
            {"int32", NumericKind.Integer},
            {"int", NumericKind.Integer},
            {"long", NumericKind.Long},
            {"int64", NumericKind.Long},
            {"single", NumericKind.Single},
            {"float", NumericKind.Single},
            {"double", NumericKind.Double}
        }

        ''' <summary>数值字面量的类型后缀 → <see cref="NumericKind"/></summary>
        Private ReadOnly LiteralSuffixes As New Dictionary(Of String, NumericKind)(StringComparer.OrdinalIgnoreCase) From {
            {"S", NumericKind.Short},
            {"L", NumericKind.Long},
            {"&", NumericKind.Long},
            {"F", NumericKind.Single},
            {"!", NumericKind.Single},
            {"R", NumericKind.Double},
            {"#", NumericKind.Double},
            {"D", NumericKind.Unknown},     ' Decimal: 运行时没有对应内核
            {"@", NumericKind.Unknown},
            {"US", NumericKind.Unknown},
            {"UI", NumericKind.Unknown},
            {"UL", NumericKind.Unknown}
        }

        ''' <summary>尝试把 VB 类型名解析为受支持的数值类型(不区分大小写)</summary>
        Public Function TryParseKind(typeName As String, ByRef kind As NumericKind) As Boolean
            If String.IsNullOrEmpty(typeName) Then
                kind = NumericKind.Unknown
                Return False
            End If

            Return Aliases.TryGetValue(typeName.Trim(), kind)
        End Function

        ''' <summary>
        ''' 从 VB 类型名文本中取出**简单名**(去掉命名空间限定、泛型实参与数组记号),
        ''' 用作 <see cref="ValueTypeInfo.ElementName"/> 的键。
        ''' </summary>
        ''' <remarks>
        ''' 例: <c>Model.Person</c> → <c>Person</c>; <c>List(Of Integer)</c> → <c>List</c>;
        ''' <c>Foo()</c> → <c>Foo</c>; <c>Foo(,)</c> → <c>Foo</c>。
        ''' 取不出合法的标识符时返回 <c>Nothing</c>。
        ''' </remarks>
        Public Function SimpleTypeName(raw As String) As String
            If String.IsNullOrEmpty(raw) Then
                Return Nothing
            End If

            Dim text As String = raw.Trim()

            ' 去掉泛型实参: List(Of Integer) -> List
            Dim generic As Integer = text.IndexOf("("c)

            If generic >= 0 Then
                text = text.Substring(0, generic)
            End If

            text = text.Trim()

            ' 去掉嵌套限定: Model.Person -> Person
            Dim dot As Integer = text.LastIndexOf("."c)

            If dot >= 0 Then
                text = text.Substring(dot + 1)
            End If

            text = text.Trim()

            ' 具名类型必须是合法标识符
            If text.Length = 0 OrElse Not Char.IsLetter(text(0)) AndAlso text(0) <> "_"c Then
                Return Nothing
            End If

            For Each c As Char In text
                If Not Char.IsLetterOrDigit(c) AndAlso c <> "_"c Then
                    Return Nothing
                End If
            Next

            Return text
        End Function

        ''' <summary>数值类型的 VB 显示名</summary>
        Public Function DisplayName(kind As NumericKind) As String
            Select Case kind
                Case NumericKind.Short : Return "Short"
                Case NumericKind.Integer : Return "Integer"
                Case NumericKind.Long : Return "Long"
                Case NumericKind.Single : Return "Single"
                Case NumericKind.Double : Return "Double"
                Case Else : Return UnknownName
            End Select
        End Function

        ' ==================================================================
        ' 运算符结果类型(VB 逐元素语义)
        ' ==================================================================

        ''' <summary>
        ''' <c>+ - * Mod</c> 的结果类型: 两个操作数的公共数值类型。
        ''' 任一操作数类型未知时返回 <see cref="NumericKind.Unknown"/>。
        ''' </summary>
        Public Function Promote(a As NumericKind, b As NumericKind) As NumericKind
            If a = NumericKind.Unknown OrElse b = NumericKind.Unknown Then
                Return NumericKind.Unknown
            End If

            ' 枚举取值顺序即 VB 的数值提升顺序
            Return If(a > b, a, b)
        End Function

        ''' <summary>
        ''' <c>/</c> 的结果类型: <c>Single / Single</c> 得 <c>Single</c>, 其余一律 <c>Double</c>。
        ''' </summary>
        Public Function DivideKind(a As NumericKind, b As NumericKind) As NumericKind
            If a = NumericKind.Unknown OrElse b = NumericKind.Unknown Then
                Return NumericKind.Unknown
            End If

            Return If(a = NumericKind.Single AndAlso b = NumericKind.Single, NumericKind.Single, NumericKind.Double)
        End Function

        ''' <summary><c>^</c> 的结果类型恒为 <c>Double</c></summary>
        Public Function PowerKind(a As NumericKind, b As NumericKind) As NumericKind
            If a = NumericKind.Unknown OrElse b = NumericKind.Unknown Then
                Return NumericKind.Unknown
            End If

            Return NumericKind.Double
        End Function

        ''' <summary>
        ''' <c>\</c> 的结果类型: 含 <c>Long</c> 得 <c>Long</c>, 否则得 <c>Integer</c>;
        ''' 操作数出现浮点类型时返回 <see cref="NumericKind.Unknown"/>(VB 本身不允许)。
        ''' </summary>
        Public Function IntegerDivideKind(a As NumericKind, b As NumericKind) As NumericKind
            If a = NumericKind.Unknown OrElse b = NumericKind.Unknown Then
                Return NumericKind.Unknown
            End If

            If a = NumericKind.Single OrElse a = NumericKind.Double Then
                Return NumericKind.Unknown
            End If
            If b = NumericKind.Single OrElse b = NumericKind.Double Then
                Return NumericKind.Unknown
            End If

            Return If(a = NumericKind.Long OrElse b = NumericKind.Long, NumericKind.Long, NumericKind.Integer)
        End Function

        ''' <summary>
        ''' <c>Mod</c> 的结果类型: <c>Short</c> 会像 VB 一样提升到 <c>Integer</c>
        ''' (运行时的 <c>SimdModulo</c> 只提供 Integer/Long/Single/Double 形态)。
        ''' </summary>
        Public Function ModuloKind(a As NumericKind, b As NumericKind) As NumericKind
            Dim common As NumericKind = Promote(a, b)

            If common = NumericKind.Short Then
                Return NumericKind.Integer
            End If

            Return common
        End Function

        ' ==================================================================
        ' 字面量类型推断
        ' ==================================================================

        ''' <summary>
        ''' 由数值字面量的文本推断其类型, 处理 VB 的类型字符后缀(<c>S</c>/<c>L</c>/<c>F</c>/<c>R</c>…)
        ''' 以及十六进制/八进制字面量。
        ''' </summary>
        ''' <remarks>
        ''' 无法识别的形态(含小数点但没有后缀以外的提示、以及 Decimal 后缀)按规则退化:
        ''' 出现小数点或指数记数法得 <c>Double</c>(注释里说明的"VB 默认"语义),
        ''' Decimal 后缀返回 <see cref="NumericKind.Unknown"/> 以避免被改写。
        ''' </remarks>
        Public Function LiteralKind(text As String) As NumericKind
            If String.IsNullOrEmpty(text) Then
                Return NumericKind.Unknown
            End If

            Dim value As String = text.Trim().Replace("_", "")

            ' 十六进制(&H1F)与八进制(&O17)字面量: VB 之中得 Integer 或 Long
            If value.StartsWith("&H", StringComparison.OrdinalIgnoreCase) OrElse
               value.StartsWith("&O", StringComparison.OrdinalIgnoreCase) Then

                Return NumericKind.Integer
            End If

            Dim kind As NumericKind

            ' 先看双字符后缀(US/UI/UL), 再看单字符后缀
            If value.Length >= 2 Then
                Dim tail2 As String = value.Substring(value.Length - 2)

                If LiteralSuffixes.TryGetValue(tail2, kind) Then
                    Return kind
                End If
            End If

            Dim tail1 As String = value.Substring(value.Length - 1)

            If Not Char.IsDigit(tail1(0)) Then
                If LiteralSuffixes.TryGetValue(tail1, kind) Then
                    Return kind
                End If
            End If

            ' 没有后缀: 含小数点或指数记数法则为 Double, 否则为 Integer
            If value.IndexOf("."c) >= 0 Then
                Return NumericKind.Double
            End If
            If Regex.IsMatch(value, "[eE][+-]?\d", RegexOptions.IgnoreCase) Then
                Return NumericKind.Double
            End If

            Return NumericKind.Integer
        End Function
    End Module
End Namespace
