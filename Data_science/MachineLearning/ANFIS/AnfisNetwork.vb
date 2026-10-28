Option Strict On
Option Explicit On

Imports System
Imports System.Linq
Imports System.Text
Imports System.Text.Json
Imports std = System.Math

''' <summary>
''' 自适应神经模糊推理系统（ANFIS）的模型主体。
''' </summary>
''' <remarks>
''' <para>
''' 网络结构固定为 Takagi-Sugeno 五层：模糊化 → 乘积触发强度 → 归一化 → 一阶线性结论 →
''' 加权求和。可变参数分两类：**前提参数**（各隶属函数的中心/宽度，扁平成一个向量）
''' 与**结论参数**（每条规则一组线性系数，存成 <c>[P, outputs]</c> 矩阵，P = R*(n+1)）。
''' </para>
''' <para>
''' 之所以把规则结论写成矩阵：<c>z = A * theta</c>，其中 A 的第 r 块是
''' <c>[w-bar_r*x_1, ..., w-bar_r*x_n, w-bar_r]</c>。这一步把第 4、5 层合并成一次
''' 矩阵乘，于是"最小二乘估计结论参数"退化成解一个标准的线性最小二乘问题，
''' 而且多个输出可以共享同一个 A（只是多解几列）。
''' </para>
''' <para>
''' 所有对参数的就地修改都必须经过 <see cref="SetPremiseParameters"/> /
''' <see cref="Theta"/>，不要直接改数组：前者负责把扁平向量按输入切回各自隶属函数。
''' </para>
''' </remarks>
Public Class AnfisNetwork

    Private _theta As Double(,)
    Private ReadOnly _premiseOffsets As Integer()

    ''' <summary>按结构创建模型（结论参数初始化为 0，即输出恒为 0）。</summary>
    ''' <param name="inputs">输入变量（顺序即特征列顺序）</param>
    ''' <param name="rules">规则库</param>
    ''' <param name="outputCount">输出维数</param>
    ''' <param name="outputNames">输出名称（缺省为 <c>y1..yK</c>）</param>
    Public Sub New(inputs As AnfisInputVariable(), rules As RuleBase, outputCount As Integer, Optional outputNames As String() = Nothing)
        If inputs Is Nothing OrElse inputs.Length = 0 Then
            Throw New ArgumentException("the network requires at least one input variable", NameOf(inputs))
        End If
        If rules Is Nothing Then
            Throw New ArgumentNullException(NameOf(rules))
        End If
        If rules.InputCount <> inputs.Length Then
            Throw New ArgumentException($"the rule base has {rules.InputCount} antecedents but there are {inputs.Length} inputs", NameOf(rules))
        End If
        If outputCount <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(outputCount), outputCount, "the output count must be positive")
        End If

        For i As Integer = 0 To inputs.Length - 1
            If inputs(i) Is Nothing Then
                Throw New ArgumentException($"the input variable [{i}] is empty", NameOf(inputs))
            End If
        Next

        Me.Inputs = inputs
        Me.Rules = rules
        Me.OutputCount = outputCount
        Me.FeatureNames = inputs.Select(Function(v) v.Name).ToArray

        If outputNames Is Nothing OrElse outputNames.Length <> outputCount Then
            Me.OutputNames = Enumerable.Range(1, outputCount).Select(Function(i) $"y{i}").ToArray
        Else
            Me.OutputNames = outputNames
        End If

        _premiseOffsets = New Integer(inputs.Length - 1) {}
        Dim offset As Integer = 0

        For i As Integer = 0 To inputs.Length - 1
            _premiseOffsets(i) = offset
            offset += inputs(i).GetParameterCount()
        Next

        PremiseParameterCount = offset
        ConsequentParameterCount = rules.RuleCount * (inputs.Length + 1)
        _theta = New Double(ConsequentParameterCount - 1, outputCount - 1) {}
    End Sub

#Region "结构"

    ''' <summary>输入变量（顺序与特征列一致）。</summary>
    Public Property Inputs As AnfisInputVariable()
    ''' <summary>规则库。</summary>
    Public Property Rules As RuleBase
    ''' <summary>输出维数。</summary>
    Public Property OutputCount As Integer
    ''' <summary>特征列名称。</summary>
    Public Property FeatureNames As String()
    ''' <summary>输出名称。</summary>
    Public Property OutputNames As String()

    ''' <summary>输入维数 n。</summary>
    Public ReadOnly Property InputCount As Integer
        Get
            Return Inputs.Length
        End Get
    End Property

    ''' <summary>前提参数总数（所有隶属函数参数之和）。</summary>
    Public ReadOnly Property PremiseParameterCount As Integer
    ''' <summary>结论参数总数 <c>R*(n+1)</c>。</summary>
    Public ReadOnly Property ConsequentParameterCount As Integer

    ''' <summary>每个输入的前提参数在扁平向量里的起始下标。</summary>
    Public ReadOnly Property PremiseOffsets As Integer()
        Get
            Return _premiseOffsets
        End Get
    End Property

    ''' <summary>
    ''' 结论参数矩阵，形状 <c>[R*(n+1), OutputCount]</c>。
    ''' </summary>
    ''' <remarks>
    ''' 第 r 条规则占用第 <c>r*(n+1) .. r*(n+1)+n</c> 行：前 n 行是斜率、最后一行是常数项。
    ''' </remarks>
    Public Property Theta As Double(,)
        Get
            Return _theta
        End Get
        Set(value As Double(,))
            If value Is Nothing Then
                Throw New ArgumentNullException(NameOf(value))
            End If
            If value.GetLength(0) <> ConsequentParameterCount OrElse value.GetLength(1) <> OutputCount Then
                Throw New ArgumentException(
                    $"the consequent matrix must be ({ConsequentParameterCount}, {OutputCount})", NameOf(value))
            End If

            _theta = value
        End Set
    End Property

