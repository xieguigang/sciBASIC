#Region "Microsoft.VisualBasic::550568968bd4d1349ca4f1e8bfe33942, ..\visualbasic_App\UXFramework\DataVisualization.Enterprise\Microsoft.VisualBasic.DataVisualization.Enterprise\HeatMap3D\AltitudeMap.xaml.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Media.Media3D
Imports System.Windows.Navigation
Imports System.Windows.Shapes

Namespace Gradiant3D

    Public Class GraphControl
        Inherits UserControl

#Region "Member Variables And Declaration"

        Dim writablebitmap As WriteableBitmap
        Dim MainModel3Dgroup As New Model3DGroup()
        Dim TheCamera As PerspectiveCamera
        Dim CameraPhi As Double = Math.PI / 6.0
        ' 30 degrees
        Dim CameraTheta As Double = Math.PI / 2.0
        Dim CameraR As Double = 70
        Const CameraDPhi As Double = 0.1
        Const CameraDTheta As Double = 0.1
        Const CameraDR As Double = 1
        Dim xmin As Integer = 0, xmax As Integer = 0, dx As Integer = 0, zmin As Integer = 0, zmax As Integer = 0, dz As Integer = 0
        Dim texture_xscale As Double = 0, texture_zscale As Double = 0
        Dim PointDictionary As New Dictionary(Of Point3D, Integer)()

        Dim modeluilist As New List(Of ModelUIElement3D)()

        Dim m_movePoint As System.Windows.Point
        Dim m_mouseDown As Boolean = False


        Dim iRows As Integer
        Dim iCols As Integer
        Dim m_fMax As Single
        Dim m_fMin As Single
        Dim pts As PointC(,)
        Dim colorLength As Integer = 10
        Dim valuelist As Single(,)

        Dim dimensionrow As Integer
        Dim dimensioncol As Integer
        Dim yposlist As Integer(,)

#End Region

#Region "Properties"
        Public Property ScaleMax() As Double
        Public Property ScaleMin() As Double
        Public Property FixedColorScale() As Boolean
        Public Property ShowValuesInMap() As Boolean
        Public Property ShowValueLabelOnMuseEnter() As Boolean
        Public Property ShowLowerPanel() As Boolean
        Public Property Unit() As String
