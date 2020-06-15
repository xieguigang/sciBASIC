Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Graphic.Canvas

    Public MustInherit Class Layout

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="canvas"></param>
        ''' <param name="dependency">
        ''' a list of object with its ``[top-left]`` layout information. 
        ''' this objects table should always contains an ``canvas`` 
        ''' object layout information.
        ''' </param>
        ''' <returns></returns>
        Public MustOverride Function GetLocation(canvas As GraphicsRegion, dependency As LayoutDependency) As PointF

    End Class

    Public Class LayoutDependency

        ReadOnly dependency As Dictionary(Of String, RectangleF)

        ''' <summary>
        ''' create a new blank layout dependency
        ''' </summary>
        ''' <param name="canvas"></param>
        Sub New(canvas As GraphicsRegion)
            Dim rect As Rectangle = canvas.PlotRegion
            Dim rectf As New RectangleF(rect.Location.PointF, rect.Size.SizeF)

            dependency = New Dictionary(Of String, RectangleF) From {
                {NameOf(canvas), rectf}
            }
        End Sub

        Public Function GetTarget(obj As String) As RectangleF
            If dependency.ContainsKey(obj) Then
                Return dependency(obj)
            Else
                Throw New MissingPrimaryKeyException("missing layout dependency: " & obj)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return dependency.Keys.GetJson
        End Function

    End Class

    ''' <summary>
    ''' 绝对位置定位
    ''' </summary>
    Public Class Absolute : Inherits Layout

        Public Property x As Double
        Public Property y As Double

        Public Overrides Function GetLocation(canvas As GraphicsRegion, dependency As LayoutDependency) As PointF
            Return New PointF(x, y)
        End Function
    End Class

    ''' <summary>
    ''' 百分比相对定位
    ''' </summary>
    Public Class PercentageRelative : Inherits Layout

        Public Property x As Double
        Public Property y As Double
        Public Property target As String = "canvas"

        Public Overrides Function GetLocation(canvas As GraphicsRegion, dependency As LayoutDependency) As PointF
            Dim rect As RectangleF = dependency.GetTarget(target)
            Dim x = rect.Left + Me.x * rect.Width
            Dim y = rect.Top + Me.y * rect.Height

            Return New PointF(x, y)
        End Function
    End Class
End Namespace