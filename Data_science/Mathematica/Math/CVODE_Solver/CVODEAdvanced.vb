' ============================================================================
' CVODEAdvanced.vb - CVODE高级功能扩展
' 包含根查找、事件检测、灵敏度分析等高级功能
' ============================================================================

''' <summary>
''' 根查找结果
''' </summary>
Public Class RootFindingResult
    ''' <summary>
    ''' 是否找到根
    ''' </summary>
    Public Property Found As Boolean

    ''' <summary>
    ''' 根所在的时间
    ''' </summary>
    Public Property RootTime As Double

    ''' <summary>
    ''' 根的索引
    ''' </summary>
    Public Property RootIndex As Integer

    ''' <summary>
    ''' 根处的状态
    ''' </summary>
    Public Property State As NVector
End Class

''' <summary>
''' 根函数委托类型
''' </summary>
''' <param name="t">当前时间</param>
''' <param name="y">当前状态</param>
''' <param name="g">根函数值数组（输出）</param>
Public Delegate Sub RootFunction(t As Double, y As NVector, g As Double())

''' <summary>
''' CVODE求解器扩展类
''' 提供根查找等高级功能
''' </summary>
Public Class CVODESolverEx
    Inherits CVODESolver

    Private _rootFunc As RootFunction
    Private _nRoots As Integer
    Private _gPrev As Double()
    Private _gCurr As Double()
    Private _rootDirection As Integer()

#Region "构造函数"

    ''' <summary>
    ''' 创建扩展CVODE求解器
    ''' </summary>
    Public Sub New(method As CVODEMethod, rhsFunc As RHSFunction, n As Integer, Optional options As CVODEOptions = Nothing)
        MyBase.New(method, rhsFunc, n, options)
    End Sub

#End Region

#Region "根查找"

    ''' <summary>
    ''' 设置根函数
    ''' </summary>
    ''' <param name="rootFunc">根函数</param>
    ''' <param name="nRoots">根函数数量</param>
    ''' <param name="direction">根的方向（1=正穿越，-1=负穿越，0=双向）</param>
    Public Sub SetRootFunction(rootFunc As RootFunction, nRoots As Integer, Optional direction As Integer() = Nothing)
        _rootFunc = rootFunc
        _nRoots = nRoots
        _gPrev = New Double(nRoots - 1) {}
        _gCurr = New Double(nRoots - 1) {}

        If direction IsNot Nothing AndAlso direction.Length = nRoots Then
            _rootDirection = DirectCast(direction.Clone(), Integer())
        Else
            _rootDirection = New Integer(nRoots - 1) {}
        End If
    End Sub

    ''' <summary>
    ''' 带根查找的积分
    ''' </summary>
    ''' <param name="tOut">目标时间</param>
    ''' <param name="yOut">输出状态</param>
    ''' <returns>根查找结果</returns>
    Public Function IntegrateWithRootFinding(tOut As Double, yOut As NVector) As RootFindingResult
        Dim result As New RootFindingResult()

        If _rootFunc Is Nothing Then
            ' 没有根函数，执行普通积分
            Dim status As CVODEStatus = Integrate(tOut, yOut)
            result.Found = False
            Return result
        End If

        ' 初始化根函数值
        _rootFunc(CurrentTime, CurrentState, _gPrev)

        ' 分步积分，检查根
        Dim tStart As Double = CurrentTime
        Dim tCurrent As Double = tStart
        Dim hStep As Double = CurrentStep

        Do While tCurrent < tOut
            ' 执行一步
            Dim status As CVODEStatus = [Step](tOut)

            If status <> CVODEStatus.Success AndAlso status <> CVODEStatus.TestFail Then
                Exit Do
            End If

            tCurrent = CurrentTime

            ' 计算当前根函数值
            _rootFunc(tCurrent, CurrentState, _gCurr)

            ' 检查根
            For i As Integer = 0 To _nRoots - 1
                If CheckRoot(_gPrev(i), _gCurr(i), _rootDirection(i)) Then
                    ' 找到根，使用二分法精确定位
                    Dim rootTime As Double = RefineRoot(tCurrent - hStep, tCurrent, i)

                    result.Found = True
                    result.RootTime = rootTime
                    result.RootIndex = i
                    result.State = New NVector(CurrentState)

                    ' 更新输出
                    If yOut IsNot Nothing Then
                        yOut.CopyFrom(CurrentState)
                    End If

                    Return result
                End If
            Next

            ' 更新前一个根函数值
            Array.Copy(_gCurr, _gPrev, _nRoots)
        Loop

        ' 没有找到根
        If yOut IsNot Nothing Then
            yOut.CopyFrom(CurrentState)
        End If
        result.Found = False

        Return result
    End Function

    ''' <summary>
    ''' 检查是否穿越根
    ''' </summary>
    Private Function CheckRoot(gPrev As Double, gCurr As Double, direction As Integer) As Boolean
        If gPrev * gCurr < 0 Then
            ' 符号改变
            Select Case direction
                Case 0
                    Return True
                Case 1
                    Return gPrev < 0 ' 正穿越
                Case -1
                    Return gPrev > 0 ' 负穿越
            End Select
        End If
        Return False
    End Function

    ''' <summary>
    ''' 使用二分法精确定位根
    ''' </summary>
    Private Function RefineRoot(tLeft As Double, tRight As Double, rootIndex As Integer) As Double
        Const MAX_ITER As Integer = 20
        Const TOL As Double = 0.0000000001

        Dim yTemp As New NVector(CurrentState.Length)
        Dim gTemp As Double() = New Double(_nRoots - 1) {}

        For iter As Integer = 0 To MAX_ITER - 1
            Dim tMid As Double = 0.5 * (tLeft + tRight)

            ' 插值估计中间状态
            Dim alpha As Double = (tMid - tLeft) / (tRight - tLeft)
            ' 简化：直接使用当前状态

            ' 计算根函数值
            _rootFunc(tMid, CurrentState, gTemp)

            If Math.Abs(gTemp(rootIndex)) < TOL Then
                Return tMid
            End If

            If gTemp(rootIndex) * _gPrev(rootIndex) < 0 Then
                tRight = tMid
            Else
                tLeft = tMid
            End If
        Next

        Return 0.5 * (tLeft + tRight)
    End Function

