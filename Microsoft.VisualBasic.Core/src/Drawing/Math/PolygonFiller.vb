Imports System.Drawing
Imports std = System.Math

Namespace Imaging.Math2D

    Friend Class PolygonFiller

        ''' <summary>
        ''' 填充多边形并返回内部所有点的集合
        ''' </summary>
        ''' <param name="vertices">多边形顶点（按顺时针或逆时针顺序）</param>
        ''' <returns>填充点的集合（List(Of Point)）</returns>
        Public Shared Function FillPolygon(vertices As List(Of Point)) As List(Of Point)
            If vertices Is Nothing OrElse vertices.Count < 3 Then Return New List(Of Point)()

            ' 1. 初始化边表（ET）和活性边表（AET）
            Dim edgeTable As New SortedDictionary(Of Integer, List(Of Edge))()
            Dim activeEdges As New List(Of Edge)()
            Dim fillPoints As New List(Of Point)()

            ' 2. 计算多边形的Y范围
            Dim yMin As Integer = vertices.Min(Function(v) v.Y)
            Dim yMax As Integer = vertices.Max(Function(v) v.Y)

            ' 3. 构建边表（ET）
            For i As Integer = 0 To vertices.Count - 1
                Dim p1 As Point = vertices(i)
                Dim p2 As Point = vertices((i + 1) Mod vertices.Count)

                ' 跳过水平边
                If p1.Y = p2.Y Then Continue For

                ' 确保p1是边的下端顶点
                Dim edge As New Edge()
                If p1.Y > p2.Y Then
                    edge.YMax = p1.Y
                    edge.X = p2.X
                    edge.DeltaX = CSng(p1.X - p2.X) / (p1.Y - p2.Y)
                    edge.YStart = p2.Y
                Else
                    edge.YMax = p2.Y
                    edge.X = p1.X
                    edge.DeltaX = CSng(p2.X - p1.X) / (p2.Y - p1.Y)
                    edge.YStart = p1.Y
                End If

                ' 添加至边表（按YStart分组）
                If Not edgeTable.ContainsKey(edge.YStart) Then
                    edgeTable(edge.YStart) = New List(Of Edge)()
                End If
                edgeTable(edge.YStart).Add(edge)
            Next

            ' 4. 扫描线算法（从yMin到yMax逐行处理）
            For y As Integer = yMin To yMax
                ' 4.1 将新边加入活性边表（AET）
                If edgeTable.ContainsKey(y) Then
                    activeEdges.AddRange(edgeTable(y))
                End If

                ' 4.2 移除不再相交的边（YMax <= 当前Y）
                activeEdges.RemoveAll(Function(e) e.YMax <= y)

                ' 4.3 按当前X坐标排序活性边表
                activeEdges.Sort(Function(e1, e2) e1.X.CompareTo(e2.X))

                ' 4.4 填充扫描线（两两配对交点）
                For i As Integer = 0 To activeEdges.Count - 1 Step 2
                    If i + 1 >= activeEdges.Count Then Exit For

                    Dim xStart As Integer = CInt(std.Ceiling(activeEdges(i).X))
                    Dim xEnd As Integer = CInt(std.Floor(activeEdges(i + 1).X))

                    ' 添加当前扫描线的填充点
                    For x As Integer = xStart To xEnd
                        fillPoints.Add(New Point(x, y))
                    Next
                Next

                ' 4.5 更新活性边表中边的X坐标（增量计算）
                For Each edge In activeEdges
                    edge.X += edge.DeltaX
                Next
            Next

            Return fillPoints
        End Function

        ' 边数据结构（存储扫描线所需信息）
        Private Class Edge
            Public Property YMax As Integer     ' 边的最高Y坐标
            Public Property X As Single         ' 当前扫描线与边的交点X坐标
            Public Property DeltaX As Single     ' X坐标增量（1/斜率）
            Public Property YStart As Integer    ' 边的最低Y坐标（起始扫描线）
        End Class
    End Class
End Namespace