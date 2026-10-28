Option Strict On
Option Explicit On

Imports System
Imports System.Linq

Namespace TSK

    ''' <summary>
    ''' 按网格划分从训练数据构建 TSK 网络结构（不做任何训练）。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 每个输入等距放 m 个隶属函数，规则数 <c>m^n</c>。网格划分的优点是规则可解释、
    ''' 前向完全向量化；缺点是规则数随维数指数增长，
    ''' 因此 <see cref="TskStructureOptions.MaxRules"/> 与 <see cref="TskStructureOptions.MaxConsequentParams"/>
    ''' 会在超限时直接报错并给出可操作的提示。
    ''' </para>
    ''' <para>
    ''' 定义域统计**只使用传入的数据**。做交叉验证时请对训练折调用本方法，
    ''' 不要用全量数据建模再把同一结构套到每一折上（那会泄漏验证折的分布信息）。
    ''' </para>
    ''' </remarks>
    Public Module TskNetworkBuilder

        ''' <summary>
        ''' 按数据的取值范围与配置构建模型结构（不训练）。
        ''' </summary>
        ''' <param name="x">样本矩阵（行 = 样本，列 = 输入）</param>
        ''' <param name="y">目标矩阵（行 = 样本，列 = 输出），只用于读取输出维数与名称</param>
        ''' <param name="options">结构配置</param>
        Public Function CreateGrid(x As Double()(), y As Double()(), Optional options As TskStructureOptions = Nothing) As TskNetwork
            If x Is Nothing OrElse x.Length = 0 Then
                Throw New ArgumentException("the feature matrix is empty", NameOf(x))
            End If
            If y Is Nothing OrElse y.Length <> x.Length Then
                Throw New ArgumentException("the target does not match the feature matrix", NameOf(y))
            End If

            Dim config As TskStructureOptions = If(options, New TskStructureOptions()).ValidateStructure()
            Dim n As Integer = x(0).Length
            Dim outputCount As Integer = y(0).Length

            Dim mfCounts(n - 1) As Integer

            If config.MfCounts IsNot Nothing Then
                If config.MfCounts.Length <> n Then
                    Throw New ArgumentException($"MfCounts has {config.MfCounts.Length} entries but there are {n} inputs", NameOf(options))
                End If

                Call Array.Copy(config.MfCounts, mfCounts, n)
            Else
                For i As Integer = 0 To n - 1
                    mfCounts(i) = config.MfCount
                Next
            End If

            Dim names As String() = config.FeatureNames

            If names Is Nothing OrElse names.Length <> n Then
                names = Enumerable.Range(1, n).Select(Function(i) $"x{i}").ToArray
            End If

            Dim inputs(n - 1) As TskInputVariable

            For i As Integer = 0 To n - 1
                Dim minimum As Double = Double.MaxValue
                Dim maximum As Double = Double.MinValue

                For r As Integer = 0 To x.Length - 1
                    Dim v As Double = x(r)(i)

                    If v < minimum Then
                        minimum = v
                    End If
                    If v > maximum Then
                        maximum = v
                    End If
                Next

                inputs(i) = TskInputVariable.CreateGrid(names(i), minimum, maximum, mfCounts(i), config.MfKind)
            Next

            Dim rules As FuzzyRuleBase = FuzzyRuleBase.CreateFullGrid(mfCounts, config.RuleEnableMask, config.MaxRules)
            Dim consequentCount As Integer = rules.RuleCount * (n + 1)

            If consequentCount > config.MaxConsequentParams Then
                Throw New InvalidOperationException(
                    $"{rules.RuleCount} rules over {n} inputs need {consequentCount} consequent parameters, " &
                    $"which exceeds MaxConsequentParams = {config.MaxConsequentParams}. " &
                    $"The least squares normal equations would need {consequentCount}^2 doubles of memory; " &
                    $"reduce MfCount, prune rules, or switch to a sequential update strategy.")
            End If

            Dim outputNames As String() = config.OutputNames

            If outputNames Is Nothing OrElse outputNames.Length <> outputCount Then
                outputNames = Nothing
            End If

            Return New TskNetwork(inputs, rules, outputCount, outputNames)
        End Function

        ''' <summary>
        ''' 用显式给定的输入变量与规则表直接组装网络（用于从既有 Mamdani 模型或专家规则迁移）。
        ''' </summary>
        ''' <param name="inputs">输入变量（顺序即特征列顺序）</param>
        ''' <param name="rules">规则表</param>
        ''' <param name="outputCount">输出维数</param>
        ''' <param name="outputNames">输出名称，<c>Nothing</c> 时自动生成</param>
        Public Function FromRuleTable(inputs As TskInputVariable(),
                                      rules As FuzzyRuleBase,
                                      Optional outputCount As Integer = 1,
                                      Optional outputNames As String() = Nothing) As TskNetwork

            Return New TskNetwork(inputs, rules, outputCount, outputNames)
        End Function

    End Module

End Namespace