#End Region

#Region "前提参数"

    ''' <summary>按输入顺序拼接的前提参数向量。</summary>
    Public Function GetPremiseParameters() As Double()
        Dim values(PremiseParameterCount - 1) As Double

        For i As Integer = 0 To Inputs.Length - 1
            Dim part As Double() = Inputs(i).GetParameters()
            Call Array.Copy(part, 0, values, _premiseOffsets(i), part.Length)
        Next

        Return values
    End Function

    ''' <summary>把扁平向量按输入切回各自隶属函数。</summary>
    Public Sub SetPremiseParameters(values As Double())
        If values Is Nothing OrElse values.Length <> PremiseParameterCount Then
            Throw New ArgumentException($"the premise vector must have {PremiseParameterCount} elements", NameOf(values))
        End If

        For i As Integer = 0 To Inputs.Length - 1
            Dim count As Integer = Inputs(i).GetParameterCount()
            Dim part(count - 1) As Double

            Call Array.Copy(values, _premiseOffsets(i), part, 0, count)
            Call Inputs(i).SetParameters(part)
        Next
    End Sub

    ''' <summary>与 <see cref="GetPremiseParameters"/> 一一对应的"必须为正"标记。</summary>
    Public Function GetPremisePositiveMask() As Boolean()
        Dim mask(PremiseParameterCount - 1) As Boolean

        For i As Integer = 0 To Inputs.Length - 1
            Dim part As Boolean() = Inputs(i).GetPositiveMask()
            Call Array.Copy(part, 0, mask, _premiseOffsets(i), part.Length)
        Next

        Return mask
    End Function

    ''' <summary>与 <see cref="GetPremiseParameters"/> 一一对应的可读参数名。</summary>
    Public Function GetPremiseParameterNames() As String()
        Dim names(PremiseParameterCount - 1) As String

        For i As Integer = 0 To Inputs.Length - 1
            Dim part As String() = Inputs(i).GetParameterNames()
            Dim offset As Integer = _premiseOffsets(i)

            For p As Integer = 0 To part.Length - 1
                names(offset + p) = $"{Inputs(i).Name}.{part(p)}"
            Next
        Next

        Return names
    End Function

#End Region

