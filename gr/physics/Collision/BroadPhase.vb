' Copyright (c) 2018 GPL3 Licensed
' Broad Phase：均匀网格空间划分，粗略剔除不可能碰撞的物体对。

Imports System
Imports System.Collections.Generic
Imports System.Math
Imports std = System.Math
Imports Microsoft.VisualBasic.Imaging.Physics.RigidBody

Namespace Microsoft.VisualBasic.Imaging.Physics.Collision

    ''' <summary>
    ''' Broad Phase 宽相位剔除。将所有刚体的 AABB 写入均匀网格，
    ''' 只返回网格相邻单元内可能相交的物体对，将碰撞检测从 O(n²) 降到约 O(n)。
    ''' </summary>
    Public Module BroadPhase

        ''' <summary>一对可能发生碰撞的刚体</summary>
        Public Structure BodyPair
            Public A As RigidBody
            Public B As RigidBody
        End Structure

        ''' <summary>计算所有潜在的碰撞对（AABB 重叠）</summary>
        Public Function ComputePairs(bodies As List(Of RigidBody)) As List(Of BodyPair)
            Dim n = bodies.Count
            If n < 2 Then Return New List(Of BodyPair)

            Dim boxes(n - 1) As AABB
            Dim maxR = 0.0

            For i = 0 To n - 1
                boxes(i) = bodies(i).GetAABB()
                Dim e = boxes(i).Extents
                maxR = std.Max(maxR, std.Max(e.x, e.y))
            Next

            Dim cell = If(maxR > 0, maxR * 2.0, 64.0)
            Dim grid As New Dictionary(Of Long, List(Of Integer))
            Dim seen As New HashSet(Of Long)
            Dim pairs As New List(Of BodyPair)

            For i = 0 To n - 1
                Dim b = boxes(i)
                Dim minCx = std.Floor(b.min.x / cell)
                Dim minCy = std.Floor(b.min.y / cell)
                Dim maxCx = std.Floor(b.max.x / cell)
                Dim maxCy = std.Floor(b.max.y / cell)

                For cx = minCx To maxCx
                    For cy = minCy To maxCy
                        Dim key = HashCell(cx, cy)
                        If Not grid.ContainsKey(key) Then grid(key) = New List(Of Integer)
                        grid(key).Add(i)
                    Next
                Next
            Next

            For Each kv In grid
                Dim list = kv.Value
                For ai = 0 To list.Count - 1
                    For bi = ai + 1 To list.Count - 1
                        Dim i = list(ai), j = list(bi)
                        If i = j Then Continue For

                        Dim lo = std.Min(i, j), hi = std.Max(i, j)
                        Dim pid = CLng(lo) * n + hi
                        If seen.Contains(pid) Then Continue For
                        seen.Add(pid)

                        If boxes(lo).Overlaps(boxes(hi)) Then
                            pairs.Add(New BodyPair With {.A = bodies(lo), .B = bodies(hi)})
                        End If
                    Next
                Next
            Next

            Return pairs
        End Function

        Private Function HashCell(cx As Integer, cy As Integer) As Long
            Return (CLng(cx) * 15823L) Xor (CLng(cy) * 9737333L)
        End Function
    End Module
End Namespace
