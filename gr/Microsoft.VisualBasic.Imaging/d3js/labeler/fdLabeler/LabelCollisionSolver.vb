Imports System.Drawing
Imports std = System.Math

Namespace d3js.Layout

    ''' <summary>
    ''' 用于解决文本标签重叠问题的力导向迭代算法模块
    ''' </summary>
    Public Module LabelCollisionSolver

        ''' <summary>
        ''' 解决标签重叠问题的主方法
        ''' </summary>
        ''' <param name="labels">需要进行布局的标签数组</param>
        ''' <param name="maxIterations">最大迭代次数，默认300</param>
        ''' <param name="padding">标签之间的额外间距(像素)，默认2.0</param>
        ''' <param name="springStrength">弹簧力度(将标签拉回原点的力度)，默认0.1，值越大标签越靠近原点但可能重叠</param>
        Public Sub ResolveOverlaps(labels As Label(),
                               Optional maxIterations As Integer = 300,
                               Optional padding As Single = 2.0F,
                               Optional springStrength As Single = 0.1F)

            If labels Is Nothing OrElse labels.Length <= 1 Then Return

            ' 记录每个标签的初始位置（即数据点位置），作为弹簧力的锚点
            Dim anchors As PointF() = New PointF(labels.Length - 1) {}
            For i As Integer = 0 To labels.Length - 1
                anchors(i) = labels(i).location
            Next

            ' 偏移量累加器，用于在一次迭代中合并所有排斥力
            Dim shifts As PointF() = New PointF(labels.Length - 1) {}

            Dim convergenceThreshold As Single = 0.5F ' 收敛阈值，当最大移动量小于此值时停止迭代

            For iter As Integer = 1 To maxIterations
                ' 重置当前迭代的偏移量
                Array.Clear(shifts, 0, shifts.Length)

                Dim maxShift As Single = 0.0F

                ' 1. 计算标签两两之间的排斥力
                For i As Integer = 0 To labels.Length - 1
                    For j As Integer = i + 1 To labels.Length - 1
                        Dim a = labels(i)
                        Dim b = labels(j)

                        ' 计算包含了padding的碰撞矩形
                        Dim rectA As New RectangleF(a.X, a.Y, a.width + padding, a.height + padding)
                        Dim rectB As New RectangleF(b.X, b.Y, b.width + padding, b.height + padding)

                        ' 检测是否相交（重叠）
                        If rectA.IntersectsWith(rectB) Then
                            ' 计算中心点距离和重叠量
                            Dim cx1 As Single = a.X + a.width / 2.0F
                            Dim cy1 As Single = a.Y + a.height / 2.0F
                            Dim cx2 As Single = b.X + b.width / 2.0F
                            Dim cy2 As Single = b.Y + b.height / 2.0F

                            Dim dx As Single = cx1 - cx2
                            Dim dy As Single = cy1 - cy2

                            ' 如果两个标签中心完全重合（dx=0, dy=0），人为赋予一个随机方向推开
                            If dx = 0.0F AndAlso dy = 0.0F Then
                                Dim rand As New Random()
                                dx = CSng(rand.NextDouble() - 0.5) * 2.0F
                                dy = CSng(rand.NextDouble() - 0.5) * 2.0F
                            End If

                            ' 计算X轴和Y轴的重叠深度
                            Dim overlapX As Single = (a.width / 2.0F + b.width / 2.0F + padding) - std.Abs(dx)
                            Dim overlapY As Single = (a.height / 2.0F + b.height / 2.0F + padding) - std.Abs(dy)

                            Dim pushX As Single = 0.0F
                            Dim pushY As Single = 0.0F

                            ' 选择重叠深度最小的轴进行推开（避免不必要的长距离滑动）
                            If overlapX > 0 AndAlso overlapY > 0 Then
                                If overlapX < overlapY Then
                                    pushX = If(dx > 0, overlapX, -overlapX)
                                Else
                                    pushY = If(dy > 0, overlapY, -overlapY)
                                End If
                            End If

                            ' 根据pinned状态分配推力
                            Dim moveA As Single = If(a.pinned, 0.0F, 1.0F)
                            Dim moveB As Single = If(b.pinned, 0.0F, 1.0F)
                            Dim totalMove As Single = moveA + moveB

                            If totalMove > 0 Then
                                Dim ratioA As Single = moveA / totalMove
                                Dim ratioB As Single = moveB / totalMove

                                ' 力减半，因为是双向推开（类似弹簧阻尼）
                                shifts(i).X += pushX * ratioA * 0.5F
                                shifts(i).Y += pushY * ratioA * 0.5F
                                shifts(j).X -= pushX * ratioB * 0.5F
                                shifts(j).Y -= pushY * ratioB * 0.5F
                            End If
                        End If
                    Next
                Next

                ' 2. 应用排斥力并添加弹簧力（拉回原点）
                For i As Integer = 0 To labels.Length - 1
                    If labels(i).pinned Then Continue For

                    ' 弹簧力：偏离锚点越远，拉回的力越大
                    Dim springX As Single = (anchors(i).X - labels(i).X) * springStrength
                    Dim springY As Single = (anchors(i).Y - labels(i).Y) * springStrength

                    ' 最终位移 = 排斥力 + 弹簧力
                    Dim finalX As Single = shifts(i).X + springX
                    Dim finalY As Single = shifts(i).Y + springY

                    ' 更新位置 (利用你类中的 Property 会自动重建 RectangleF)
                    labels(i).X += finalX
                    labels(i).Y += finalY

                    ' 记录本次迭代的最大位移，用于判断是否收敛
                    Dim currentShift As Single = std.Sqrt(finalX * finalX + finalY * finalY)
                    If currentShift > maxShift Then maxShift = currentShift
                Next

                ' 如果所有标签的位移都极小，说明已达到平衡，提前退出
                If maxShift < convergenceThreshold Then Exit For
            Next
        End Sub

    End Module

End Namespace