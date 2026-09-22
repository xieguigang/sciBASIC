Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.Data

Namespace Printing

    ''' <summary>
    ''' 数据打印函数: 把基础数据元素类型的集合按 GNU R 的向量格式打印, 把 <see cref="NumericTable"/>
    ''' 按带边框的表格打印。用于脚本调试时快速查看数据内容。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>向量打印</b>(<c>{1,3,24,23,4,23}</c>):
    ''' <code>
    ''' [1]  1  3 24 23  4 23
    ''' </code>
    ''' 方括号之中是<b>本行第一个元素的下标</b>(与 GNU R 一致, 而不是元素个数);
    ''' 超出 <c>width</c>(缺省 80 字符)时自动折行, 每行都带自己的起始下标
    ''' (与 R 一致地把下标标记整体右对齐, 即填充加在方括号之前):
    ''' <code>
    '''  [1]  1  2  3 ... 25
    ''' [26] 26 27 28 ... 40
    ''' </code>
    ''' </para>
    ''' <para>
    ''' <b>元素类型的格式化规则</b>(<see cref="RFormat.ElementText"/>):
    ''' <list type="bullet">
    ''' <item><see cref="Integer"/> / <see cref="Byte"/> / <see cref="Short"/> / <see cref="Long"/> /
    ''' <see cref="UInteger"/> / <see cref="ULong"/> / <see cref="Decimal"/>: 整数文本, 右对齐;</item>
    ''' <item><see cref="Single"/> / <see cref="Double"/>: 按有效数字位数截断(缺省 7 位, 对应 R 的
    ''' <c>options(digits=7)</c>), 右对齐; <c>NaN</c>/<c>Inf</c>/<c>-Inf</c> 使用 R 的写法;</item>
    ''' <item><see cref="Boolean"/>: <c>True</c>/<c>False</c>, 右对齐;</item>
    ''' <item><see cref="String"/> / <see cref="Char"/>: 带双引号的字面量, 左对齐;</item>
    ''' <item>枚举(<c>Enum</c>): <c>ToString</c> 之后当作字符串向量的成员打印(带双引号, 左对齐);</item>
    ''' <item><c>Nothing</c> 元素打印为 <c>NA</c>; 元素本身是集合时(例如 <c>Double()()</c>)按 R 的
    ''' <c>[[i]]</c> 列表格式打印。</item>
    ''' </list>
    ''' </para>
    ''' <para>
    ''' <b>二维表打印</b>(<see cref="NumericTable"/>): 表头是特征列名与标签列名, 首列是行名, 数值列右对齐,
    ''' 渲染为 <c>+---+</c> 边框表格。
    ''' </para>
    ''' <para>
    ''' <b>在 VBS 脚本之中使用</b>: 脚本里直接写 <c>print(x)</c> 即可(宿主会注入一个同名转发函数,
    ''' 见 <c>VBS\src\VBScript\Syntax\ScriptRefactor.vb</c>)。之所以需要那个转发函数, 是因为在生成代码的
    ''' 导入作用域之中 <c>Print</c> 这个名字已经被 <c>Microsoft.VisualBasic.FileSystem</c> 与
    ''' <c>Microsoft.VisualBasic.CommandLine.CLITools</c> 同时导出, VB 会直接报 <c>BC30561</c> 名称不明确,
    ''' 因此必须由脚本所在 Module 本地声明一个同名成员来遮蔽它们。
    ''' </para>
    ''' <para>
    ''' <b>以库的方式调用</b>: <c>Imports Microsoft.VisualBasic.Printing</c> 之后即可使用
    ''' <c>Print(data)</c> / <c>ToText(data)</c>。
    ''' </para>
    ''' </remarks>
    Public Module Print

#Region "collections"

        ''' <summary>
        ''' 把集合对象按 GNU R 的向量格式打印到 <paramref name="output"/>(缺省为控制台)。
        ''' </summary>
        ''' <typeparam name="T">
        ''' 集合的元素类型: <see cref="Integer"/> / <see cref="Byte"/> / <see cref="Single"/> /
        ''' <see cref="Double"/> / <see cref="Boolean"/> / <see cref="String"/> / <see cref="Char"/> /
        ''' <see cref="Short"/> / <see cref="Long"/> / <see cref="UInteger"/> / <see cref="ULong"/> /
        ''' <see cref="Decimal"/> 以及枚举。
        ''' </typeparam>
        ''' <param name="data">集合(数组 / <see cref="List(Of T)"/> / 任何 <see cref="IEnumerable(Of T)"/>)</param>
        ''' <param name="output">输出设备, 缺省为控制台; 传入 <see cref="StringWriter"/> 可以捕获输出文本</param>
        ''' <param name="width">每行最大字符数, 缺省 80; 传入非正数表示不折行</param>
        ''' <param name="digits">浮点数的有效数字位数, 缺省 7</param>
        ''' <example>
        ''' <code>
        ''' Dim x = {1, 3, 24, 23, 4, 23}
        ''' Call print(x)
        ''' ' [1]  1  3 24 23  4 23
        ''' </code>
        ''' </example>
        Public Sub Print(Of T)(data As IEnumerable(Of T),
                               Optional output As TextWriter = Nothing,
                               Optional width As Integer = RFormat.DefaultWidth,
                               Optional digits As Integer = RFormat.DefaultDigits)

            Call WriteLine(ToText(data, width, digits), output)
        End Sub

        ''' <summary>
        ''' 把集合对象渲染为 GNU R 风格的向量文本(不打印)。
        ''' </summary>
        ''' <inheritdoc cref="Print(Of T)(IEnumerable(Of T), TextWriter, Integer, Integer)"/>
        Public Function ToText(Of T)(data As IEnumerable(Of T),
                                     Optional width As Integer = RFormat.DefaultWidth,
                                     Optional digits As Integer = RFormat.DefaultDigits) As String

            If data Is Nothing Then
                Return RFormat.NULL
            End If

            Return RFormat.VectorText(data.Select(Function(x) CObj(x)), GetType(T), width, digits)
        End Function

#End Region

#Region "numeric table"

        ''' <summary>
        ''' 把数值二维表按带边框的表格打印到 <paramref name="output"/>(缺省为控制台)。
        ''' </summary>
        ''' <param name="table">数值二维表, 可以为 <c>Nothing</c>(打印为 <c>NULL</c>)</param>
        ''' <param name="output">输出设备, 缺省为控制台; 传入 <see cref="StringWriter"/> 可以捕获输出文本</param>
        ''' <param name="digits">浮点数的有效数字位数, 缺省 7</param>
        ''' <example>
        ''' <code>
        ''' Dim tbl = NumericTable.FromColumns({"x", "y"}, {{1, 2, 3}, {4, 5, 6}}, {"s1", "s2", "s3"})
        ''' Call print(tbl)
        ''' ' +----+---+---+
        ''' ' |    | x | y |
        ''' ' +----+---+---+
        ''' ' | s1 | 1 | 4 |
        ''' ' | s2 | 2 | 5 |
        ''' ' | s3 | 3 | 6 |
        ''' ' +----+---+---+
        ''' </code>
        ''' </example>
        Public Sub Print(table As NumericTable,
                         Optional output As TextWriter = Nothing,
                         Optional digits As Integer = RFormat.DefaultDigits)

            Call WriteLine(ToText(table, digits), output)
        End Sub

        ''' <summary>
        ''' 把数值二维表渲染为带边框的表格文本(不打印)。
        ''' </summary>
        ''' <inheritdoc cref="Print(NumericTable, TextWriter, Integer)"/>
        Public Function ToText(table As NumericTable,
                               Optional digits As Integer = RFormat.DefaultDigits) As String

            Return RFormat.TableText(table, digits)
        End Function

#End Region

#Region "runtime dispatch"

        ''' <summary>
        ''' 把字符串按 R 的标量形式打印(例如 <c>[1] "done"</c>)。
        ''' </summary>
        ''' <remarks>
        ''' 该重载存在的意义是**优先级**: <see cref="String"/> 同时也是一个 <c>IEnumerable(Of Char)</c>,
        ''' 若没有这个精确匹配的重载, <c>print("abc")</c> 会被当作字符向量打印为 <c>[1] "a" "b" "c"</c>。
        ''' </remarks>
        Public Sub Print(value As String, Optional output As TextWriter = Nothing)
            Call WriteLine(ToText(value), output)
        End Sub

        ''' <summary>把字符串按 R 的标量形式渲染(不打印), 例如 <c>[1] "done"</c></summary>
        Public Function ToText(value As String) As String
            If value Is Nothing Then
                Return RFormat.NULL
            End If

            Return RFormat.ScalarText(value)
        End Function

        ''' <summary>
        ''' 打印任意值: 按<b>运行时类型</b>分派到二维表 / 集合 / 标量的打印格式。
        ''' </summary>
        ''' <param name="value">任意值: 二维表、集合、标量, 或者 <c>Nothing</c>(打印为 <c>NULL</c>)</param>
        ''' <param name="output">输出设备, 缺省为控制台</param>
        ''' <param name="width">集合折行的每行最大字符数, 缺省 80; 非正数表示不折行</param>
        ''' <param name="digits">浮点数的有效数字位数, 缺省 7</param>
        ''' <remarks>
        ''' 脚本宿主注入的 <c>print</c> 转发函数就指向本重载: 脚本是一种动态场景(变量常常是
        ''' <c>let</c> 声明的 <see cref="Object"/>, 或者 <c>New List(Of Double)</c> 这类集合),
        ''' 因此无法依赖编译期的重载解析, 只能在运行期判断。
        ''' </remarks>
        Public Sub Print(value As Object,
                         Optional output As TextWriter = Nothing,
                         Optional width As Integer = RFormat.DefaultWidth,
                         Optional digits As Integer = RFormat.DefaultDigits)

            Call WriteLine(ToText(value, width, digits), output)
        End Sub

        ''' <summary>
        ''' 把任意值按运行时类型渲染为文本(不打印): 二维表 → 表格; 集合 → R 向量; 其它 → R 标量。
        ''' </summary>
        ''' <inheritdoc cref="Print(Object, TextWriter, Integer, Integer)"/>
        Public Function ToText(value As Object,
                               Optional width As Integer = RFormat.DefaultWidth,
                               Optional digits As Integer = RFormat.DefaultDigits) As String

            If value Is Nothing Then
                Return RFormat.NULL
            ElseIf TypeOf value Is NumericTable Then
                Return RFormat.TableText(DirectCast(value, NumericTable), digits)
            ElseIf TypeOf value Is String Then
                Return RFormat.ScalarText(DirectCast(value, String), digits)
            ElseIf TypeOf value Is IEnumerable Then
                ' 数组可以拿到确切的元素类型, 从而让空集合也能打印成 numeric(0) 之类的形式
                Dim type As Type = value.GetType
                Dim elementType As Type = If(type.IsArray, type.GetElementType, Nothing)

                Return RFormat.VectorText(DirectCast(value, IEnumerable).Cast(Of Object)(), elementType, width, digits)
            Else
                Return RFormat.ScalarText(value, digits)
            End If
        End Function

#End Region

        ''' <summary>把渲染结果写出一行; <paramref name="output"/> 为 <c>Nothing</c> 时写控制台</summary>
        Private Sub WriteLine(text As String, output As TextWriter)
            Call If(output Is Nothing, Console.Out, output).WriteLine(text)
        End Sub
    End Module
End Namespace