#Region "推理"

    ''' <summary>批量预测。</summary>
    ''' <param name="x">样本矩阵（行 = 样本）</param>
    ''' <param name="chunkRows">每次前向的行数上限，控制临时内存</param>
    Public Function Predict(x As Double()(), Optional chunkRows As Integer = 1024) As Double()()
        If x Is Nothing Then
            Throw New ArgumentNullException(NameOf(x))
        End If

        Dim result(x.Length - 1)() As Double

        If x.Length = 0 Then
            Return result
        End If

        Dim layers As New AnfisLayers(Me)
        Dim chunkSize As Integer = If(chunkRows > 0, chunkRows, x.Length)

        For start As Integer = 0 To x.Length - 1 Step chunkSize
            Dim rows As Integer = std.Min(chunkSize, x.Length - start)
            Dim fwd As AnfisForward = layers.Forward(x, start, rows)

            For t As Integer = 0 To rows - 1
                Dim z(OutputCount - 1) As Double

                For o As Integer = 0 To OutputCount - 1
                    z(o) = fwd.Z(t, o)
                Next

                result(start + t) = z
            Next
        Next

        Return result
    End Function

    ''' <summary>单样本预测。</summary>
    Public Function PredictRow(x As Double()) As Double()
        Dim batch As Double()() = New Double()() {x}
        Dim z As Double()() = Predict(batch, 1)
        Return z(0)
    End Function

    ''' <summary>
    ''' 单样本的规则触发强度（归一化后），长度等于规则数。
    ''' </summary>
    ''' <remarks>
    ''' 这是 ANFIS 可解释性的主要抓手：哪些规则在这个样本上真正被激活、各自占多大权重，
    ''' 可以直接画出来，也可以用于剪枝决策。
    ''' </remarks>
    Public Function GetRuleActivations(x As Double()) As Double()
        Dim layers As New AnfisLayers(Me)
        Dim batch As Double()() = New Double()() {x}
        Dim fwd As AnfisForward = layers.Forward(batch, 0, 1)
        Dim result(fwd.RuleCount - 1) As Double

        For r As Integer = 0 To fwd.RuleCount - 1
            result(r) = fwd.Wbar(0, r)
        Next

        Return result
    End Function

    ''' <summary>单样本的隶属度矩阵，形状 <c>[InputCount, m_i]</c>。</summary>
    Public Function GetMembershipDegrees(x As Double()) As Double()()
        Dim layers As New AnfisLayers(Me)
        Dim batch As Double()() = New Double()() {x}
        Dim fwd As AnfisForward = layers.Forward(batch, 0, 1)
        Dim result(fwd.InputCount - 1)() As Double

        For i As Integer = 0 To fwd.InputCount - 1
            Dim m As Integer = fwd.Mu(i).GetLength(1)
            Dim row(m - 1) As Double

            For k As Integer = 0 To m - 1
                row(k) = fwd.Mu(i)(0, k)
            Next

            result(i) = row
        Next

        Return result
    End Function

#End Region

