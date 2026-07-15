' /********************************************************************************/
'
'     WaterRenderer - bridges the physics FluidEngine3D (which works in its own
'     Vector3 world space) into the Drawing3D Camera for GDI+ rendering:
'
'       * centres the world around the box centroid so the camera rotates
'         around the model centre,
'       * draws the 12 edges of the cuboid container as a wire frame,
'       * projects every water particle and paints it as a depth + velocity
'         shaded blue dot.
'
' /********************************************************************************/

Imports System.Drawing
Imports System.Threading.Tasks
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Physics
Imports P3 = Microsoft.VisualBasic.Imaging.Physics.Vector3

Namespace FluidSim3D

    ''' <summary>
    ''' renders the 3D SPH water simulation onto a GDI+ canvas through the
    ''' Drawing3D camera (rotate + perspective project pipeline).
    ''' </summary>
    Public Class WaterRenderer

        Public Property Camera As New Camera()
        ''' <summary>background gradient top colour.</summary>
        Public Property BackgroundTop As Color = Color.FromArgb(255, 16, 23, 38)
        ''' <summary>background gradient bottom colour.</summary>
        Public Property BackgroundBottom As Color = Color.FromArgb(255, 8, 12, 20)
        ''' <summary>the wire frame colour of the container box.</summary>
        Public Property BoundaryColor As Color = Color.FromArgb(180, 63, 208, 201)
        ''' <summary>the fastest speed (world units / s) mapped to the deepest blue.</summary>
        Public Property VelocityScale As Single = 80.0F

        ' the 12 edges of a cuboid, expressed as index pairs into the 8 corners
        Shared ReadOnly boxEdges As (Integer, Integer)() = {
            (0, 1), (1, 2), (2, 3), (3, 0),
            (4, 5), (5, 6), (6, 7), (7, 4),
            (0, 4), (1, 5), (2, 6), (3, 7)
        }

        ' cached blue colour ramp (built once)
        Private cachedRamp As SolidBrush() = Nothing

        Public Sub New()
            Camera.AngleX = 20
            Camera.AngleY = -30
            Camera.AngleZ = 0
            Camera.FieldOfView = 256
            Camera.Offset = New PointF(0, 0)
        End Sub

        ''' <summary>
        ''' compute a good initial view distance so the whole box fits the canvas.
        ''' </summary>
        Public Sub FitView(box As P3, canvas As Size)
            Dim maxDim = System.Math.Max(box.x, System.Math.Max(box.y, box.z))
            Dim minDim = System.Math.Min(canvas.Width, canvas.Height)
            If minDim <= 0 Then minDim = 600
            Const perspectiveK As Double = 6
            Camera.FieldOfView = CSng(256 * perspectiveK)
            Dim vd As Double = maxDim * Camera.FieldOfView / (0.42 * minDim)
            If vd < 1 Then vd = 1
            Camera.ViewDistance = CSng(vd)
        End Sub

        ''' <summary>
        ''' draw the container wire frame and the water particle cloud.
        ''' </summary>
        Public Sub Draw(g As Graphics, canvas As Size, engine As FluidEngine3D, params As FluidParameters, showBoundary As Boolean)
            Camera.Screen = canvas
            DrawBackground(g, canvas)

            If engine Is Nothing Then
                Return
            End If

            Dim center = engine.BoxSize * 0.5

            If showBoundary Then
                DrawBoundary(g, engine.BoxSize, center)
            End If

            DrawWater(g, engine, center, params)
        End Sub

        Private Sub DrawBackground(g As Graphics, canvas As Size)
            Using bg As New SolidBrush(BackgroundBottom)
                g.FillRectangle(bg, 0, 0, canvas.Width, canvas.Height)
            End Using
            ' a subtle vertical gradient for depth
            Using grad As New Drawing2D.LinearGradientBrush(
                New Point(0, 0), New Point(0, canvas.Height),
                BackgroundTop, BackgroundBottom)
                g.FillRectangle(grad, 0, 0, canvas.Width, canvas.Height)
            End Using
        End Sub

        Private Sub DrawBoundary(g As Graphics, box As P3, center As P3)
            ' 8 corners, centred around the box centroid
            Dim corners = New Point3D() {
                ToPoint(0, 0, 0, box, center),
                ToPoint(box.x, 0, 0, box, center),
                ToPoint(box.x, box.y, 0, box, center),
                ToPoint(0, box.y, 0, box, center),
                ToPoint(0, 0, box.z, box, center),
                ToPoint(box.x, 0, box.z, box, center),
                ToPoint(box.x, box.y, box.z, box, center),
                ToPoint(0, box.y, box.z, box, center)
            }

            ' rotate (SIMD batch) + project
            Dim rotated = Camera.Rotate(corners).ToArray()
            Dim proj(rotated.Length - 1) As PointF
            For i = 0 To rotated.Length - 1
                proj(i) = ProjectToScreen(rotated(i))
            Next

            Using pen As New Pen(BoundaryColor, 1.5F)
                For Each e In boxEdges
                    g.DrawLine(pen, proj(e.Item1), proj(e.Item2))
                Next
            End Using
        End Sub

        Private Sub DrawWater(g As Graphics, engine As FluidEngine3D, center As P3, params As FluidParameters)
            Dim entities = engine.Entity
            Dim n = entities.Count

            If n = 0 Then Return

            ' build centred world points
            Dim pts(n - 1) As Point3D
            Dim speeds(n - 1) As Single
            Dim i = 0
            For Each p In entities
                pts(i) = New Point3D(p.Position.x - center.x, p.Position.y - center.y, p.Position.z - center.z)
                speeds(i) = CSng(p.Velocity.Magnitude)
                i += 1
            Next

            ' SIMD batch rotation
            Dim rotated = Camera.Rotate(pts).ToArray()

            Dim vd = Camera.ViewDistance
            Dim fov = Camera.FieldOfView
            Dim w2 = CSng(Camera.Screen.Width) / 2.0F
            Dim h2 = CSng(Camera.Screen.Height) / 2.0F
            Dim ox = Camera.Offset.X
            Dim oy = Camera.Offset.Y

            Dim xy(n - 1) As PointF
            Dim depth(n - 1) As Single
            Dim factor(n - 1) As Single

            ' parallel projection that matches Camera.Project exactly, plus the
            ' per point perspective factor used to scale the dot size.
            Parallel.For(0, n, Sub(k)
                Dim pr = rotated(k)
                Dim d = vd + pr.Z
                Dim f As Single = If(d <= 0, 0, fov / d)
                factor(k) = f
                depth(k) = pr.Z
                xy(k) = New PointF(pr.X * f + w2 + ox, pr.Y * f + h2 + oy)
            End Sub)

            ' painter order: far (large Z) first, near (small Z) last
            Dim order(n - 1) As Integer
            For k = 0 To n - 1 : order(k) = k : Next
            System.Array.Sort(order, Function(a, b) depth(b).CompareTo(depth(a)))

            ' pre compute a small blue gradient colour ramp (light -> deep)
            If cachedRamp Is Nothing Then cachedRamp = BuildRamp()
            Dim ramp = cachedRamp

            Dim baseSize = System.Math.Max(2.0F, params.ParticleSize)
            Dim vscale = System.Math.Max(1.0F, VelocityScale)
            Dim maxSize = baseSize * 3.0F

            For Each k In order
                ' skip points that ended up behind the camera plane
                If depth(k) <= -vd Then Continue For

                Dim speed = speeds(k)
                Dim t = speed / vscale
                If t < 0 Then t = 0 Else If t > 1 Then t = 1
                Dim cidx = CInt(t * (ramp.Length - 1))
                Dim brush = ramp(cidx)

                ' perspective dot size: nearer particles (smaller depth) are larger
                Dim sz = baseSize * (vd / depth(k))
                If sz < 1.5F Then sz = 1.5F
                If sz > maxSize Then sz = maxSize
                Dim half = sz / 2.0F

                g.FillEllipse(brush, xy(k).X - half, xy(k).Y - half, sz, sz)
            Next
        End Sub

        ''' <summary>
        ''' a smooth ramp from light cyan to deep ocean blue used to shade the
        ''' water particles by their speed.
        ''' </summary>
        Private Function BuildRamp() As SolidBrush()
            Dim stops = New Color() {
                Color.FromArgb(255, 127, 227, 255),
                Color.FromArgb(255, 46, 155, 255),
                Color.FromArgb(255, 30, 111, 208),
                Color.FromArgb(255, 14, 58, 102)
            }
            Dim out(63) As SolidBrush
            For i = 0 To 63
                Dim t = i / 63.0
                Dim seg = t * (stops.Length - 1)
                Dim idx = CInt(System.Math.Floor(seg))
                Dim f = seg - idx
                If idx >= stops.Length - 1 Then idx = stops.Length - 2 : f = 1
                Dim a = stops(idx), b = stops(idx + 1)
                Dim r = CInt(a.R + (b.R - a.R) * f)
                Dim gg = CInt(a.G + (b.G - a.G) * f)
                Dim bl = CInt(a.B + (b.B - a.B) * f)
                out(i) = New SolidBrush(Color.FromArgb(255, r, gg, bl))
            Next
            Return out
        End Function

        <Runtime.CompilerServices.MethodImpl(Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)>
        Private Function ToPoint(x As Double, y As Double, z As Double, box As P3, center As P3) As Point3D
            Return New Point3D(x - center.x, y - center.y, z - center.z)
        End Function

        <Runtime.CompilerServices.MethodImpl(Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)>
        Private Function ProjectToScreen(p As Point3D) As PointF
            Dim depth = Camera.ViewDistance + p.Z
            Dim f As Single = If(depth <= 0, 0, Camera.FieldOfView / depth)
            Return New PointF(p.X * f + Camera.Screen.Width / 2 + Camera.Offset.X,
                              p.Y * f + Camera.Screen.Height / 2 + Camera.Offset.Y)
        End Function

    End Class

End Namespace
