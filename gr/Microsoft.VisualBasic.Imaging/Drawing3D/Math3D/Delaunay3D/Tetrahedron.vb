#Region "Microsoft.VisualBasic::3ff25a4790cd3c71767dca1c68c3d72d, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Math3D\Delaunay3D\Tetrahedron.vb"

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

    '   Total Lines: 204
    '    Code Lines: 137 (67.16%)
    ' Comment Lines: 33 (16.18%)
    '    - Xml Docs: 87.88%
    ' 
    '   Blank Lines: 34 (16.67%)
    '     File Size: 8.61 KB


    '     Structure Tetrahedron
    ' 
    '         Properties: RadiusRatio, SignedVolume6, Volume
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Contains, Det3, Det4, InSphere, InSphereDet
    '                   ToString, TriangleArea, TryCircumsphere
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace Drawing3D.Math3D.Delaunay3D

    ''' <summary>
    ''' 四面体（由四个三维坐标点构成），提供体积、外接球、InSphere 空球判定与质量度量。
    ''' 坐标使用 <see cref="Drawing3D.Point3D"/>（Double 精度）。
    ''' </summary>
    Public Structure Tetrahedron

        Public A As Point3D
        Public B As Point3D
        Public C As Point3D
        Public D As Point3D

        Sub New(a As Point3D, b As Point3D, c As Point3D, d As Point3D)
            Me.A = a
            Me.B = b
            Me.C = c
            Me.D = d
        End Sub

        ''' <summary>
        ''' 有向体积的 6 倍（标量三重积 (B-A) · ((C-A) × (D-A))）。
        ''' 正值表示 (A, B, C, D) 为正定向（右手定则），零值表示共面退化。
        ''' </summary>
        Public ReadOnly Property SignedVolume6 As Double
            Get
                Dim bx As Double = B.X - A.X, by As Double = B.Y - A.Y, bz As Double = B.Z - A.Z
                Dim cx As Double = C.X - A.X, cy As Double = C.Y - A.Y, cz As Double = C.Z - A.Z
                Dim dx As Double = D.X - A.X, dy As Double = D.Y - A.Y, dz As Double = D.Z - A.Z
                ' (C-A) x (D-A)
                Dim nx As Double = cy * dz - cz * dy
                Dim ny As Double = cz * dx - cx * dz
                Dim nz As Double = cx * dy - cy * dx
                Return bx * nx + by * ny + bz * nz
            End Get
        End Property

        Public ReadOnly Property Volume As Double
            Get
                Return std.Abs(SignedVolume6) / 6.0
            End Get
        End Property

        ''' <summary>
        ''' 求外接球。共面（退化）四面体返回 False。
        ''' </summary>
        Public Function TryCircumsphere(ByRef center As Point3D, ByRef radius As Double) As Boolean
            Dim bx As Double = B.X - A.X, by As Double = B.Y - A.Y, bz As Double = B.Z - A.Z
            Dim cx As Double = C.X - A.X, cy As Double = C.Y - A.Y, cz As Double = C.Z - A.Z
            Dim dx As Double = D.X - A.X, dy As Double = D.Y - A.Y, dz As Double = D.Z - A.Z

            Dim m As Double(,) = {
                {2 * bx, 2 * by, 2 * bz},
                {2 * cx, 2 * cy, 2 * cz},
                {2 * dx, 2 * dy, 2 * dz}}
            Dim det As Double = Det3(m)

            If std.Abs(det) < 1.0E-14 Then
                Return False
            End If

            Dim br2 As Double = bx * bx + by * by + bz * bz
            Dim cr2 As Double = cx * cx + cy * cy + cz * cz
            Dim dr2 As Double = dx * dx + dy * dy + dz * dz
            Dim rhs As Double() = {br2, cr2, dr2}
            Dim xyz As Double() = New Double(2) {}

            ' Cramer 法则逐分量求解
            For col As Integer = 0 To 2
                Dim mm As Double(,) = CType(m.Clone, Double(,))

                For row As Integer = 0 To 2
                    mm(row, col) = rhs(row)
                Next

                xyz(col) = Det3(mm) / det
            Next

            center = New Point3D(A.X + xyz(0), A.Y + xyz(1), A.Z + xyz(2))
            radius = std.Sqrt(xyz(0) * xyz(0) + xyz(1) * xyz(1) + xyz(2) * xyz(2))
            Return True
        End Function

        ''' <summary>
        ''' 质量度量：外接球半径 / (3 × 内切球半径)。正四面体约为 1，越大越畸变；
        ''' 退化四面体返回 +∞。
        ''' </summary>
        Public ReadOnly Property RadiusRatio As Double
            Get
                Dim vol As Double = Volume

                If vol < 1.0E-20 Then
                    Return Double.PositiveInfinity
                End If

                ' 表面积
                Dim area As Double =
                    TriangleArea(A, B, C) + TriangleArea(A, B, D) +
                    TriangleArea(A, C, D) + TriangleArea(B, C, D)
                Dim inRadius As Double = 3.0 * vol / area
                Dim center As Point3D = Nothing
                Dim circumRadius As Double = 0

                If Not TryCircumsphere(center, circumRadius) Then
                    Return Double.PositiveInfinity
                End If

                Return circumRadius / (3.0 * inRadius)
            End Get
        End Property

        ''' <summary>
        ''' InSphere 判定的原始行列式（4×4）：
        ''' 假设 (A, B, C, D) 正定向，则结果大于 0 表示点 <paramref name="p"/> 在四面体外接球内部。
        ''' 行列式同时除以 6 倍有向体积以消除定向差异（即返回值恒有"正=在球内"的语义）。
        ''' </summary>
        Public Shared Function InSphere(a As Point3D, b As Point3D, c As Point3D, d As Point3D, p As Point3D) As Double
            Dim det As Double = InSphereDet(a, b, c, d, p)
            Dim orient As Double = New Tetrahedron(a, b, c, d).SignedVolume6

            If std.Abs(orient) < Double.Epsilon Then
                Return 0
            End If

            ' 取负号归一化：使"返回值 > 0 = 点在外接球内"与四面体定向无关
            Return -det / orient
        End Function

        ''' <summary>
        ''' InSphere 行列式（未做定向归一化）：
        ''' 正定向四面体返回正值表示点在外接球内。
        ''' </summary>
        Public Shared Function InSphereDet(a As Point3D, b As Point3D, c As Point3D, d As Point3D, p As Point3D) As Double
            Dim ax As Double = a.X - p.X, ay As Double = a.Y - p.Y, az As Double = a.Z - p.Z
            Dim bx As Double = b.X - p.X, by As Double = b.Y - p.Y, bz As Double = b.Z - p.Z
            Dim cx As Double = c.X - p.X, cy As Double = c.Y - p.Y, cz As Double = c.Z - p.Z
            Dim dx As Double = d.X - p.X, dy As Double = d.Y - p.Y, dz As Double = d.Z - p.Z
            Dim m As Double(,) = {
                {ax, ay, az, ax * ax + ay * ay + az * az},
                {bx, by, bz, bx * bx + by * by + bz * bz},
                {cx, cy, cz, cx * cx + cy * cy + cz * cz},
                {dx, dy, dz, dx * dx + dy * dy + dz * dz}}
            Return Det4(m)
        End Function

        ''' <summary>
        ''' 带容差与定向归一化的空球判定：点 <paramref name="p"/> 是否位于四面体外接球内。
        ''' </summary>
        ''' <param name="eps">行列式相对容差（相对于体积归一化后的量纲）。</param>
        Public Shared Function Contains(a As Point3D, b As Point3D, c As Point3D, d As Point3D, p As Point3D, eps As Double) As Boolean
            Return InSphere(a, b, c, d, p) > eps
        End Function

        ''' <summary>三角形面积。</summary>
        Public Shared Function TriangleArea(a As Point3D, b As Point3D, c As Point3D) As Double
            Dim ux As Double = b.X - a.X, uy As Double = b.Y - a.Y, uz As Double = b.Z - a.Z
            Dim vx As Double = c.X - a.X, vy As Double = c.Y - a.Y, vz As Double = c.Z - a.Z
            Dim nx As Double = uy * vz - uz * vy
            Dim ny As Double = uz * vx - ux * vz
            Dim nz As Double = ux * vy - uy * vx
            Return 0.5 * std.Sqrt(nx * nx + ny * ny + nz * nz)
        End Function

        Private Shared Function Det3(m As Double(,)) As Double
            Return m(0, 0) * (m(1, 1) * m(2, 2) - m(1, 2) * m(2, 1)) -
                   m(0, 1) * (m(1, 0) * m(2, 2) - m(1, 2) * m(2, 0)) +
                   m(0, 2) * (m(1, 0) * m(2, 1) - m(1, 1) * m(2, 0))
        End Function

        Private Shared Function Det4(m As Double(,)) As Double
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

        Public Overrides Function ToString() As String
            Return $"[Tetrahedron vol={Volume.ToString("G4")}]"
        End Function
    End Structure
End Namespace