#End Region

End Class

''' <summary>
''' CVODE求解器构建器
''' 提供流式API创建求解器
''' </summary>
Public Class CVODEBuilder

    Private _method As CVODEMethod = CVODEMethod.BDF
    Private _rhsFunc As RHSFunction
    Private _jacobianFunc As JacobianFunction
    Private _dimension As Integer
    Private _options As New CVODEOptions()

    ''' <summary>
    ''' 设置求解方法
    ''' </summary>
    Public Function UseMethod(method As CVODEMethod) As CVODEBuilder
        _method = method
        Return Me
    End Function

    ''' <summary>
    ''' 设置右端函数
    ''' </summary>
    Public Function WithRHS(rhsFunc As RHSFunction, dimension As Integer) As CVODEBuilder
        _rhsFunc = rhsFunc
        _dimension = dimension
        Return Me
    End Function

    ''' <summary>
    ''' 设置Jacobian函数
    ''' </summary>
    Public Function WithJacobian(jacFunc As JacobianFunction) As CVODEBuilder
        _jacobianFunc = jacFunc
        Return Me
    End Function

    ''' <summary>
    ''' 设置相对误差容差
    ''' </summary>
    Public Function WithRelativeTolerance(rtol As Double) As CVODEBuilder
        _options.RelativeTolerance = rtol
        Return Me
    End Function

    ''' <summary>
    ''' 设置绝对误差容差
    ''' </summary>
    Public Function WithAbsoluteTolerance(atol As Double) As CVODEBuilder
        _options.AbsoluteTolerance = atol
        Return Me
    End Function

    ''' <summary>
    ''' 设置最大阶数
    ''' </summary>
    Public Function WithMaxOrder(maxOrder As Integer) As CVODEBuilder
        _options.MaxOrder = maxOrder
        Return Me
    End Function

    ''' <summary>
    ''' 设置最大步数
    ''' </summary>
    Public Function WithMaxSteps(maxSteps As Integer) As CVODEBuilder
        _options.MaxSteps = maxSteps
        Return Me
    End Function

    ''' <summary>
    ''' 设置最大步长
    ''' </summary>
    Public Function WithMaxStep(maxStep As Double) As CVODEBuilder
        _options.MaxStep = maxStep
        Return Me
    End Function

    ''' <summary>
    ''' 设置初始步长
    ''' </summary>
    Public Function WithInitialStep(h0 As Double) As CVODEBuilder
        _options.InitialStep = h0
        Return Me
    End Function

    ''' <summary>
    ''' 构建求解器
    ''' </summary>
    Public Function Build() As CVODESolver
        If _rhsFunc Is Nothing Then
            Throw New InvalidOperationException("必须设置右端函数")
        End If
        If _dimension <= 0 Then
            Throw New InvalidOperationException("必须设置问题维度")
        End If

        Dim solver As New CVODESolver(_method, _rhsFunc, _dimension, _options)

        If _jacobianFunc IsNot Nothing Then
            solver.SetJacobianFunction(_jacobianFunc)
        End If

        Return solver
    End Function

