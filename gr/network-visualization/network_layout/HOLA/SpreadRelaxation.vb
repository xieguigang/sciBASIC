Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Language

Namespace Hola

    ''' <summary>
    ''' HOLA 阶段 5：扩散松弛（Spread Relaxation）。
    ''' 在两个坐标轴上分别以一维扫描排序的方式施加分离约束（相邻节点间距 >= nodeGap），
    ''' 借助 CoLa 求解器投影，消除节点与边的重叠，使布局疏密得当。
    ''' </summary>
    Public Module SpreadRelaxation

        ''' <summary>
        ''' 执行扩散松弛：先沿 x 轴、再沿 y 轴扫描施加分离约束并投影求解，
        ''' 重复若干轮直到位移收敛或达到最大迭代。
        ''' </summary>
        Public Sub Relax(state As HolaLayoutState, opts As HolaOptions)
            For iter As Integer = 1 To opts.maxIterations
                Dim mx = SpreadAxis(state, ConstraintHelper.Axis.Horizontal, opts)
                Dim my = SpreadAxis(state, ConstraintHelper.Axis.Vertical, opts)

                If System.Math.Max(mx, my) < opts.convergeEpsilon Then
                    Exit For
                End If
            Next
        End Sub

        ''' <summary>
        ''' 在单一坐标轴上扫描：按坐标排序后，对相邻节点施加最小间距约束并投影。
        ''' </summary>
        Private Function SpreadAxis(state As HolaLayoutState, ax As ConstraintHelper.Axis, opts As HolaOptions) As Double
            Dim order(state.nodes.Length - 1) As Integer
            For i As Integer = 0 To order.Length - 1
                order(i) = i
            Next

            Array.Sort(order, Function(a As Integer, b As Integer)
                                  Return ConstraintHelper.GetCoord(state.positions(a), ax).CompareTo(
                                         ConstraintHelper.GetCoord(state.positions(b), ax))
                              End Function)

            ' 相邻节点之间的分离约束：后一个 - 前一个 >= nodeGap
            Dim pairs(state.nodes.Length - 2) As (left As Integer, right As Integer, gap As Double)
            For k As Integer = 0 To order.Length - 2
                pairs(k) = (order(k), order(k + 1), opts.nodeGap)
            Next

            Return ConstraintHelper.ProjectConstraints(state, ax, pairs, equality:=False, opts)
        End Function
    End Module
End Namespace