#End Region

        Public Sub New()
            InitializeComponent()
            Me.Unit = ""
            AddHandler Me.IsVisibleChanged, New DependencyPropertyChangedEventHandler(AddressOf MyControl_IsVisibileChanged)
        End Sub

        Private Sub MyControl_IsVisibileChanged(sender As Object, e As DependencyPropertyChangedEventArgs)
            Me.Focusable = True
            Keyboard.Focus(Me)
        End Sub

        Private Sub UserControl_KeyDown(sender As Object, e As KeyEventArgs)
            Select Case e.Key
                Case Key.Up
                    CameraPhi += CameraDPhi
                    If CameraPhi > Math.PI / 2.0 Then
                        CameraPhi = Math.PI / 2.0
                    End If
                    Exit Select
                Case Key.Down
                    CameraPhi -= CameraDPhi
                    If CameraPhi < -Math.PI / 2.0 Then
                        CameraPhi = -Math.PI / 2.0
                    End If
                    Exit Select
                Case Key.Left
                    CameraTheta += CameraDTheta
                    Exit Select
                Case Key.Right
                    CameraTheta -= CameraDTheta
                    Exit Select
                Case Key.Add, Key.OemPlus
                    CameraR -= CameraDR
                    If CameraR < CameraDR Then
                        CameraR = CameraDR
                    End If
                    Exit Select
                Case Key.Subtract, Key.OemMinus
                    CameraR += CameraDR
                    Exit Select
            End Select

            PositionCamera()
        End Sub

        Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
            Me.Focus()
            TheCamera = New PerspectiveCamera()
            TheCamera.FieldOfView = 40
            MainViewport.Camera = TheCamera
            PositionCamera()
            DefineLights()
        End Sub

        Private Sub UserControl_MouseMove(sender As Object, e As MouseEventArgs)
            If m_mouseDown Then
                Dim pt As System.Windows.Point = e.GetPosition(MainViewport)
                Dim width As Double = MainViewport.ActualWidth
                Dim height As Double = MainViewport.ActualHeight
                If m_movePoint.X < pt.X Then
                    CameraTheta += CameraDTheta / 6
                End If
                If m_movePoint.Y < pt.Y Then
                    CameraPhi += CameraDPhi / 6
                    If CameraPhi > Math.PI / 2.0 Then
                        CameraPhi = Math.PI / 2.0

                    End If
                End If

                If m_movePoint.X > pt.X Then
                    CameraTheta -= CameraDTheta / 6
                End If
                If m_movePoint.Y > pt.Y Then
                    CameraPhi -= CameraDPhi / 6
                    If CameraPhi < -Math.PI / 2.0 Then
                        CameraPhi = -Math.PI / 2.0

                    End If
                End If
                m_movePoint = pt

                PositionCamera()
            End If

        End Sub

        Private Sub UserControl_MouseDown(sender As Object, e As MouseButtonEventArgs)
            Me.Focus()

            Dim pt As System.Windows.Point = e.GetPosition(MainViewport)
            If e.ChangedButton = MouseButton.Left Then
                m_mouseDown = True
                m_movePoint = pt
            End If
        End Sub

        Private Sub UserControl_MouseUp(sender As Object, e As MouseButtonEventArgs)
            m_mouseDown = False
        End Sub

        Private Sub UserControl_MouseEnter(sender As Object, e As MouseEventArgs)
            m_mouseDown = False
        End Sub

        Private Sub UserControl_MouseWheel(sender As Object, e As MouseWheelEventArgs)
            If e.Delta < 0 Then
                If CameraR <= 10 Then
                    Return
                End If

                CameraR -= CameraDR
                If CameraR < CameraDR Then
                    CameraR = CameraDR
                End If
            Else
                CameraR += CameraDR
            End If

            PositionCamera()
        End Sub

        Private Sub DefineLights()
            Dim ambient_light As New AmbientLight(Colors.Gray)
            Dim directional_light As New DirectionalLight(Colors.Gray, New Vector3D(-1.0, -3.0, -2.0))
            MainModel3Dgroup.Children.Add(ambient_light)
            MainModel3Dgroup.Children.Add(directional_light)
        End Sub

        Private Sub CreateAltitudeMap(values As Double(,))

            Dim xwidth As Integer = values.GetUpperBound(0)
            Dim zwidth As Integer = values.GetUpperBound(1)
            Dim dx As Double = (xmax - xmin) \ xwidth
            Dim dz As Double = (zmax - zmin) \ zwidth


            Dim get_values As System.Collections.Generic.IEnumerable(Of Double) = From value As Double In values Select value
            Dim ymin As Double = get_values.Min()
            Dim ymax As Double = colorLength
            Dim bm_maker As New BitmapCreator(xwidth, zwidth)

            For ix As Integer = 0 To xwidth - 1
                For iz As Integer = 0 To zwidth - 1
                    Dim red As Byte, green As Byte, blue As Byte
                    MapRainbowColor(values(ix, iz), ymin, ymax, red, green, blue)
                    bm_maker.SetPixel(ix, iz, red, green, blue, 255)
                Next
            Next

            writablebitmap = bm_maker.MakeBitmap(96, 96)
        End Sub


        Private Sub MapRainbowColor(value As Double, minvalue As Double, maxvalue As Double, ByRef blue As Byte, ByRef green As Byte, ByRef red As Byte)

            Dim colorvalue As Integer = CInt(Math.Truncate(1023 * (value - minvalue) / (maxvalue - minvalue)))

            If colorvalue < 256 Then
                red = 255
                green = CByte(colorvalue)
                blue = 0
            ElseIf colorvalue < 512 Then

                colorvalue -= 256
                red = CByte(255 - colorvalue)
                green = 255
                blue = 0
            ElseIf colorvalue < 768 Then

                colorvalue -= 512
                red = 0
                green = 255
                blue = CByte(colorvalue)
            Else
                colorvalue -= 768
                red = 0
                green = CByte(255 - colorvalue)
                blue = 255
            End If
        End Sub

        Private Sub DefineModel(modelgroup As Model3DGroup, values As Double(,))
            Dim meshgeometry As New MeshGeometry3D()

            Dim xoffset As Single = xmax / 2.0F
            Dim yoffset As Single = zmax / 2.0F
            Dim xindex As Integer = xmin
            While xindex <= xmax - dx
                Dim zindex As Integer = zmin
                While zindex <= zmax - dz
                    Dim point00 As New Point3D(xindex - xoffset, values(xindex, zindex), zindex - yoffset)
                    Dim point10 As New Point3D(xindex - xoffset + dx, values(xindex + dx, zindex), zindex - yoffset)
                    Dim point01 As New Point3D(xindex - xoffset, values(xindex, zindex + dz), zindex - yoffset + dz)
                    Dim point11 As New Point3D(xindex - xoffset + dx, values(xindex + dx, zindex + dz), zindex - yoffset + dz)

                    AddTriangle(meshgeometry, point00, point01, point11)

                    AddTriangle(meshgeometry, point00, point11, point10)
                    zindex += dx
                End While
                xindex += dx
            End While

            Dim texturebrush As New ImageBrush()
            texturebrush.ImageSource = writablebitmap
            Dim surface_material As New DiffuseMaterial(texturebrush)
            Dim surface_model As New GeometryModel3D(meshgeometry, surface_material)
            surface_model.BackMaterial = surface_material
            modelgroup.Children.Add(surface_model)
        End Sub

        Private Sub AddTriangle(mesh As MeshGeometry3D, point1 As Point3D, point2 As Point3D, point3 As Point3D)
            Dim index1 As Integer = AddPoint(mesh.Positions, mesh.TextureCoordinates, point1)
            Dim index2 As Integer = AddPoint(mesh.Positions, mesh.TextureCoordinates, point2)
            Dim index3 As Integer = AddPoint(mesh.Positions, mesh.TextureCoordinates, point3)

            mesh.TriangleIndices.Add(index1)
            mesh.TriangleIndices.Add(index2)
            mesh.TriangleIndices.Add(index3)
        End Sub

        Private Function AddPoint(points As Point3DCollection, texture_coords As PointCollection, point As Point3D) As Integer
            If PointDictionary.ContainsKey(point) Then
                Return PointDictionary(point)
            End If

            points.Add(point)
            PointDictionary.Add(point, points.Count - 1)

            texture_coords.Add(New System.Windows.Point((point.X - xmin) * texture_xscale, (point.Z - zmin) * texture_zscale))

            Return points.Count - 1
        End Function

        ''' <summary>
        ''' Calculate the camera's position in Cartesian coordinates.
        ''' </summary>
        Private Sub PositionCamera()
            Dim y As Double = CameraR * Math.Sin(CameraPhi)
            Dim hyp As Double = CameraR * Math.Cos(CameraPhi)
            Dim x As Double = hyp * Math.Cos(CameraTheta)
            Dim z As Double = hyp * Math.Sin(CameraTheta)
            TheCamera.Position = New Point3D(x, y, z)

            ' Look toward the origin.
            TheCamera.LookDirection = New Vector3D(-x, -y, -z)

            ' Set the Up direction.
            TheCamera.UpDirection = New Vector3D(0, 1, 0)
        End Sub

        Public Sub Draw3DMap(data As Single(,))
            valuelist = data
            PointDictionary.Clear()
            MainViewport.Children.Clear()
            MainModel3Dgroup.Children.Clear()
            modeluilist.Clear()
            DefineLights()

            Dim bVal As Boolean = False
            For i As Integer = 0 To data.GetUpperBound(0) - 1
                For j As Integer = 0 To data.GetUpperBound(1) - 1
                    If data(i, j) <> 0 Then
                        bVal = True
                        Exit For
                    End If
                Next
            Next

            If bVal = False Then
                Return
            End If

            Dim val As Double(,) = CreatePoints(data)

            xmin = 0
            xmax = val.GetUpperBound(0)
            dx = 1
            zmin = 0
            zmax = val.GetUpperBound(1)
            dz = 1

            texture_xscale = (xmax - xmin)
            texture_zscale = (zmax - zmin)

            CreateAltitudeMap(val)

            dimensionrow = data.GetLength(0)
            dimensioncol = data.GetLength(1)

            DefineModel(MainModel3Dgroup, val)
            If Me.ShowLowerPanel Then
                DrawPanelSurface(MainModel3Dgroup)
            End If

            ' Add the group of models to a ModelVisual3D.
            Dim model_visual As New ModelVisual3D()
            model_visual.Content = MainModel3Dgroup

            MainViewport.Children.Add(model_visual)

            DrawFullSensorData(data, dimensionrow, dimensioncol)
        End Sub

        Private Sub DrawPanelSurface(modelgroup As Model3DGroup)
            Dim meshgeometry As New MeshGeometry3D()
            Dim X As Integer = (dimensioncol - 1) * 3
            Dim Z As Integer = (dimensionrow - 1) * 3
            X -= 1
            Z -= 1

            meshgeometry.Positions.Add(New Point3D(-X, 0, -Z))
            meshgeometry.Positions.Add(New Point3D(-X, 0, Z))
            meshgeometry.Positions.Add(New Point3D(X, 0, Z))

            meshgeometry.TextureCoordinates.Add(New System.Windows.Point(0, 0))
            meshgeometry.TextureCoordinates.Add(New System.Windows.Point(0, 1))
            meshgeometry.TextureCoordinates.Add(New System.Windows.Point(1, 1))

            meshgeometry.TriangleIndices.Add(0)
            meshgeometry.TriangleIndices.Add(1)
            meshgeometry.TriangleIndices.Add(2)

            meshgeometry.Positions.Add(New Point3D(-X, 0, -Z))
            meshgeometry.Positions.Add(New Point3D(X, 0, Z))
            meshgeometry.Positions.Add(New Point3D(X, 0, -Z))

            meshgeometry.TextureCoordinates.Add(New System.Windows.Point(0, 0))
            meshgeometry.TextureCoordinates.Add(New System.Windows.Point(1, 1))
            meshgeometry.TextureCoordinates.Add(New System.Windows.Point(1, 0))

            meshgeometry.TriangleIndices.Add(3)
            meshgeometry.TriangleIndices.Add(4)
            meshgeometry.TriangleIndices.Add(5)

            Dim colorbrush As New ImageBrush()
            colorbrush.ImageSource = writablebitmap

            Dim colormaterial As New DiffuseMaterial(colorbrush)

            Dim geometrymodel As New GeometryModel3D(meshgeometry, colormaterial)

            geometrymodel.BackMaterial = colormaterial

            modelgroup.Children.Add(geometrymodel)
        End Sub

        Private Sub DrawFullSensorData(data As Single(,), m As Integer, n As Integer)
            Dim fMax As Single = 0, fMin As Single = 0

            If Me.FixedColorScale Then
                fMax = CSng(Me.ScaleMax)
                fMin = CSng(Me.ScaleMin)
            Else
                For i As Integer = 0 To m - 1
                    For j As Integer = 0 To n - 1
                        If fMax < data(i, j) Then
                            fMax = data(i, j)
                        End If
                        If fMin > data(i, j) Then
                            fMin = data(i, j)
                        End If
                    Next
                Next
            End If

            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1

                    Dim cindex As Integer = CInt(Math.Truncate(Math.Round((colorLength * (data(i, j) - fMin) + (fMax - data(i, j))) / (fMax - fMin))))
                    If cindex < 1 Then
                        cindex = 1
                    End If
                    If cindex > colorLength Then
                        cindex = colorLength
                    End If

                    Dim i2 As Integer = i
                    Dim j2 As Integer = j
                    If i = m - 1 Then
                        i2 = i - 1
                    End If
                    If j = n - 1 Then
                        j2 = j - 1
                    End If

                    If yposlist(i2, j2) > cindex Then

                        cindex = cindex + ((yposlist(i2, j2) - cindex) \ 3)
                    End If

                    Dim model3Dgroup As New Model3DGroup()
                    Dim geometry As GeometryModel3D

                    If Me.ShowValuesInMap Then
                        geometry = DrawLayerData((data(i, j)).ToString() & Me.Unit, j, i, cindex)
                        model3Dgroup.Children.Add(geometry)
                    End If

                    geometry = DrawDataLayer((data(i, j)).ToString(), j, i, cindex)
                    model3Dgroup.Children.Add(geometry)

                    Dim modelvisual As New ModelVisual3D()

                    Dim element As New ModelUIElement3D()
                    element.Model = model3Dgroup

                    If Me.ShowValueLabelOnMuseEnter Then
                        AddHandler element.MouseEnter, AddressOf element_MouseEnter
                        'AddHandler element.MouseLeave, AddressOf element_MouseLeave
                    End If

                    modelvisual.Children.Add(element)
                    modeluilist.Add(element)
                    MainViewport.Children.Add(modelvisual)
                Next
            Next
        End Sub

        Private Function DrawDataLayer(data As String, Xindex As Integer, Zindex As Integer, Yindex As Integer) As GeometryModel3D
            Dim meshgeometry As New MeshGeometry3D()

            Dim iXIncr As Double = Xindex * 5.5
            Dim iZIncr As Double = Zindex * 5.5
            Dim X As Integer = (dimensioncol - 1) * 3
            Dim Z As Integer = (dimensionrow - 1) * 3
            X += 1
            Z += 1
            Dim wid As Integer = X - 4

            Dim xoffset As Double = 1.5, zoffset As Double = 1
            iZIncr = iZIncr + zoffset
            iXIncr = iXIncr + xoffset
            Dim iHeight As Double = Yindex + 0.5
            Dim cellWidth As Double = 0.2


            meshgeometry.Positions.Add(New Point3D(-X + iXIncr, Yindex, -Z + iZIncr + 3))
            meshgeometry.Positions.Add(New Point3D(-X + iXIncr, Yindex, -Z + iZIncr))
            meshgeometry.Positions.Add(New Point3D(-wid + iXIncr - cellWidth, Yindex, -Z + iZIncr))


            meshgeometry.TextureCoordinates.Add(New System.Windows.Point(0, 0))
            meshgeometry.TextureCoordinates.Add(New System.Windows.Point(0, 1))
            meshgeometry.TextureCoordinates.Add(New System.Windows.Point(1, 1))


            meshgeometry.TriangleIndices.Add(0)
            meshgeometry.TriangleIndices.Add(1)
            meshgeometry.TriangleIndices.Add(2)

            meshgeometry.Positions.Add(New Point3D(-X + iXIncr, Yindex, -Z + iZIncr + 3))
            meshgeometry.Positions.Add(New Point3D(-wid + iXIncr - cellWidth, Yindex, -Z + iZIncr))
            meshgeometry.Positions.Add(New Point3D(-wid + iXIncr - cellWidth, Yindex, -Z + iZIncr + 3))


            meshgeometry.TextureCoordinates.Add(New System.Windows.Point(0, 0))
            meshgeometry.TextureCoordinates.Add(New System.Windows.Point(1, 1))
            meshgeometry.TextureCoordinates.Add(New System.Windows.Point(1, 0))


            meshgeometry.TriangleIndices.Add(3)
            meshgeometry.TriangleIndices.Add(4)
            meshgeometry.TriangleIndices.Add(5)

            Dim br As System.Windows.Media.Brush = System.Windows.Media.Brushes.Transparent
            Dim colormaterial As New DiffuseMaterial(br)
            Dim geometrymodel As New GeometryModel3D(meshgeometry, colormaterial)
            geometrymodel.BackMaterial = colormaterial

            Return geometrymodel
        End Function

        Private Function DrawLayerData(data As String, xindex As Integer, zindex As Integer, yindex As Integer) As GeometryModel3D
            Dim mesh As New MeshGeometry3D()
            Dim iXIncr As Double = xindex * 5.5
            Dim iZIncr As Double = zindex * 5.5
            Dim X As Integer = (dimensioncol - 1) * 3
            Dim Z As Integer = (dimensionrow - 1) * 3
            X += 1
            Z += 1
            Dim wid As Integer = X - 5


            Dim xoffset As Double = 1.5, zoffset As Double = 2
            iZIncr = iZIncr + zoffset
            iXIncr = iXIncr + xoffset
            Dim iHeight As Double = yindex + 0.5
            Dim cellWidthdiff As Double = 3
            If Me.Unit.Length <= 0 Then
                cellWidthdiff = 4
            End If
            mesh.Positions.Add(New Point3D(-X + iXIncr, iHeight, -Z + iZIncr))
            mesh.Positions.Add(New Point3D(-X + iXIncr, yindex, -Z + iZIncr))
            mesh.Positions.Add(New Point3D(-wid + iXIncr - cellWidthdiff, yindex, -Z + iZIncr))

            mesh.TextureCoordinates.Add(New System.Windows.Point(0, 0))
            mesh.TextureCoordinates.Add(New System.Windows.Point(0, 1))
            mesh.TextureCoordinates.Add(New System.Windows.Point(1, 1))

            mesh.TriangleIndices.Add(0)
            mesh.TriangleIndices.Add(1)
            mesh.TriangleIndices.Add(2)

            mesh.Positions.Add(New Point3D(-X + iXIncr, iHeight, -Z + iZIncr))
            mesh.Positions.Add(New Point3D(-wid + iXIncr - cellWidthdiff, yindex, -Z + iZIncr))
            mesh.Positions.Add(New Point3D(-wid + iXIncr - cellWidthdiff, iHeight, -Z + iZIncr))

            mesh.TextureCoordinates.Add(New System.Windows.Point(0, 0))
            mesh.TextureCoordinates.Add(New System.Windows.Point(1, 1))
            mesh.TextureCoordinates.Add(New System.Windows.Point(1, 0))

            mesh.TriangleIndices.Add(3)
            mesh.TriangleIndices.Add(4)
            mesh.TriangleIndices.Add(5)

            Dim tb As New TextBlock(New Run(data))
            tb.Foreground = System.Windows.Media.Brushes.Red
            tb.FontFamily = New System.Windows.Media.FontFamily("Arial")

            Dim mat As New DiffuseMaterial()
            mat.Brush = New VisualBrush(tb)

            Dim surface_model As New GeometryModel3D(mesh, mat)

            surface_model.BackMaterial = mat

            Return surface_model
        End Function

        Private Sub element_MouseEnter(sender As Object, e As MouseEventArgs)
            Dim p As System.Windows.Point = e.GetPosition(Nothing)
            lblVals.Margin = New Thickness(p.X - 4, p.Y - 2, 20, 20)
            lblVals.Visibility = Visibility.Visible

            For modelindex As Integer = 0 To modeluilist.Count - 1
                If modeluilist(modelindex) Is e.Source Then
                    Dim iCout As Integer = 0
                    For row As Integer = 0 To valuelist.GetLength(0) - 1
                        For col As Integer = 0 To valuelist.GetLength(1) - 1

                            If modelindex = iCout Then
                                lblVals.Content = String.Format("{0}", valuelist(row, col))

                                Return
                            End If
                            iCout += 1
                        Next

                    Next
                End If
            Next
        End Sub

        Private Sub DrawSensorData(modelgroup As Model3DGroup, data As String, Xindex As Integer, Zindex As Integer)
            Dim meshgeometry As New MeshGeometry3D()

            Dim iXIncr As Double = Xindex * 5
            Dim iZIncr As Double = Zindex * 5


            Dim xoffset As Double = 1, zoffset As Double = 2
            iZIncr = iZIncr + zoffset
            iXIncr = iXIncr + xoffset
            Dim iHeight As Double = -4.5
            Dim cellWidth As Double = 4


            meshgeometry.Positions.Add(New Point3D(-10 + iXIncr, iHeight, -10 + iZIncr))
            meshgeometry.Positions.Add(New Point3D(-10 + iXIncr, -5, -10 + iZIncr))
            meshgeometry.Positions.Add(New Point3D(-5 + iXIncr - cellWidth, -5, -10 + iZIncr))

            meshgeometry.TextureCoordinates.Add(New System.Windows.Point(0, 0))
            meshgeometry.TextureCoordinates.Add(New System.Windows.Point(0, 1))
            meshgeometry.TextureCoordinates.Add(New System.Windows.Point(1, 1))

            meshgeometry.TriangleIndices.Add(0)
            meshgeometry.TriangleIndices.Add(1)
            meshgeometry.TriangleIndices.Add(2)


            meshgeometry.Positions.Add(New Point3D(-10 + iXIncr, iHeight, -10 + iZIncr))
            meshgeometry.Positions.Add(New Point3D(-5 + iXIncr - cellWidth, -5, -10 + iZIncr))
            meshgeometry.Positions.Add(New Point3D(-5 + iXIncr - cellWidth, iHeight, -10 + iZIncr))


            meshgeometry.TextureCoordinates.Add(New System.Windows.Point(0, 0))
            meshgeometry.TextureCoordinates.Add(New System.Windows.Point(1, 1))
            meshgeometry.TextureCoordinates.Add(New System.Windows.Point(1, 0))


            meshgeometry.TriangleIndices.Add(3)
            meshgeometry.TriangleIndices.Add(4)
            meshgeometry.TriangleIndices.Add(5)


            Dim tb As New TextBlock(New Run(data))
            tb.Foreground = System.Windows.Media.Brushes.Blue
            tb.FontFamily = New System.Windows.Media.FontFamily("Arial")

            Dim material As New DiffuseMaterial()
            material.Brush = New VisualBrush(tb)
            Dim geometrymodel As New GeometryModel3D(meshgeometry, material)
            geometrymodel.BackMaterial = material

            modelgroup.Children.Add(geometrymodel)
        End Sub

        Private Function CreatePoints(DAT As Single(,)) As Double(,)
            iRows = DAT.GetLength(0)
            iCols = DAT.GetLength(1)
            yposlist = New Integer(iRows - 1, iCols - 1) {}
            Dim fMax As Single = Single.MinValue
            Dim fMin As Single = Single.MaxValue

            If Me.FixedColorScale Then
                fMax = CSng(Me.ScaleMax)
                fMin = CSng(Me.ScaleMin)
                If fMax <> m_fMax OrElse fMin <> m_fMin Then

                    If fMax = fMin Then
                        Dim fShift As Single = (fMax / 10)
                        fMax = fMax + fShift
                        fMin = fMin - fShift
                    End If
                End If

                m_fMax = fMax
                m_fMin = fMin
            Else

                For i As Integer = 0 To iRows - 1
                    For j As Integer = 0 To iCols - 1
                        If DAT(i, j) > -200 AndAlso DAT(i, j) < fMin Then
                            fMin = DAT(i, j)
                        End If
                        If DAT(i, j) > fMax AndAlso DAT(i, j) < 400 Then
                            fMax = DAT(i, j)
                        End If
                    Next
                Next
                If fMax = fMin Then
                    fMin = 0
                End If
                If fMax <> m_fMax OrElse fMin <> m_fMin Then
                    If fMax = fMin Then
                        Dim fShift As Single = (fMax / 10)
                        fMax = fMax + fShift
                        fMin = fMin - fShift
                    End If
                    m_fMax = fMax
                    m_fMin = fMin
                End If
            End If

            pts = New PointC(iRows - 1, iCols - 1) {}

            For i As Integer = 0 To iRows - 1
                For j As Integer = 0 To iCols - 1

                    pts(i, j) = New PointC(New PointF(j, i), DAT(i, j))
                Next
            Next

            Dim iPoints As Integer = 5
            Dim newArrRow As Integer = (iPoints + 1) * (iRows - 1)
            Dim newArrCol As Integer = (iPoints + 1) * (iCols - 1)
            Dim fNewArr As Double(,) = New Double(newArrCol - 1, newArrRow - 1) {}
            Dim iAddRow As Integer = 0
            Dim iAddCol As Integer = 0
            For i As Integer = 0 To iRows - 2
                iAddRow = i * (iPoints + 1)

                For j As Integer = 0 To iCols - 2

                    Dim pta As PointF() = New PointF(3) {pts(i, j).pointf, pts(i + 1, j).pointf, pts(i + 1, j + 1).pointf, pts(i, j + 1).pointf}
                    Dim cdata As Single() = New Single(3) {pts(i, j).C, pts(i + 1, j).C, pts(i + 1, j + 1).C, pts(i, j + 1).C}

                    iAddCol = (j * (iPoints + 1))

                    Dim iVal As Integer(,) = Interp(pta, cdata, iPoints, m_fMax, m_fMin)

                    For rowindex As Integer = 0 To iVal.GetUpperBound(0)
                        For colindex As Integer = 0 To iVal.GetUpperBound(1)
                            fNewArr(rowindex + iAddCol, colindex + iAddRow) = iVal(rowindex, colindex)
                            If yposlist(i, j) < iVal(rowindex, colindex) Then
                                yposlist(i, j) = iVal(rowindex, colindex)
                            End If
                        Next

                    Next
                Next
            Next
            Dim iMax As Integer = fNewArr.GetUpperBound(0)
            Dim jMax As Integer = fNewArr.GetUpperBound(1)

            For i As Integer = 0 To iMax
                For j As Integer = 0 To jMax
                    If (i = 0) OrElse (j = 0) OrElse (j = fNewArr.GetUpperBound(1)) OrElse (i = fNewArr.GetUpperBound(0)) Then
                        fNewArr(i, j) = 0
                    End If
                Next
            Next

            Return fNewArr
        End Function

        Private Function Interp(pta As PointF(), cData As Single(), npoints As Integer, cmax As Single, cmin As Single) As Integer(,)
            Dim pts As PointC(,) = New PointC(npoints, npoints) {}
            Dim x0 As Single = pta(0).X
            Dim x1 As Single = pta(3).X
            Dim y0 As Single = pta(0).Y
            Dim y1 As Single = pta(1).Y
            Dim dx As Single = (x1 - x0) / npoints
            Dim dy As Single = (y1 - y0) / npoints
            Dim C00 As Single = cData(0)
            Dim C10 As Single = cData(1)
            Dim C11 As Single = cData(2)
            Dim C01 As Single = cData(3)

            For i As Integer = 0 To npoints
                Dim x As Single = x0 + i * dx
                '
                For j As Integer = 0 To npoints
                    Dim y As Single = y0 + j * dy
                    Dim C As Single = (y1 - y) * ((x1 - x) * C00 + (x - x0) * C10) / (x1 - x0) / (y1 - y0) + (y - y0) * ((x1 - x) * C01 + (x - x0) * C11) / (x1 - x0) / (y1 - y0)
                    pts(j, i) = New PointC(New PointF(x, y), C)
                Next
            Next

            Dim fVal As Integer(,) = New Integer(npoints, npoints) {}

            For i As Integer = 0 To npoints

                For j As Integer = 0 To npoints
                    Dim cindex As Integer = CInt(Math.Truncate(Math.Round((colorLength * (pts(i, j).C - cmin) + (cmax - pts(i, j).C)) / (cmax - cmin))))
                    If cindex < 1 Then
                        cindex = 1
                    End If
                    If cindex > colorLength Then
                        cindex = colorLength
                    End If


                    fVal(i, j) = cindex
                Next
            Next

            Return fVal
        End Function
    End Class
End Namespace
