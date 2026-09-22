#Region "Microsoft.VisualBasic::c2558519c854659bcd55a03ee22b3681, Microsoft.VisualBasic.Core\src\Printing\RFormat.vb"

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

    '   Total Lines: 450
    '    Code Lines: 250 (55.56%)
    ' Comment Lines: 125 (27.78%)
    '    - Xml Docs: 96.80%
    ' 
    '   Blank Lines: 75 (16.67%)
    '     File Size: 20.40 KB


    '     Module RFormat
    ' 
    '         Function: AlignTokens, ElementText, FirstNotNull, HeadersOrDefault, IndexPrefix
    '                   IndexPrefixLength, IsNestedCollection, IsTextElement, ListText, NumericText
    '                   Quote, RTypeName, ScalarText, TableText, VectorText
    '                   WrapVector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.Linq
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter.Flags
Imports Microsoft.VisualBasic.Data

Namespace Printing

    ''' <summary>
    ''' GNU R 风格的文本格式化内核: 把集合对象/二维表渲染为 R 的 <c>print()</c> 观感文本。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 本模块只负责"把数据变成字符串", 控制台写出与公开 API 由 <see cref="Print"/> 提供。
    ''' 之所以独立成一个模块, 是因为运行期(<c>vbs run.vb</c>)与工程期(<c>vbs make-project</c>)、
    ''' 以及脚本动态分派的入口都需要复用同一套渲染规则。
    ''' </para>
    ''' <para>
    ''' <b>与 GNU R 的对应关系</b>:
    ''' <list type="table">
    ''' <listheader><term>本项目</term><description>GNU R</description></listheader>
    ''' <item><term>向量 <c>[1] 1 3 24</c></term><description><c>print(c(1,3,24))</c></description></item>
    ''' <item><term>折行 + 续行下标 <c>[9] ...</c></term><description><c>print(1:100)</c>(<c>options(width=80)</c>)</description></item>
    ''' <item><term>7 位有效数字</term><description><c>options(digits=7)</c> 下的 <c>format()</c></description></item>
    ''' <item><term>字符串/枚举带双引号且左对齐</term><description>character 向量</description></item>
    ''' <item><term>嵌套集合 <c>[[1]]</c></term><description>list 打印</description></item>
    ''' <item><term>缺失值 <c>NA</c>、空对象 <c>NULL</c>、<c>numeric(0)</c></term><description>同 R 的约定</description></item>
    ''' </list>
    ''' </para>
    ''' <para>
    ''' <b>刻意保留的差异</b>:
    ''' <list type="bullet">
    ''' <item>逻辑值打印为 <c>True</c>/<c>False</c>(沿用 .NET 的 <see cref="Object.ToString"/> 语义),
    ''' 而不是 R 的 <c>TRUE</c>/<c>FALSE</c>;</item>
    ''' <item>折行按"当前行还能放得下几个元素"贪心填充, 而不是 R 的"先定列数再整列换行",
    ''' 两者在等宽数值向量的情况下结果完全一致;</item>
    ''' <item>二维表沿用框架既有的 <see cref="ConsoleTableBuilder"/>(带 <c>+---+</c> 边框),
    ''' 而不是 R 的无边框对齐表格。</item>
    ''' </list>
    ''' </para>
    ''' </remarks>
    Friend Module RFormat

        ''' <summary>
        ''' 每行的最大字符数, 对应 R 的 <c>options(width=)</c> 缺省值 80。
        ''' 传入非正数表示不折行。
        ''' </summary>
        Public Const DefaultWidth As Integer = 80

        ''' <summary>
        ''' 浮点数的有效数字位数, 对应 R 的 <c>options(digits=)</c> 缺省值 7。
        ''' </summary>
        Public Const DefaultDigits As Integer = 7

        ''' <summary>缺失值(集合之中的 <c>Nothing</c> 元素), 与 R 的 <c>NA</c> 对应</summary>
        Public Const NA As String = "NA"

        ''' <summary>空对象(例如 <c>Nothing</c> 的二维表), 与 R 的 <c>NULL</c> 对应</summary>
        Public Const NULL As String = "NULL"

