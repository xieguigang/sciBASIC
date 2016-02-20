Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataStructures

Public Class BezierCurve

    Dim _bezierPoints As List(Of PointF)

    ''' <summary>
    ''' store the list of points in the bezier curve
    ''' </summary>
    Public ReadOnly Property BezierPointList() As List(Of PointF)
        Get
            Return _bezierPoints
        End Get
    End Property

    Dim _InitPoints As List(Of PointF) = New List(Of PointF)()

    ''' <summary>
    ''' store the list of initial points
    ''' </summary>
    Public ReadOnly Property InitPointsList() As List(Of PointF)
        Get
            If _InitPoints Is Nothing Then
                _InitPoints = New List(Of PointF)()
            End If
            Return _InitPoints
        End Get
    End Property

    ''' <summary>
    ''' store the number of iterations
    ''' </summary>
    Public Property Iterations As Integer

    Public Sub New(ctrl1 As PointF, ctrl2 As PointF, ctrl3 As PointF, iteration As Integer)
        ReCalculate(ctrl1, ctrl2, ctrl3, iteration)
    End Sub

    ''' <summary>
    ''' recreate the bezier curve.
    ''' </summary>
    ''' <param name="ctrl1">first initial point</param>
    ''' <param name="ctrl2">second initial point</param>
    ''' <param name="ctrl3">third initial point</param>
    ''' <param name="iteration">number of iteration of the algorithm</param>
    ''' <returns>the list of points in the curve</returns>
    Public Function ReCalculate(ctrl1 As PointF, ctrl2 As PointF, ctrl3 As PointF, iteration As Integer) As List(Of PointF)
        Iterations = iteration
        _InitPoints.Clear()
        _InitPoints.AddRange(New PointF() {ctrl1, ctrl2, ctrl3})
        CreateBezier(ctrl1, ctrl2, ctrl3)
        Return _bezierPoints
    End Function

    ''' <summary>
    ''' create a bezier curve
    ''' </summary>
    ''' <param name="ctrl1">first initial point</param>
    ''' <param name="ctrl2">second initial point</param>
    ''' <param name="ctrl3">third initial point</param>
    Private Sub CreateBezier(ctrl1 As PointF, ctrl2 As PointF, ctrl3 As PointF)
        _bezierPoints = New List(Of PointF)()
        _bezierPoints.Clear()
        _bezierPoints.Add(ctrl1)
        ' add the first control point
        PopulateBezierPoints(ctrl1, ctrl2, ctrl3, 0)
        _bezierPoints.Add(ctrl3)
        ' add the last control point
    End Sub

    ''' <summary>
    ''' Recursivly call to construct the bezier curve with control points
    ''' </summary>
    ''' <param name="ctrl1">first control point of bezier curve segment</param>
    ''' <param name="ctrl2">second control point of bezier curve segment</param>
    ''' <param name="ctrl3">third control point of bezier curve segment</param>
    ''' <param name="currentIteration">the current interation of a branch</param>
    Private Sub PopulateBezierPoints(ctrl1 As PointF, ctrl2 As PointF, ctrl3 As PointF, currentIteration As Integer)
        If currentIteration < _Iterations Then
            'calculate next mid points
            Dim midPoint1 As PointF = MidPoint(ctrl1, ctrl2)
            Dim midPoint2 As PointF = MidPoint(ctrl2, ctrl3)
            Dim midPoint3 As PointF = MidPoint(midPoint1, midPoint2)
            'the next control point
            currentIteration += 1
            PopulateBezierPoints(ctrl1, midPoint1, midPoint3, currentIteration)
            'left branch
            _bezierPoints.Add(midPoint3)
            'add the next control point
            'right branch
            PopulateBezierPoints(midPoint3, midPoint2, ctrl3, currentIteration)
        End If
    End Sub

    ''' <summary>
    ''' Find mid point
    ''' </summary>
    ''' <param name="controlPoint1">first control point</param>
    ''' <param name="controlPoint2">second control point</param>
    ''' <returns></returns>
    Private Function MidPoint(controlPoint1 As PointF, controlPoint2 As PointF) As PointF
        Return New PointF((controlPoint1.X + controlPoint2.X) / 2, (controlPoint1.Y + controlPoint2.Y) / 2)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="Parallel">并行版本的</param>
    ''' <param name="WindowSize">数据采样的窗口大小，默认大小是<paramref name="data"></paramref>的百分之1</param>
    ''' <returns></returns>
    ''' <remarks>先对数据进行采样，然后插值，最后返回插值后的平滑曲线数据以用于下一步分析</remarks>
    Public Shared Function BezierSmoothInterpolation(data As Double(),
                                                     Optional WindowSize As Integer = -1,
                                                     Optional iteration As Integer = 3,
                                                     Optional Parallel As Boolean = False) As Double()
        If WindowSize <= 0 Then
            WindowSize = data.Count / 100
        End If

        If WindowSize < 3 Then
            WindowSize = 3 '最少需要3个点进行插值
        End If

        Dim SlideWindows = data.CreateSlideWindows(WindowSize, offset:=WindowSize - 1)
        Dim ChunkBuffer As List(Of Double) = New List(Of Double)

        If Parallel Then
            Dim LQuery = (From win In SlideWindows.AsParallel
                          Select idt = __interpolation(win.Elements, iteration), i = win.p
                          Order By i Ascending).ToArray
            For Each win In LQuery
                Call ChunkBuffer.AddRange(win.idt)
            Next
        Else
            Dim LQuery = (From win As SlideWindowHandle(Of Double) In SlideWindows
                          Select idt = __interpolation(win.Elements, iteration), i = win.p
                          Order By i Ascending).ToArray
            For Each win In LQuery
                Call ChunkBuffer.AddRange(win.idt)
            Next
        End If

        Return ChunkBuffer.ToArray
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Elements"></param>
    ''' <param name="iteration"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Shared Function __interpolation(Elements As Double(), iteration As Integer) As Double()
        Dim data As Double() = New Double(2) {}

        data(0) = Elements.First
        data(1) = Elements(Elements.Count / 2)
        data(2) = Elements.Last

        Dim ChunkTemp = New BezierCurve(New PointF(0, data(0)), New PointF(1, data(1)), New PointF(2, data(2)), iteration).BezierPointList
        Elements = (From p In ChunkTemp Select CDbl(p.Y)).ToArray
        Return Elements
    End Function
End Class
