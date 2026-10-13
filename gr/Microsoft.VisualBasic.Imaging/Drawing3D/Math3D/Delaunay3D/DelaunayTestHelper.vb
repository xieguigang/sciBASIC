#Region "Microsoft.VisualBasic::0248299c14af1ba30cf3c871de449f63, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Math3D\Delaunay3D\DelaunayTestHelper.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 262
    '    Code Lines: 189 (72.14%)
    ' Comment Lines: 25 (9.54%)
    '    - Xml Docs: 84.00%
    ' 
    '   Blank Lines: 48 (18.32%)
    '     File Size: 12.16 KB


    '     Class DelaunayQualityStats
    ' 
    '         Function: ToString
    ' 
    '     Module DelaunayTestHelper
    ' 
    '         Function: ComputeQuality, Det3, Det4, FaceNormalOutward, InSphereNorm
    '                   MinDihedralAngle, SignedVolume6, ValidateDelaunay
    ' 
    '         Sub: Normalize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace Drawing3D.Math3D.Delaunay3D

    ''' <summary>
    ''' 三维 Delaunay 网格的质量统计。
    ''' </summary>
    Public Class DelaunayQualityStats

        ''' <summary>四面体总数。</summary>
        Public Tetrahedrons As Integer
        ''' <summary>零体积（共面退化）四面体数。</summary>
        Public Degenerate As Integer
        ''' <summary>最小半径比（RadiusRatio = R/(3r)，正四面体 ≈ 1）。</summary>
        Public MinRadiusRatio As Double = Double.PositiveInfinity
        ''' <summary>最大半径比。</summary>
        Public MaxRadiusRatio As Double = 0
        ''' <summary>最小二面角（角度制）。</summary>
        Public MinDihedralAngle As Double = 180
        ''' <summary>平均半径比。</summary>
        Public AverageRadiusRatio As Double

        Public Overrides Function ToString() As String
            Return $"tets={Tetrahedrons}, degenerate={Degenerate}, " &
                   $"radiusRatio=[{MinRadiusRatio.ToString("G3")}, {MaxRadiusRatio.ToString("G3")}], " &
                   $"avg={AverageRadiusRatio.ToString("G3")}, minDihedral={MinDihedralAngle.ToString("G3")} deg"
        End Function
    End Class

    ''' <summary>
    ''' 3D Delaunay 三角剖分结果的校验与质量度量工具（自测/调试用）。
    ''' </summary>
    Public Module DelaunayTestHelper

        ''' <summary>
        ''' 暴力校验 Delaunay 空球特性：对每个四面体，检查所有非顶点输入点
        ''' 是否落在其外接球内部。返回违反空球特性的次数（0 表示通过）。
        ''' 复杂度 O(T × n)，仅适合小规模自测。
        ''' </summary>
        ''' <param name="eps">归一化 InSphere 行列式容差。</param>
        Public Function ValidateDelaunay(points As List(Of Point3D),
                                         tetra As List(Of TetraIndices),
                                         Optional eps As Double = 0.000001) As Integer

            If points.Count < 4 Then
                Return 0
            End If

            ' 内部归一化，与剖分时的坐标尺度保持一致的数值语义
            Dim wx As Double() = Nothing, wy As Double() = Nothing, wz As Double() = Nothing
            Call Normalize(points, wx, wy, wz)
            Dim violations As Integer = 0

            For Each t As TetraIndices In tetra
                Dim v As Integer() = {t.A, t.B, t.C, t.D}
                Dim orient As Double = SignedVolume6(wx, wy, wz, v)

                If std.Abs(orient) < 1.0E-18 Then
                    Continue For
                End If

                For p As Integer = 0 To points.Count - 1
                    If p = t.A OrElse p = t.B OrElse p = t.C OrElse p = t.D Then
                        Continue For
                    End If

                    Dim d As Double = -InSphereNorm(wx, wy, wz, v, wx(p), wy(p), wz(p)) / orient

                    If d > eps Then
                        violations += 1
                    End If
                Next
            Next

            Return violations
        End Function

        ''' <summary>统计四面体网格质量（半径比、退化数、最小二面角）。</summary>
        Public Function ComputeQuality(points As List(Of Point3D), tetra As List(Of TetraIndices)) As DelaunayQualityStats
            Dim stats As New DelaunayQualityStats With {.Tetrahedrons = tetra.Count}
            Dim sumRatio As Double = 0

            For Each t As TetraIndices In tetra
                Dim tet As New Tetrahedron(points(t.A), points(t.B), points(t.C), points(t.D))
                Dim ratio As Double = tet.RadiusRatio

                If Double.IsInfinity(ratio) Then
                    stats.Degenerate += 1
                    Continue For
                End If

                stats.MinRadiusRatio = std.Min(stats.MinRadiusRatio, ratio)
                stats.MaxRadiusRatio = std.Max(stats.MaxRadiusRatio, ratio)
                sumRatio += ratio
                stats.MinDihedralAngle = std.Min(stats.MinDihedralAngle, MinDihedralAngle(tet))
            Next

            Dim valid As Integer = tetra.Count - stats.Degenerate
            stats.AverageRadiusRatio = If(valid > 0, sumRatio / valid, Double.PositiveInfinity)
            Return stats
        End Function

        ''' <summary>四面体的最小二面角（角度制）。退化四面体返回 0。</summary>
        Public Function MinDihedralAngle(tet As Tetrahedron) As Double
            If std.Abs(tet.SignedVolume6) < 1.0E-20 Then
                Return 0
            End If

            Dim v As Point3D() = {tet.A, tet.B, tet.C, tet.D}
            Dim minAngle As Double = 180.0

            ' 遍历 4 个面，计算每个面与其相邻面的二面角（共 6 条棱，每条棱计算一次）
            Dim faces As Integer()() = {
                New Integer() {0, 1, 2, 3}, New Integer() {0, 1, 3, 2},
                New Integer() {0, 2, 3, 1}, New Integer() {1, 2, 3, 0}}

            For fi As Integer = 0 To faces.Length - 1
                For fj As Integer = fi + 1 To faces.Length - 1
                    ' 两个面共享 2 个顶点 -> 一条棱
                    Dim shared1 As Integer = -1, shared2 As Integer = -1
                    Dim otherF1 As Integer = -1, otherF2 As Integer = -1

                    For Each a As Integer In faces(fi)
                        If faces(fj).Contains(a) Then
                            If shared1 < 0 Then shared1 = a Else shared2 = a
                        Else
                            otherF1 = a
                        End If
                    Next

                    For Each b As Integer In faces(fj)
                        If Not faces(fi).Contains(b) Then
                            otherF2 = b
                        End If
                    Next

                    If shared1 < 0 OrElse shared2 < 0 OrElse otherF1 < 0 OrElse otherF2 < 0 Then
                        Continue For
                    End If

                    Dim n1 As Point3D = FaceNormalOutward(v, faces(fi)(0), faces(fi)(1), faces(fi)(2), faces(fi)(3))
                    Dim n2 As Point3D = FaceNormalOutward(v, faces(fj)(0), faces(fj)(1), faces(fj)(2), faces(fj)(3))
                    Dim l1 As Double = std.Sqrt(n1.X * n1.X + n1.Y * n1.Y + n1.Z * n1.Z)
                    Dim l2 As Double = std.Sqrt(n2.X * n2.X + n2.Y * n2.Y + n2.Z * n2.Z)

                    If l1 < 1.0E-30 OrElse l2 < 1.0E-30 Then
                        Return 0
                    End If

                    Dim cosD As Double = (n1.X * n2.X + n1.Y * n2.Y + n1.Z * n2.Z) / (l1 * l2)
                    cosD = std.Max(-1.0, std.Min(1.0, cosD))
                    ' 两外法向夹角 = π - 内二面角
                    Dim dihedral As Double = (180.0 - std.Acos(cosD) * 180.0 / std.PI)
                    minAngle = std.Min(minAngle, dihedral)
                Next
            Next

            Return minAngle
        End Function

        ''' <summary>面的外法向（背离第 4 个顶点 <paramref name="opposite"/>）。</summary>
        Private Function FaceNormalOutward(v As Point3D(), a As Integer, b As Integer, c As Integer, opposite As Integer) As Point3D
            Dim ux As Double = v(b).X - v(a).X, uy As Double = v(b).Y - v(a).Y, uz As Double = v(b).Z - v(a).Z
            Dim wx As Double = v(c).X - v(a).X, wy As Double = v(c).Y - v(a).Y, wz As Double = v(c).Z - v(a).Z
            Dim nx As Double = uy * wz - uz * wy
            Dim ny As Double = uz * wx - ux * wz
            Dim nz As Double = ux * wy - uy * wx

            If (v(opposite).X - v(a).X) * nx + (v(opposite).Y - v(a).Y) * ny + (v(opposite).Z - v(a).Z) * nz > 0 Then
                nx = -nx : ny = -ny : nz = -nz
            End If

            Return New Point3D(nx, ny, nz)
        End Function

        Private Sub Normalize(points As List(Of Point3D), ByRef wx As Double(), ByRef wy As Double(), ByRef wz As Double())
            Dim n As Integer = points.Count
            Dim minX As Double = points(0).X, maxX As Double = minX
            Dim minY As Double = points(0).Y, maxY As Double = minY
            Dim minZ As Double = points(0).Z, maxZ As Double = minZ

            For Each p As Point3D In points
                minX = std.Min(minX, p.X) : maxX = std.Max(maxX, p.X)
                minY = std.Min(minY, p.Y) : maxY = std.Max(maxY, p.Y)
                minZ = std.Min(minZ, p.Z) : maxZ = std.Max(maxZ, p.Z)
            Next

            Dim cx As Double = (minX + maxX) * 0.5
            Dim cy As Double = (minY + maxY) * 0.5
            Dim cz As Double = (minZ + maxZ) * 0.5
            Dim span As Double = std.Max(std.Max(maxX - minX, maxY - minY), std.Max(maxZ - minZ, 1.0E-30))
            Dim scale As Double = 1.0 / span

            ReDim wx(n - 1) : ReDim wy(n - 1) : ReDim wz(n - 1)

            For i As Integer = 0 To n - 1
                wx(i) = (points(i).X - cx) * scale
                wy(i) = (points(i).Y - cy) * scale
                wz(i) = (points(i).Z - cz) * scale
            Next
        End Sub

        Private Function SignedVolume6(wx As Double(), wy As Double(), wz As Double(), v As Integer()) As Double
            Dim ax As Double = wx(v(0)), ay As Double = wy(v(0)), az As Double = wz(v(0))
            Dim bx As Double = wx(v(1)) - ax, by As Double = wy(v(1)) - ay, bz As Double = wz(v(1)) - az
            Dim cx As Double = wx(v(2)) - ax, cy As Double = wy(v(2)) - ay, cz As Double = wz(v(2)) - az
            Dim dx As Double = wx(v(3)) - ax, dy As Double = wy(v(3)) - ay, dz As Double = wz(v(3)) - az
            Dim nx As Double = cy * dz - cz * dy
            Dim ny As Double = cz * dx - cx * dz
            Dim nz As Double = cx * dy - cy * dx
            Return bx * nx + by * ny + bz * nz
        End Function

        Private Function InSphereNorm(wx As Double(), wy As Double(), wz As Double(), v As Integer(),
                                      qx As Double, qy As Double, qz As Double) As Double
            Dim ax As Double = wx(v(0)) - qx, ay As Double = wy(v(0)) - qy, az As Double = wz(v(0)) - qz
            Dim bx As Double = wx(v(1)) - qx, by As Double = wy(v(1)) - qy, bz As Double = wz(v(1)) - qz
            Dim cx As Double = wx(v(2)) - qx, cy As Double = wy(v(2)) - qy, cz As Double = wz(v(2)) - qz
            Dim dx As Double = wx(v(3)) - qx, dy As Double = wy(v(3)) - qy, dz As Double = wz(v(3)) - qz
            Dim m As Double(,) = {
                {ax, ay, az, ax * ax + ay * ay + az * az},
                {bx, by, bz, bx * bx + by * by + bz * bz},
                {cx, cy, cz, cx * cx + cy * cy + cz * cz},
                {dx, dy, dz, dx * dx + dy * dy + dz * dz}}
            Return Det4(m)
        End Function

        Private Function Det3(m As Double(,)) As Double
            Return m(0, 0) * (m(1, 1) * m(2, 2) - m(1, 2) * m(2, 1)) -
                   m(0, 1) * (m(1, 0) * m(2, 2) - m(1, 2) * m(2, 0)) +
                   m(0, 2) * (m(1, 0) * m(2, 1) - m(1, 1) * m(2, 0))
        End Function

        Private Function Det4(m As Double(,)) As Double
            Dim sum As Double = 0
            Dim sign As Double = 1

            For j As Integer = 0 To 3
                Dim minor As Double(,) = New Double(2, 2) {}
                Dim mr As Integer = 0

                For i As Integer = 1 To 3
                    Dim mc As Integer = 0

                    For k As Integer = 0 To 3
                        If k <> j Then
                            minor(mr, mc) = m(i, k)
                            mc += 1
                        End If
                    Next

                    mr += 1
                Next

                sum += sign * m(0, j) * Det3(minor)
                sign = -sign
            Next

            Return sum
        End Function
    End Module
End Namespace