#Region "element format"

        ''' <summary>
        ''' 把集合之中的一个元素渲染为 R 风格的标量文本。
        ''' </summary>
        ''' <param name="value">元素值, 可以为 <c>Nothing</c>(渲染为 <c>NA</c>)</param>
        ''' <param name="digits">浮点数的有效数字位数</param>
        ''' <remarks>
        ''' 各类型的基本数据类型(<see cref="Integer"/> / <see cref="Byte"/> / <see cref="Single"/> /
        ''' <see cref="Double"/> / <see cref="Boolean"/> / <see cref="String"/> / <see cref="Char"/> /
        ''' <see cref="Short"/> / <see cref="Long"/> / <see cref="UInteger"/> / <see cref="ULong"/> /
        ''' <see cref="Decimal"/>)以及枚举都被覆盖; 枚举按照约定先 <c>ToString</c> 再当作字符串向量的
        ''' 成员打印; 其余类型退化为 <see cref="Object.ToString"/>。
        ''' </remarks>
        Public Function ElementText(value As Object, Optional digits As Integer = DefaultDigits) As String
            If value Is Nothing Then
                Return NA
            End If

            Dim type As Type = value.GetType

            If type.IsEnum Then
                Return Quote(value.ToString)
            End If

            Select Case System.Type.GetTypeCode(type)
                Case TypeCode.Double, TypeCode.Single
                    Return NumericText(CDbl(value), digits)
                Case TypeCode.Decimal
                    ' Decimal 的存在意义就是精确十进制, 因此不做有效位截断
                    Return CDec(value).ToString(CultureInfo.InvariantCulture)
                Case TypeCode.Boolean
                    Return CBool(value).ToString
                Case TypeCode.Char, TypeCode.String
                    Return Quote(CStr(value))
                Case TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16,
                     TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64
                    Return Convert.ToString(value, CultureInfo.InvariantCulture)
                Case Else
                    Return value.ToString
            End Select
        End Function

        ''' <summary>
        ''' 把单个标量渲染为 R 的 <c>print(42)</c> 形式: <c>[1] 42</c>。
        ''' </summary>
        ''' <param name="value">标量值; <c>Nothing</c> 渲染为 <c>NULL</c></param>
        ''' <param name="digits">浮点数的有效数字位数</param>
        Public Function ScalarText(value As Object, Optional digits As Integer = DefaultDigits) As String
            If value Is Nothing Then
                Return NULL
            End If

            Return $"[1] {ElementText(value, digits)}"
        End Function

        ''' <summary>
        ''' 把一个浮点值渲染为 R 的 <c>format()</c> 风格文本。
        ''' </summary>
        ''' <param name="value">浮点值</param>
        ''' <param name="digits">有效数字位数</param>
        ''' <remarks>
        ''' 1. <c>NaN</c> / <c>Inf</c> / <c>-Inf</c> 使用 R 的写法而不是 .NET 的
        ''' <c>NaN</c> / <c>∞</c>(并且与当前区域设置无关);
        ''' 2. <see cref="Single"/> 会先拓宽到 <see cref="Double"/> 再按有效位截断,
        ''' 因此 <c>1.1F</c> 打印为 <c>1.1</c> 而不是 <c>1.100000023841858</c>;
        ''' 3. .NET 的 <c>G</c> 格式输出大写指数(<c>1.234568E+07</c>), 这里统一改成 R 的小写 <c>e</c>。
        ''' </remarks>
        Public Function NumericText(value As Double, Optional digits As Integer = DefaultDigits) As String
            If Double.IsNaN(value) Then
                Return "NaN"
            ElseIf Double.IsPositiveInfinity(value) Then
                Return "Inf"
            ElseIf Double.IsNegativeInfinity(value) Then
                Return "-Inf"
            End If

            Dim precision As Integer = If(digits > 0, digits, DefaultDigits)

            Return value.ToString($"G{precision}", CultureInfo.InvariantCulture).Replace("E", "e")
        End Function

        ''' <summary>给字符串加上 R 风格的双引号(内部的双引号转义为 <c>\"</c>)</summary>
        Public Function Quote(text As String) As String
            If text Is Nothing Then
                Return NA
            End If

            Return """" & text.Replace("""", "\""") & """"
        End Function

        ''' <summary>
        ''' 元素是否属于"字符串向量"的成员(字符/字符串/枚举)。
        ''' 这类元素在 R 之中左对齐, 其余(数值与逻辑值)右对齐。
        ''' </summary>
        Public Function IsTextElement(value As Object) As Boolean
            If value Is Nothing Then
                Return False
            End If

            Dim type As Type = value.GetType

            If type.IsEnum Then
                Return True
            End If

            Dim code As TypeCode = System.Type.GetTypeCode(type)

            Return code = TypeCode.Char OrElse code = TypeCode.String
        End Function

        ''' <summary>
        ''' 空集合在 R 之中按元素类型打印, 例如 <c>numeric(0)</c> / <c>character(0)</c> / <c>logical(0)</c>。
        ''' </summary>
        ''' <param name="elementType">集合的元素类型; 未知(动态类型集合)时传 <c>Nothing</c></param>
        Public Function RTypeName(elementType As Type) As String
            If elementType Is Nothing Then
                Return "vector"
            End If
            If elementType.IsEnum Then
                Return "factor"
            End If

            Select Case System.Type.GetTypeCode(elementType)
                Case TypeCode.Char, TypeCode.String
                    Return "character"
                Case TypeCode.Boolean
                    Return "logical"
                Case TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16,
                     TypeCode.Int32, TypeCode.UInt32
                    Return "integer"
                Case TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                    Return "numeric"
                Case Else
                    Return "vector"
            End Select
        End Function

#End Region

#Region "vector format"

        ''' <summary>
        ''' 把一个集合渲染为 R 风格向量文本(必要时按 <paramref name="width"/> 折行)。
        ''' </summary>
        ''' <param name="data">集合元素(已经统一为 <see cref="Object"/>, 因此可以承载动态类型的集合)</param>
        ''' <param name="elementType">集合的静态元素类型, 仅用于空集合的 <c>numeric(0)</c> 之类的类型名; 未知时传 <c>Nothing</c></param>
        ''' <param name="width">每行最大字符数, 非正数表示不折行</param>
        ''' <param name="digits">浮点数的有效数字位数</param>
        Public Function VectorText(data As IEnumerable(Of Object),
                                   Optional elementType As Type = Nothing,
                                   Optional width As Integer = DefaultWidth,
                                   Optional digits As Integer = DefaultDigits) As String

            Dim items As Object() = data.ToArray

            If items.Length = 0 Then
                Return $"{RTypeName(elementType)}(0)"
            End If
            If IsNestedCollection(items) Then
                ' 例如 Double()() 或者 List(Of List(Of Double)), 按 R 的 list 格式打印
                Return ListText(items, width, digits)
            End If

            Dim isText As Boolean = IsTextElement(FirstNotNull(items))
            Dim tokens As String() = items.Select(Function(x) ElementText(x, digits)).ToArray

            Return WrapVector(AlignTokens(tokens, isText), width)
        End Function

        ''' <summary>
        ''' 按 R 的规则折行: 每行以 <c>[本行起始下标]</c> 开头, 下标右对齐到最大下标的宽度。
        ''' </summary>
        ''' <param name="tokens">已对齐到统一列宽的元素文本</param>
        ''' <param name="width">每行最大字符数, 非正数表示不折行</param>
        ''' <remarks>
        ''' 以 <c>{1,3,24,23,4,23}</c> 为例, 输出正是 R 的
        ''' <c>[1]  1  3 24 23  4 23</c>(下标 <c>[1]</c> 表示本行第一个元素的下标 1)。
        ''' </remarks>
        Private Function WrapVector(tokens As String(), width As Integer) As String
            If tokens.Length = 0 Then
                Return String.Empty
            End If

            Dim indexWidth As Integer = CStr(tokens.Length).Length
            Dim cellWidth As Integer = tokens.Max(Function(s) s.Length)
            Dim noWrap As Boolean = width <= 0
            Dim sb As New StringBuilder
            Dim firstOnLine As Boolean = True
            Dim lineLength As Integer = 0

            Call sb.Append(IndexPrefix(1, indexWidth))
            lineLength = IndexPrefixLength(indexWidth)

            For i As Integer = 0 To tokens.Length - 1
                If Not firstOnLine AndAlso
                    Not noWrap AndAlso
                    (lineLength + 1 + cellWidth) > width Then

                    Call sb.AppendLine()
                    Call sb.Append(IndexPrefix(i + 1, indexWidth))

                    lineLength = IndexPrefixLength(indexWidth)
                    firstOnLine = True
                End If

                If Not firstOnLine Then
                    Call sb.Append(" ")
                    lineLength += 1
                End If

                Call sb.Append(tokens(i))
                lineLength += tokens(i).Length
                firstOnLine = False
            Next

            Return sb.ToString()
        End Function

        ''' <summary>
        ''' R 风格的行首下标标记, 例如当前行从第 9 个元素开始时为 <c>[9] </c>。
        ''' </summary>
        ''' <remarks>
        ''' 与 R 一致地把填充加在方括号<b>之前</b>(而不是把数字在方括号之内右对齐):
        ''' 40 个元素的向量首行是 <c> [1] </c>、次行是 <c>[26] </c>。
        ''' </remarks>
        Private Function IndexPrefix(index As Integer, width As Integer) As String
            Return $"[{index}]".PadLeft(width + 2) & " "
        End Function

        ''' <summary><c>[下标] </c> 标记自身的字符数(左右方括号、一个空格, 以及右对齐所需的填充)</summary>
        Private Function IndexPrefixLength(width As Integer) As Integer
            Return width + 3
        End Function

        ''' <summary>把元素文本对齐到统一的列宽: 字符串向量左对齐, 数值向量右对齐</summary>
        Private Function AlignTokens(tokens As String(), isText As Boolean) As String()
            Dim maxWidth As Integer = tokens.Max(Function(s) s.Length)
            Dim aligned As String() = New String(tokens.Length - 1) {}

            For i As Integer = 0 To tokens.Length - 1
                Dim text As String = tokens(i)

                If text.Length >= maxWidth Then
                    aligned(i) = text
                ElseIf isText Then
                    aligned(i) = text.PadRight(maxWidth)
                Else
                    aligned(i) = text.PadLeft(maxWidth)
                End If
            Next

            Return aligned
        End Function

        ''' <summary>集合之中的元素本身也是集合(例如 <c>Double()()</c>)</summary>
        Private Function IsNestedCollection(items As Object()) As Boolean
            For Each item As Object In items
                If item Is Nothing Then
                    Continue For
                End If
                If TypeOf item Is String Then
                    Return False
                End If

                Return TypeOf item Is IEnumerable
            Next

            Return False
        End Function

        ''' <summary>
        ''' R 的 list 打印格式: 每个成员带 <c>[[i]]</c> 标题, 成员之间以空行分隔。
        ''' </summary>
        Private Function ListText(items As Object(), width As Integer, digits As Integer) As String
            Dim sb As New StringBuilder

            For i As Integer = 0 To items.Length - 1
                If i > 0 Then
                    ' 结束上一项, 并在成员之间留一个空行(与 R 的 list 打印一致)
                    Call sb.AppendLine()
                    Call sb.AppendLine()
                End If

                Call sb.AppendLine($"[[{i + 1}]]")

                Dim item As Object = items(i)

                If item Is Nothing Then
                    Call sb.AppendLine(NULL)
                Else
                    Dim inner As IEnumerable = DirectCast(item, IEnumerable)

                    Call sb.Append(VectorText(
                        inner.Cast(Of Object)(),
                        inner.GetType.GetElementType,
                        width,
                        digits))
                End If
            Next

            Return sb.ToString().TrimEnd()
        End Function

        Private Function FirstNotNull(items As Object()) As Object
            For Each item As Object In items
                If item IsNot Nothing Then
                    Return item
                End If
            Next

            Return Nothing
        End Function

#End Region

#Region "table format"

        ''' <summary>
        ''' 把一个 <see cref="NumericTable"/> 渲染为带边框的表格文本。
        ''' </summary>
        ''' <param name="table">二维表, 可以为 <c>Nothing</c>(渲染为 <c>NULL</c>)</param>
        ''' <param name="digits">浮点数的有效数字位数</param>
        ''' <remarks>
        ''' 表头由特征列名(<see cref="NumericTable.featureNames"/>)与标签列名(<see cref="NumericTable.labelNames"/>)
        ''' 组成, 首列是行名(<see cref="NumericTable.rowNames"/>, 缺失时自动生成 <c>1..n</c>);
        ''' 列名缺失或长度不匹配时按 R 的习惯生成 <c>V1..Vn</c> / <c>L1..Lk</c> 占位列名。
        ''' 行名一列左对齐, 数值列右对齐。
        ''' </remarks>
        Public Function TableText(table As NumericTable, Optional digits As Integer = DefaultDigits) As String
            If table Is Nothing Then
                Return NULL
            End If

            Dim rowIds As String() = table.RowNamesOrDefault()
            Dim headers As New List(Of String) From {String.Empty}
            Dim align As New Dictionary(Of Integer, TextAligntment) From {
                {0, TextAligntment.Left}
            }

            ' 行名一列不写列名(与 R 的 print.data.frame 一致)
            For Each name As String In HeadersOrDefault(table.featureNames, table.nfeatures, "V")
                Call align.Add(headers.Count, TextAligntment.Right)
                Call headers.Add(name)
            Next
            For Each name As String In HeadersOrDefault(table.labelNames, table.nlabels, "L")
                Call align.Add(headers.Count, TextAligntment.Right)
                Call headers.Add(name)
            Next

            Dim data As ConsoleTableBaseData = ConsoleTableBaseData.FromColumnHeaders(headers.ToArray())

            For i As Integer = 0 To table.nsamples - 1
                Dim line As New List(Of Object) From {CObj(rowIds(i))}
                Dim features As Double() = If(table.features Is Nothing, Nothing, table.features(i))
                Dim labels As Double() = If(table.labels Is Nothing, Nothing, table.labels(i))

                For j As Integer = 0 To table.nfeatures - 1
                    Call line.Add(CObj(NumericText(If(features Is Nothing, Double.NaN, features(j)), digits)))
                Next
                For j As Integer = 0 To table.nlabels - 1
                    Call line.Add(CObj(NumericText(If(labels Is Nothing, Double.NaN, labels(j)), digits)))
                Next

                Call data.AppendLine(line.ToArray)
            Next

            Dim builder As ConsoleTableBuilder = ConsoleTableBuilder.From(data)

            builder = builder.WithFormat(ConsoleTableBuilderFormat.Alternative)
            builder = builder.WithTextAlignment(align)
            builder = builder.WithHeaderTextAlignment(align)

            Return builder.Export().ToString().TrimEnd()
        End Function

        ''' <summary>列名缺失或者长度与矩阵宽度不一致时, 生成 <c>V1..Vn</c> 这样的占位列名</summary>
        Private Function HeadersOrDefault(names As String(), size As Integer, prefix As String) As String()
            If names IsNot Nothing AndAlso names.Length = size Then
                Return names
            End If

            Return Enumerable.Range(1, size).Select(Function(i) $"{prefix}{i}").ToArray
        End Function

#End Region

    End Module
End Namespace
