Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

Namespace PostScript

    Public Class SpatialHashing

        ReadOnly gridSize As SizeF
        ReadOnly grid As New Dictionary(Of UInteger, Dictionary(Of UInteger, List(Of PSElement)))

        Public Sub New(width As Single, height As Single, Optional numGrids As Integer = 10)
            Me.gridSize = New SizeF(width / numGrids, height / numGrids)
        End Sub

        Public Sub AddPlot(ps As PostScriptBuilder)
            For Each element As PSElement In ps.AsEnumerable
                Call AddShape(element)
            Next
        End Sub

        Public Sub AddShape(shape As PSElement)
            Dim pos = shape.GetXy
            Dim size = shape.GetSize
            Dim startX As UInteger = HashX(pos.X)
            Dim endX As UInteger = HashX(pos.X + size.Width)
            Dim startY As UInteger = HashY(pos.Y)
            Dim endY As UInteger = HashY(pos.Y + size.Height)

            For x As UInteger = startX To endX
                If Not grid.ContainsKey(x) Then
                    Call grid.Add(x, New Dictionary(Of UInteger, List(Of PSElement)))
                End If

                Dim col = grid(key:=x)

                For y As UInteger = startY To endY
                    If Not col.ContainsKey(y) Then
                        Call col.Add(y, New List(Of PSElement))
                    End If

                    Call col(key:=y).Add(shape)
                Next
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function HashX(x As Single) As UInteger
            Return CUInt(std.Floor(x / gridSize.Width))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function HashY(y As Single) As UInteger
            Return CUInt(std.Floor(y / gridSize.Height))
        End Function

        Public Function FindShapeByPoint(x As Single, y As Single) As PSElement
            Dim hX As UInteger = HashX(x)
            Dim hY As UInteger = HashY(y)

            If grid.ContainsKey(hX) Then
                Dim col = grid(key:=hX)

                If col.ContainsKey(hY) Then
                    Return col(key:=hY).LastOrDefault
                End If
            End If

            Return Nothing
        End Function
    End Class
End Namespace