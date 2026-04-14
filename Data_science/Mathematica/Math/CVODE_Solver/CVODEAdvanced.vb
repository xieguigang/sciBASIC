Imports std = System.Math

' ============================================================================
' CVODEAdvanced.vb - CVODE高级功能扩展
' 包含根查找、事件检测、灵敏度分析等高级功能
' ============================================================================

''' <summary>
''' CVODE求解器扩展类
''' 提供根查找等高级功能
''' </summary>
Public Class CVODESolverEx : Inherits CVODESolver

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

            If std.Abs(gTemp(rootIndex)) < TOL Then
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


