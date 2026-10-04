Option Strict On
Option Explicit On

Imports std = System.Math

Namespace Stl

    ''' <summary>
    ''' Accumulates triangles for binary STL export. Every Add* method expects
    ''' counter-clockwise winding seen from outside (right-hand rule); the face
    ''' normal is computed automatically and stored with each triangle.
    ''' </summary>
    Public NotInheritable Class MeshBuilder

        ''' <summary>Floats stored per triangle: 3 normal + 3×3 coordinates.</summary>
        Public Const FloatsPerTriangle As Integer = 12

        Private ReadOnly _flat As New List(Of Single)

        ''' <summary>Number of triangles accumulated so far.</summary>
        Public ReadOnly Property TriangleCount As Integer
            Get
                Return _flat.Count \ FloatsPerTriangle
            End Get
        End Property

        ''' <summary>Reads triangle <paramref name="index"/> as 12 Singles (normal, v1, v2, v3).</summary>
        Public Function GetTriangle(index As Integer) As Single()
            If index < 0 OrElse index >= TriangleCount Then
                Throw New ArgumentOutOfRangeException(NameOf(index))
            End If
            Dim t(FloatsPerTriangle - 1) As Single
            For k = 0 To FloatsPerTriangle - 1
                t(k) = _flat(index * FloatsPerTriangle + k)
            Next
            Return t
        End Function

        ''' <summary>Adds a raw triangle; the normal follows the right-hand rule.</summary>
        Public Sub AddTriangle(ax As Double, ay As Double, az As Double,
                           bx As Double, by As Double, bz As Double,
                           cx As Double, cy As Double, cz As Double)
            Dim ux = bx - ax, uy = by - ay, uz = bz - az
            Dim vx = cx - ax, vy = cy - ay, vz = cz - az
            Dim nx = uy * vz - uz * vy
            Dim ny = uz * vx - ux * vz
            Dim nz = ux * vy - uy * vx

            Dim len = std.Sqrt(nx * nx + ny * ny + nz * nz)
            If len > 0.000000000000001 Then
                nx /= len
                ny /= len
                nz /= len
            Else
                nx = 0.0 : ny = 0.0 : nz = 0.0    ' degenerate: STL allows a zero normal
            End If

            _flat.Add(CSng(nx)) : _flat.Add(CSng(ny)) : _flat.Add(CSng(nz))
            _flat.Add(CSng(ax)) : _flat.Add(CSng(ay)) : _flat.Add(CSng(az))
            _flat.Add(CSng(bx)) : _flat.Add(CSng(by)) : _flat.Add(CSng(bz))
            _flat.Add(CSng(cx)) : _flat.Add(CSng(cy)) : _flat.Add(CSng(cz))
        End Sub

        ''' <summary>Adds a quad as two triangles (a,b,c) + (a,c,d).</summary>
        Public Sub AddQuad(ax As Double, ay As Double, az As Double,
                       bx As Double, by As Double, bz As Double,
                       cx As Double, cy As Double, cz As Double,
                       dx As Double, dy As Double, dz As Double)
            AddTriangle(ax, ay, az, bx, by, bz, cx, cy, cz)
            AddTriangle(ax, ay, az, cx, cy, cz, dx, dy, dz)
        End Sub

        ''' <summary>
        ''' Axis-aligned box, x in [x0,x1], y in [y0,y1], z in [z0,z1]. All six
        ''' faces are emitted with outward normals (12 triangles).
        ''' </summary>
        Public Sub AddBox(x0 As Double, y0 As Double, x1 As Double, y1 As Double,
                      z0 As Double, z1 As Double)
            If x1 < x0 Then Dim t = x0 : x0 = x1 : x1 = t
            If y1 < y0 Then Dim t = y0 : y0 = y1 : y1 = t
            If z1 < z0 Then Dim t = z0 : z0 = z1 : z1 = t

            ' top (+z)
            AddQuad(x0, y0, z1, x1, y0, z1, x1, y1, z1, x0, y1, z1)
            ' bottom (-z)
            AddQuad(x0, y0, z0, x0, y1, z0, x1, y1, z0, x1, y0, z0)
            ' south (-y)
            AddQuad(x0, y0, z0, x1, y0, z0, x1, y0, z1, x0, y0, z1)
            ' north (+y)
            AddQuad(x0, y1, z0, x0, y1, z1, x1, y1, z1, x1, y1, z0)
            ' west (-x)
            AddQuad(x0, y0, z0, x0, y0, z1, x0, y1, z1, x0, y1, z0)
            ' east (+x)
            AddQuad(x1, y0, z0, x1, y1, z0, x1, y1, z1, x1, y0, z1)
        End Sub

        ''' <summary>
        ''' Vertical prism (a "cylinder" with the given side count) around
        ''' (cx, cy) from z0 to z1 — used for fountains and tree trunks.
        ''' </summary>
        Public Sub AddPrism(cx As Double, cy As Double,
                        z0 As Double, z1 As Double,
                        radius As Double, sides As Integer)
            If sides < 3 Then sides = 3

            Dim rimX(sides - 1) As Double
            Dim rimY(sides - 1) As Double
            For k = 0 To sides - 1
                Dim theta = 2.0 * std.PI * k / sides
                rimX(k) = cx + radius * std.Cos(theta)
                rimY(k) = cy + radius * std.Sin(theta)
            Next

            ' side faces
            For k = 0 To sides - 1
                Dim k2 = (k + 1) Mod sides
                AddQuad(rimX(k), rimY(k), z0, rimX(k2), rimY(k2), z0,
                    rimX(k2), rimY(k2), z1, rimX(k), rimY(k), z1)
            Next

            ' top cap
            For k = 0 To sides - 1
                Dim k2 = (k + 1) Mod sides
                AddTriangle(cx, cy, z1, rimX(k), rimY(k), z1, rimX(k2), rimY(k2), z1)
            Next

            ' bottom cap
            For k = 0 To sides - 1
                Dim k2 = (k + 1) Mod sides
                AddTriangle(cx, cy, z0, rimX(k2), rimY(k2), z0, rimX(k), rimY(k), z0)
            Next
        End Sub

        ''' <summary>
        ''' Gable roof over the rectangle [x0,x1]×[y0,y1]; the ridge runs along X
        ''' at the Y midline, eaves at zEave and ridge at zRidge. Used for
        ''' bungalows and plaza kiosks.
        ''' </summary>
        Public Sub AddGableRoof(x0 As Double, y0 As Double, x1 As Double, y1 As Double,
                            zEave As Double, zRidge As Double)
            Dim ym = (y0 + y1) / 2.0

            ' south slope (facing -y)
            AddQuad(x0, y0, zEave, x1, y0, zEave, x1, ym, zRidge, x0, ym, zRidge)
            ' north slope (facing +y)
            AddQuad(x0, y1, zEave, x0, ym, zRidge, x1, ym, zRidge, x1, y1, zEave)
            ' west gable end (-x)
            AddTriangle(x0, y0, zEave, x0, y1, zEave, x0, ym, zRidge)
            ' east gable end (+x)
            AddTriangle(x1, y0, zEave, x1, ym, zRidge, x1, y1, zEave)
        End Sub

    End Class
End Namespace