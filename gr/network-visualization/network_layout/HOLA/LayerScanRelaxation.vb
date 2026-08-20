Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Language

Namespace Hola

    ''' <summary>
    ''' HOLA 阶段 2/3：分层扫描松弛（Layer-wise Scan Relaxation）。
    ''' 通过几何方式检测相交的连线（边），对相交边涉及到的节点施加分离/对齐约束，
    ''' 并借助 CoLa 求解器投影，从而逐步消除边交叉，使布局趋向"人类可读"的正交形态。
    ''' </summary>
    Public Module LayerScanRelaxation

        ''' <summary>
        ''' 执行分层扫描松弛。重复若干轮：检测所有相交边对，生成分离约束并投影求解，
        ''' 直到没有交叉或达到最大迭代次数。
        ''' </summary>
        Public Sub Relax(state As HolaLayoutState, opts As HolaOptions)
            For iter As Integer = 1 To opts.maxIterations
                Dim crossing = FindCrossingPairs(state)

                If crossing.Length = 0 Then
                    Exit For
                End If

                ' 对每条相交边的一端节点施加沿 y 轴的分离，使两条线错开
                Dim pairs As New List(Of (left As Integer, right As Integer, gap As Double))
                For Each cp In crossing
                    Dim e1u = cp.e1u : Dim e1v = cp.e1v
                    Dim e2u = cp.e2u : Dim e2v = cp.e2v

                    ' 让两条边各自的两个端点沿 y 轴拉开，避免它们处在同一水平带
                    pairs.Add((e1u, e1v, opts.desiredEdgeLength))
                    pairs.Add((e2u, e2v, opts.desiredEdgeLength))
                Next

                Dim maxMove = ConstraintHelper.ProjectConstraints(state, ConstraintHelper.Axis.Vertical, pairs.ToArray, equality:=False, opts)

                If maxMove < opts.convergeEpsilon Then
                    Exit For
                End If
            Next
        End Sub

        ''' <summary>
        ''' 检测当前布局中所有几何相交的无向边对。两线段 (u-v) 与 (a-b) 相交即记录。
        ''' </summary>
        Private Function FindCrossingPairs(state As HolaLayoutState) As (e1u As Integer, e1v As Integer, e2u As Integer, e2v As Integer)()
            Dim result As New List(Of (e1u As Integer, e1v As Integer, e2u As Integer, e2v As Integer))
            Dim es = state.edges

            For i As Integer = 0 To es.Length - 1
                Dim p1 = state.positions(es(i).u)
                Dim p2 = state.positions(es(i).v)

                For j As Integer = i + 1 To es.Length - 1
                    ' 共享端点的边视作相邻、忽略
                    If es(i).u = es(j).u OrElse es(i).u = es(j).v OrElse
                       es(i).v = es(j).u OrElse es(i).v = es(j).v Then
                        Continue For
                    End If

                    Dim p3 = state.positions(es(j).u)
                    Dim p4 = state.positions(es(j).v)

                    If SegmentsIntersect(p1, p2, p3, p4) Then
                        result.Add((es(i).u, es(i).v, es(j).u, es(j).v))
                    End If
                Next
            Next

            Return result.ToArray
        End Function

        ''' <summary>
        ''' 标准线段相交判定（跨立实验）。
        ''' </summary>
        Private Function SegmentsIntersect(p1 As FDGVector2, p2 As FDGVector2, p3 As FDGVector2, p4 As FDGVector2) As Boolean
            Dim d1 = Cross(p3, p4, p1)
            Dim d2 = Cross(p3, p4, p2)
            Dim d3 = Cross(p1, p2, p3)
            Dim d4 = Cross(p1, p2, p4)

            If ((d1 > 0 AndAlso d2 < 0) OrElse (d1 < 0 AndAlso d2 > 0)) AndAlso
               ((d3 > 0 AndAlso d4 < 0) OrElse (d3 < 0 AndAlso d4 > 0)) Then
                Return True
            End If

            ' 退化情形（共线触及）按不相交处理，避免抖动
            Return False
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function Cross(a As FDGVector2, b As FDGVector2, c As FDGVector2) As Double
            Return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)
        End Function
    End Module
End Namespace
