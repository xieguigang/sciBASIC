Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Algorithm
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq

Namespace PostScript

    Public Class SpatialLookup

        ReadOnly xlookup As BlockSearchFunction(Of PSElement)
        ReadOnly ylookup As BlockSearchFunction(Of PSElement)

        Sub New(ps As PostScriptBuilder)
            Dim elements As PSElement() = ps.AsEnumerable.ToArray

            xlookup = CreateLookup(elements, Function(i) i.GetXy.X)
            ylookup = CreateLookup(elements, Function(i) i.GetXy.Y)
        End Sub

        Private Shared Function CreateLookup(elements As PSElement(), getAxis As Func(Of PSElement, Double)) As BlockSearchFunction(Of PSElement)
            Dim range As New DoubleRange(From i As PSElement In elements Select getAxis(i))
            Dim grid As Double = range.Length / 20

            Return New BlockSearchFunction(Of PSElement)(elements, getAxis, grid, fuzzy:=True)
        End Function

        Private Class InternalSearchXy : Inherits PSElement

            Public x As Single
            Public y As Single

            Sub New(x!, y!)
                Me.x = x
                Me.y = y
            End Sub

            Friend Overrides Sub WriteAscii(ps As Writer)
                Throw New NotImplementedException()
            End Sub

            Friend Overrides Sub Paint(g As IGraphics)
                Throw New NotImplementedException()
            End Sub

            Friend Overrides Function GetXy() As PointF
                Return New PointF(x, y)
            End Function

            Friend Overrides Function ScaleTo(scaleX As d3js.scale.LinearScale, scaleY As d3js.scale.LinearScale) As PSElement
                Throw New NotImplementedException()
            End Function
        End Class

        Public Function GetNeayby(x!, y!) As IEnumerable(Of PSElement)
            Dim q As New InternalSearchXy(x, y)
            Dim xs = xlookup.Search(q).ToArray
            Dim ys = ylookup.Search(q).ToArray

            Return xs.Intersect(ys)
        End Function

        Public Function FindNearby(x!, y!) As PSElement
            Return GetNeayby(x, y).OrderBy(Function(a) a.GetXy.Distance(x, y)).FirstOrDefault
        End Function

    End Class
End Namespace