' ---------------------------------------------------------------------------
' 极简命令行解析器
'
' 为什么不用现成的解析库：
'   1. 这条流水线的参数不到 20 个，全是“--key value / --flag”形态；
'   2. 生产环境要求零额外依赖，能直接 dotnet run 就跑；
'   3. 需要把“未知参数”收集出来告警，而不是静默忽略——扫描参数写错时
'      （例如 --iteraton）必须让人立刻发现，否则会白跑几个小时。
' ---------------------------------------------------------------------------

Imports System.Globalization

Namespace Cli

    ''' <summary>
    ''' 命令行参数集合（大小写不敏感）。
    ''' </summary>
    Public Class CommandLine

        Private ReadOnly _values As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        Private ReadOnly _flags As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Private ReadOnly _unknown As New List(Of String)()

        ''' <summary>无法识别的裸参数（不以 <c>--</c> 开头）。</summary>
        Public ReadOnly Property Unknown As IList(Of String)
            Get
                Return _unknown
            End Get
        End Property

        ''' <summary>所有带值的参数名（用于报告参数快照）。</summary>
        Public ReadOnly Property Names As IEnumerable(Of String)
            Get
                Return _values.Keys
            End Get
        End Property

        ''' <summary>
        ''' 解析参数数组。
        ''' </summary>
        Public Shared Function Parse(args As String()) As CommandLine
            Dim cli As New CommandLine()

            If args Is Nothing Then Return cli

            Dim i As Integer = 0

            While i < args.Length
                Dim token As String = args(i)

                If token.StartsWith("--") Then
                    Dim body As String = token.Substring(2)
                    Dim equalAt As Integer = body.IndexOf("="c)

                    If equalAt > 0 Then
                        cli._values(body.Substring(0, equalAt)) = body.Substring(equalAt + 1)
                        i += 1
                    ElseIf i + 1 < args.Length AndAlso Not args(i + 1).StartsWith("--") Then
                        cli._values(body) = args(i + 1)
                        i += 2
                    Else
                        ' 没有值 ⇒ 布尔开关
                        Call cli._flags.Add(body)
                        i += 1
                    End If
                Else
                    Call cli._unknown.Add(token)
                    i += 1
                End If
            End While

            Return cli
        End Function

        ''' <summary>是否出现该开关（<c>--flag</c>），或该参数被显式赋值为 true/1/yes。</summary>
        Public Function Has(name As String) As Boolean
            If _flags.Contains(name) Then Return True

            Dim text As String

            If _values.TryGetValue(name, text) Then
                Return IsTrue(text)
            End If

            Return False
        End Function

        ''' <summary>取字符串值。</summary>
        Public Function GetValue(name As String, Optional defaultValue As String = Nothing) As String
            Dim text As String

            If _values.TryGetValue(name, text) Then Return text

            Return defaultValue
        End Function

        ''' <summary>取整数值（解析失败时返回默认值）。</summary>
        Public Function GetInt(name As String, Optional defaultValue As Integer = 0) As Integer
            Dim text As String = GetValue(name)

            If String.IsNullOrEmpty(text) Then Return defaultValue

            Dim value As Integer

            If Integer.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, value) Then
                Return value
            Else
                Console.WriteLine($"[warn] 参数 --{name}={text} 不是合法整数，回退到默认值 {defaultValue}")
                Return defaultValue
            End If
        End Function

        ''' <summary>取浮点值。</summary>
        Public Function GetDouble(name As String, Optional defaultValue As Double = 0.0) As Double
            Dim text As String = GetValue(name)

            If String.IsNullOrEmpty(text) Then Return defaultValue

            Dim value As Double

            If Double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, value) Then
                Return value
            Else
                Console.WriteLine($"[warn] 参数 --{name}={text} 不是合法数值，回退到默认值 {defaultValue}")
                Return defaultValue
            End If
        End Function

        Private Shared Function IsTrue(text As String) As Boolean
            If String.IsNullOrEmpty(text) Then Return False

            Select Case text.Trim().ToLowerInvariant()
                Case "1", "true", "yes", "on", "y"
                    Return True
                Case Else
                    Return False
            End Select
        End Function

    End Class

End Namespace