#Region "导出与持久化"

    ''' <summary>导出 FIS 风格的文本描述（便于人工检查与外部工具读取）。</summary>
    Public Function ExportFisText() As String
        Dim sb As New StringBuilder()
        Dim n As Integer = InputCount

        Call sb.AppendLine("[System]")
        Call sb.AppendLine("Name='anfis'")
        Call sb.AppendLine("Type='sugeno'")
        Call sb.AppendLine($"NumInputs={n}")
        Call sb.AppendLine($"NumOutputs={OutputCount}")
        Call sb.AppendLine($"NumRules={Rules.RuleCount}")
        Call sb.AppendLine("AndMethod='prod'")
        Call sb.AppendLine("OrMethod='probor'")
        Call sb.AppendLine("ImpMethod='prod'")
        Call sb.AppendLine("AggMethod='sum'")
        Call sb.AppendLine("DefuzzMethod='wtaver'")
        Call sb.AppendLine()

        For i As Integer = 0 To n - 1
            Dim variable As AnfisInputVariable = Inputs(i)
            Call sb.AppendLine($"[Input{i + 1}]")
            Call sb.AppendLine($"Name='{variable.Name}'")
            Call sb.AppendLine($"Range=[{variable.Minimum.ToString("G6")} {variable.Maximum.ToString("G6")}]")
            Call sb.AppendLine($"NumMFs={variable.FunctionCount}")

            For k As Integer = 0 To variable.FunctionCount - 1
                Dim values As String() = variable.Functions(k).GetParameters().Select(Function(v) v.ToString("G9")).ToArray
                Call sb.AppendLine($"MF{k + 1}='mf{k + 1}':'{MfTypeName(variable.Functions(k).Kind)}',[{String.Join(" ", values)}]")
            Next

            Call sb.AppendLine()
        Next

        For o As Integer = 0 To OutputCount - 1
            Call sb.AppendLine($"[Output{o + 1}]")
            Call sb.AppendLine($"Name='{OutputNames(o)}'")
            Call sb.AppendLine("Range=[-inf inf]")
            Call sb.AppendLine($"NumMFs={Rules.RuleCount}")

            For r As Integer = 0 To Rules.RuleCount - 1
                Dim offset As Integer = r * (n + 1)
                Dim values As String() = New String(n) {}

                For j As Integer = 0 To n
                    values(j) = Theta(offset + j, o).ToString("G9")
                Next

                Call sb.AppendLine($"MF{r + 1}='rule{r + 1}':'linear',[{String.Join(" ", values)}]")
            Next

            Call sb.AppendLine()
        Next

        Call sb.AppendLine("[Rules]")

        For r As Integer = 0 To Rules.RuleCount - 1
            Dim antecedents As String() = New String(n - 1) {}

            For i As Integer = 0 To n - 1
                antecedents(i) = (Rules.Antecedent(r, i) + 1).ToString()
            Next

            Call sb.AppendLine($"{String.Join(" ", antecedents)}, 1 ({1.0.ToString("G6")}) : 1")
        Next

        Return sb.ToString()
    End Function

    Private Shared Function MfTypeName(kind As MembershipKind) As String
        Select Case kind
            Case MembershipKind.Gaussian
                Return "gaussmf"
            Case MembershipKind.GeneralizedBell
                Return "gbellmf"
            Case MembershipKind.Sigmoid
                Return "sigmf"
            Case MembershipKind.Triangular
                Return "trimf"
            Case MembershipKind.Trapezoidal
                Return "trapmf"
            Case Else
                Return "unknown"
        End Select
    End Function

    ''' <summary>序列化为 JSON。</summary>
    Public Function ToJson(Optional indented As Boolean = True) As String
        Dim dto As New AnfisModelDto With {
            .formatVersion = "1.0",
            .featureNames = FeatureNames,
            .outputNames = OutputNames,
            .inputs = Inputs.Select(Function(v) New AnfisInputDto With {
                .name = v.Name,
                .minimum = v.Minimum,
                .maximum = v.Maximum,
                .functions = v.Functions.Select(Function(mf) New AnfisFunctionDto With {
                    .kind = mf.Kind.ToString(),
                    .parameters = mf.GetParameters()
                }).ToArray
            }).ToArray,
            .antecedent = Rules.ToAntecedentTable(),
            .theta = ToJagged(Theta)
        }

        Dim options As New JsonSerializerOptions With {.WriteIndented = indented}
        Return JsonSerializer.Serialize(dto, options)
    End Function

    ''' <summary>从 JSON 反序列化。</summary>
    Public Shared Function FromJson(json As String) As AnfisNetwork
        If String.IsNullOrEmpty(json) Then
            Throw New ArgumentException("the json text is empty", NameOf(json))
        End If

        Dim dto As AnfisModelDto = JsonSerializer.Deserialize(Of AnfisModelDto)(json)

        If dto Is Nothing OrElse dto.inputs Is Nothing OrElse dto.antecedent Is Nothing OrElse dto.theta Is Nothing Then
            Throw New InvalidOperationException("the json text does not describe an ANFIS model")
        End If

        Dim inputs As AnfisInputVariable() = dto.inputs.Select(Function(d)
            Dim funcs As IMembershipFunction() = d.functions.Select(Function(mf)
                Dim kind As MembershipKind = CType([Enum].Parse(GetType(MembershipKind), mf.kind, ignoreCase:=True), MembershipKind)
                Return MembershipFactory.Create(kind, mf.parameters)
            End Function).ToArray

            Return New AnfisInputVariable(d.name, d.minimum, d.maximum, funcs)
        End Function).ToArray

        Dim antecedent As Integer(,) = New Integer(dto.antecedent.Length - 1, inputs.Length - 1) {}

        For r As Integer = 0 To dto.antecedent.Length - 1
            If dto.antecedent(r).Length <> inputs.Length Then
                Throw New InvalidOperationException(
                    $"the rule {r + 1} has {dto.antecedent(r).Length} antecedents but {inputs.Length} inputs were defined")
            End If

            For i As Integer = 0 To inputs.Length - 1
                antecedent(r, i) = dto.antecedent(r)(i)
            Next
        Next

        Dim outputCount As Integer = dto.theta(0).Length
        Dim outputNames As String() = dto.outputNames

        If outputNames Is Nothing OrElse outputNames.Length <> outputCount Then
            outputNames = Enumerable.Range(1, outputCount).Select(Function(i) $"y{i}").ToArray
        End If

        Dim network As New AnfisNetwork(inputs, New RuleBase(antecedent), outputNames.Length, outputNames) With {
            .FeatureNames = If(dto.featureNames, inputs.Select(Function(v) v.Name).ToArray),
            .Theta = FromJagged(dto.theta)
        }

        Return network
    End Function

    ''' <summary>深拷贝。</summary>
    Public Function Clone() As AnfisNetwork
        Dim inputs As AnfisInputVariable() = Me.Inputs.Select(Function(v) v.Clone()).ToArray
        Dim copy As New AnfisNetwork(inputs, Rules.Clone(), OutputCount, CType(OutputNames.Clone(), String())) With {
            .FeatureNames = CType(FeatureNames.Clone(), String()),
            .Theta = CType(Theta.Clone(), Double(,))
        }

        Return copy
    End Function

    Private Shared Function ToJagged(values As Double(,)) As Double()()
        Dim rows As Integer = values.GetLength(0)
        Dim cols As Integer = values.GetLength(1)
        Dim result(rows - 1)() As Double

        For i As Integer = 0 To rows - 1
            Dim row(cols - 1) As Double

            For j As Integer = 0 To cols - 1
                row(j) = values(i, j)
            Next

            result(i) = row
        Next

        Return result
    End Function

    Private Shared Function FromJagged(values As Double()()) As Double(,)
        Dim rows As Integer = values.Length
        Dim cols As Integer = If(rows = 0, 0, values(0).Length)
        Dim result(rows - 1, cols - 1) As Double

        For i As Integer = 0 To rows - 1
            If values(i).Length <> cols Then
                Throw New InvalidOperationException($"the row {i} has {values(i).Length} values but {cols} were expected")
            End If

            For j As Integer = 0 To cols - 1
                result(i, j) = values(i)(j)
            Next
        Next

        Return result
    End Function