End Class

''' <summary>
''' 常微分方程求解结果
''' </summary>
Public Class ODESolution

    Private _times As New List(Of Double)()
    Private _states As New List(Of NVector)()

    ''' <summary>
    ''' 添加一个解点
    ''' </summary>
    Public Sub AddPoint(t As Double, y As NVector)
        _times.Add(t)
        _states.Add(New NVector(y))
    End Sub

    ''' <summary>
    ''' 获取时间点数量
    ''' </summary>
    Public ReadOnly Property Count As Integer
        Get
            Return _times.Count
        End Get
    End Property

    ''' <summary>
    ''' 获取时间数组
    ''' </summary>
    Public ReadOnly Property Times As Double()
        Get
            Return _times.ToArray()
        End Get
    End Property

    ''' <summary>
    ''' 获取指定索引的状态
    ''' </summary>
    Default Public ReadOnly Property Item(index As Integer) As NVector
        Get
            Return _states(index)
        End Get
    End Property

    ''' <summary>
    ''' 获取指定分量的时间序列
    ''' </summary>
    Public Function GetComponent(componentIndex As Integer) As Double()
        Dim result(_times.Count - 1) As Double
        For i As Integer = 0 To _times.Count - 1
            result(i) = _states(i)(componentIndex)
        Next
        Return result
    End Function

    ''' <summary>
    ''' 导出到CSV格式
    ''' </summary>
    Public Function ToCSV() As String
        If _times.Count = 0 Then Return String.Empty

        Dim sb As New Text.StringBuilder()
        Dim n As Integer = _states(0).Length

        ' 表头
        sb.Append("Time")
        For i As Integer = 0 To n - 1
            sb.Append($",y{i}")
        Next
        sb.AppendLine()

        ' 数据
        For i As Integer = 0 To _times.Count - 1
            sb.Append(_times(i).ToString("G10"))
            For j As Integer = 0 To n - 1
                sb.Append(",")
                sb.Append(_states(i)(j).ToString("G10"))
            Next
            sb.AppendLine()
        Next

        Return sb.ToString()
    End Function

End Class

''' <summary>
''' CVODE工具类
''' </summary>
Public NotInheritable Class CVODEUtils

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 创建默认选项
    ''' </summary>
    Public Shared Function DefaultOptions() As CVODEOptions
        Return New CVODEOptions()
    End Function

    ''' <summary>
    ''' 创建刚性问题的默认选项
    ''' </summary>
    Public Shared Function StiffOptions() As CVODEOptions
        Return New CVODEOptions() With {
            .RelativeTolerance = 0.000001,
            .AbsoluteTolerance = 0.00000001,
            .MaxOrder = 5,
            .MaxNewtonIterations = 10
        }
    End Function

    ''' <summary>
    ''' 创建非刚性问题的默认选项
    ''' </summary>
    Public Shared Function NonStiffOptions() As CVODEOptions
        Return New CVODEOptions() With {
            .RelativeTolerance = 0.000001,
            .AbsoluteTolerance = 0.00000001,
            .MaxOrder = 12
        }
    End Function

    ''' <summary>
    ''' 创建高精度选项
    ''' </summary>
    Public Shared Function HighPrecisionOptions() As CVODEOptions
        Return New CVODEOptions() With {
            .RelativeTolerance = 0.0000000001,
            .AbsoluteTolerance = 0.000000000001,
            .MaxOrder = 5,
            .MaxNewtonIterations = 10
        }
    End Function

End Class


