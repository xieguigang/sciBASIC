Imports System
Imports System.Collections.Generic
Imports std = System.Math

Namespace HoughCircles

    Public Class TableRow

        Public Length As Double
        Public Angle As Double

        Public Sub New(angle As Double, length As Double)
            Me.Length = length
            Me.Angle = angle
        End Sub
    End Class

    Public Class Table

        Public Shared ReadOnly Square As TableRow() = {
            New TableRow(0, 1),
            New TableRow(std.PI / 4, std.Sqrt(2.0R)),
            New TableRow(std.PI / 2, 1),
            New TableRow(3 * std.PI / 4, std.Sqrt(2.0R)),
            New TableRow(std.PI, 1),
            New TableRow(-std.PI / 4, 1),
            New TableRow(-std.PI / 2, 1),
            New TableRow(-3 * std.PI / 4, std.Sqrt(2.0R))
        }
    End Class
End Namespace