#End Region

    ''' <summary>可读描述。</summary>
    Public Overrides Function ToString() As String
        Return $"ANFIS {InputCount} inputs x {Rules.RuleCount} rules -> {OutputCount} outputs " &
               $"({PremiseParameterCount} premise + {ConsequentParameterCount * OutputCount} consequent parameters)"
    End Function

End Class

''' <summary>ANFIS 模型的 JSON 传输对象。</summary>
Public Class AnfisModelDto

    ''' <summary>格式版本，便于将来演进。</summary>
    Public Property formatVersion As String
    ''' <summary>特征列名称。</summary>
    Public Property featureNames As String()
    ''' <summary>输出名称。</summary>
    Public Property outputNames As String()
    ''' <summary>输入变量定义。</summary>
    Public Property inputs As AnfisInputDto()
    ''' <summary>规则表（行 = 规则，列 = 输入，值为模糊集下标）。</summary>
    Public Property antecedent As Integer()()
    ''' <summary>结论参数矩阵（行主序展平）。</summary>
    Public Property theta As Double()()

End Class

''' <summary>输入变量的 JSON 传输对象。</summary>
Public Class AnfisInputDto

    ''' <summary>变量名。</summary>
    Public Property name As String
    ''' <summary>定义域下界。</summary>
    Public Property minimum As Double
    ''' <summary>定义域上界。</summary>
    Public Property maximum As Double
    ''' <summary>隶属函数列表。</summary>
    Public Property functions As AnfisFunctionDto()

End Class

''' <summary>隶属函数的 JSON 传输对象。</summary>
Public Class AnfisFunctionDto

    ''' <summary>类型名称（<see cref="MembershipKind"/> 的名字）。</summary>
    Public Property kind As String
    ''' <summary>参数向量。</summary>
    Public Property parameters As Double()

End Class
