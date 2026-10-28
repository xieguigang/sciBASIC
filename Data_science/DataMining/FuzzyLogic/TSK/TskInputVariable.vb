Option Strict On
Option Explicit On

Imports System

Namespace TSK

    ''' <summary>
    ''' TSK 推理的一个输入变量：名称、定义域、以及按定义域划分出来的若干隶属函数。
    ''' </summary>
    ''' <remarks>
    ''' 定义域应当来自**训练集**（见 <see cref="TskNetworkBuilder"/>），不能包含验证集的信息，
    ''' 否则隶属函数的初始位置会泄漏标签/分布信息，交叉验证的评估结果会偏乐观。
    ''' </remarks>
    Public Class TskInputVariable

        ''' <summary>变量名（与特征列名一致）。</summary>
        Public Property Name As String
        ''' <summary>定义域下界（训练集最小值）。</summary>
        Public Property Minimum As Double
        ''' <summary>定义域上界（训练集最大值）。</summary>
        Public Property Maximum As Double
        ''' <summary>隶属函数列表，下标即规则表里引用的模糊集编号。</summary>
        Public Property Functions As IMembershipFunction()

        ''' <summary>创建输入变量。</summary>
        Public Sub New(name As String, minimum As Double, maximum As Double, functions As IMembershipFunction())
            If String.IsNullOrEmpty(name) Then
                Throw New ArgumentException("the input variable requires a name", NameOf(name))
            End If
            If functions Is Nothing OrElse functions.Length = 0 Then
                Throw New ArgumentException("the input variable requires at least one membership function", NameOf(functions))
            End If

            Me.Name = name
            Me.Minimum = minimum
            Me.Maximum = maximum
            Me.Functions = functions
        End Sub

        ''' <summary>隶属函数个数（即该输入的模糊划分数量 m）。</summary>
        Public ReadOnly Property FunctionCount As Integer
            Get
                If Functions Is Nothing Then
                    Return 0
                Else
                    Return Functions.Length
                End If
            End Get
        End Property

        ''' <summary>隶属函数类型（同一个变量内所有函数类型一致）。</summary>
        Public ReadOnly Property Kind As MembershipKind
            Get
                Return Functions(0).Kind
            End Get
        End Property

        ''' <summary>定义域跨度。</summary>
        Public ReadOnly Property Range As Double
            Get
                Return Maximum - Minimum
            End Get
        End Property

        ''' <summary>
        ''' 按等距中心初始化一组隶属函数。
        ''' </summary>
        ''' <param name="name">变量名</param>
        ''' <param name="minimum">定义域下界</param>
        ''' <param name="maximum">定义域上界</param>
        ''' <param name="count">隶属函数个数，推荐 2~4</param>
        ''' <param name="kind">隶属函数类型</param>
        Public Shared Function CreateGrid(name As String,
                                          minimum As Double,
                                          maximum As Double,
                                          count As Integer,
                                          kind As MembershipKind) As TskInputVariable

            If count <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(count), count, "the membership function count must be positive")
            End If

            Dim low As Double = minimum
            Dim high As Double = maximum

            If high < low Then
                low = maximum
                high = minimum
            End If

            Dim span As Double = high - low
            Dim spacing As Double

            If count = 1 Then
                ' 只有一个模糊集时没有"间距"可言，用整个定义域跨度作为特征宽度
                spacing = If(span > 0.0, span, 1.0)
            Else
                spacing = span / (count - 1)

                ' 常数列（min == max）会让间距为 0，此时退化为单位间距，避免除零
                If spacing <= 0.0 Then
                    spacing = 1.0
                End If
            End If

            Dim funcs As IMembershipFunction() = New IMembershipFunction(count - 1) {}

            For k As Integer = 0 To count - 1
                Dim center As Double

                If count = 1 Then
                    center = (low + high) / 2.0
                Else
                    center = low + k * spacing
                End If

                funcs(k) = MembershipFactory.CreateGrid(kind, center, spacing)
            Next

            Return New TskInputVariable(name, low, high, funcs)
        End Function

        ''' <summary>该变量所有隶属函数的参数个数之和。</summary>
        Public Function GetParameterCount() As Integer
            Dim total As Integer = 0

            For k As Integer = 0 To FunctionCount - 1
                total += Functions(k).ParameterCount
            Next

            Return total
        End Function

        ''' <summary>该变量所有隶属函数的参数按顺序拼接成一个向量。</summary>
        Public Function GetParameters() As Double()
            Dim total As Integer = GetParameterCount()
            Dim values(total - 1) As Double
            Dim offset As Integer = 0

            For k As Integer = 0 To FunctionCount - 1
                Dim p As Double() = Functions(k).GetParameters()

                Call Array.Copy(p, 0, values, offset, p.Length)
                offset += p.Length
            Next

            Return values
        End Function

        ''' <summary>用拼接向量写回所有隶属函数的参数。</summary>
        Public Sub SetParameters(values As Double())
            Dim total As Integer = GetParameterCount()

            If values Is Nothing OrElse values.Length <> total Then
                Throw New ArgumentException($"the input variable '{Name}' expects {total} parameters", NameOf(values))
            End If

            Dim offset As Integer = 0

            For k As Integer = 0 To FunctionCount - 1
                Dim pc As Integer = Functions(k).ParameterCount
                Dim part(pc - 1) As Double

                Call Array.Copy(values, offset, part, 0, pc)
                Call Functions(k).SetParameters(part)
                offset += pc
            Next
        End Sub

        ''' <summary>与 <see cref="GetParameters"/> 一一对应的"必须为正"标记。</summary>
        Public Function GetPositiveMask() As Boolean()
            Dim total As Integer = GetParameterCount()
            Dim mask(total - 1) As Boolean
            Dim offset As Integer = 0

            For k As Integer = 0 To FunctionCount - 1
                For p As Integer = 0 To Functions(k).ParameterCount - 1
                    mask(offset + p) = Functions(k).IsPositiveParameter(p)
                Next

                offset += Functions(k).ParameterCount
            Next

            Return mask
        End Function

        ''' <summary>与 <see cref="GetParameters"/> 一一对应的参数名称（形如 <c>mf2.width</c>）。</summary>
        Public Function GetParameterNames() As String()
            Dim total As Integer = GetParameterCount()
            Dim names(total - 1) As String
            Dim offset As Integer = 0

            For k As Integer = 0 To FunctionCount - 1
                Dim mfNames As String() = Functions(k).ParameterNames()

                For p As Integer = 0 To mfNames.Length - 1
                    names(offset + p) = $"mf{k + 1}.{mfNames(p)}"
                Next

                offset += mfNames.Length
            Next

            Return names
        End Function

        ''' <summary>第 <paramref name="index"/> 个模糊集的可读标签，用于导出 IF-THEN 规则。</summary>
        Public Function FunctionLabel(index As Integer) As String
            Return $"mf{index + 1}"
        End Function

        ''' <summary>深拷贝（隶属函数逐个克隆）。</summary>
        Public Function Clone() As TskInputVariable
            Dim funcs(FunctionCount - 1) As IMembershipFunction

            For k As Integer = 0 To FunctionCount - 1
                funcs(k) = Functions(k).Clone()
            Next

            Return New TskInputVariable(Name, Minimum, Maximum, funcs)
        End Function

        ''' <summary>可读描述。</summary>
        Public Overrides Function ToString() As String
            Dim parts As String() = New String(FunctionCount - 1) {}

            For k As Integer = 0 To FunctionCount - 1
                parts(k) = Functions(k).Describe()
            Next

            Return $"{Name} [{Minimum:G4}, {Maximum:G4}]: {String.Join(" | ", parts)}"
        End Function

    End Class

End Namespace
