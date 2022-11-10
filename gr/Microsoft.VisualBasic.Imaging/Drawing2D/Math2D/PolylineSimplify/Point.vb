' High-performance polyline simplification library
'
' This is a port of simplify-js by Vladimir Agafonkin, Copyright (c) 2012
' https://github.com/mourner/simplify-js
' 
' The code is ported from JavaScript to C#.
' The library is created as portable and 
' is targeting multiple Microsoft plattforms.
'
' This library was ported by imshz @ http://www.shz.no
' https://github.com/imshz/simplify-net
'
' This code is provided as is by the author. For complete license please
' read the original license at https://github.com/mourner/simplify-js

Namespace Drawing2D.Math2D.PolylineSimplify

    Public Class Point : Implements IEquatable(Of Point)

        Public X As Double
        Public Y As Double
        Public Z As Double

        Public Sub New(x As Double, y As Double, Optional z As Double = 0)
            Me.X = x
            Me.Y = y
            Me.Z = z
        End Sub

        Public ReadOnly Property IsValid As Boolean
            Get
                Return X <= 90.0 AndAlso Y >= -90.0 AndAlso Y <= 180.0 AndAlso X >= -180.0
            End Get
        End Property

        Public Overrides Function Equals(obj As Object) As Boolean
            If ReferenceEquals(Nothing, obj) Then Return False
            If ReferenceEquals(Me, obj) Then Return True
            If obj.GetType() IsNot GetType(Point) AndAlso obj.GetType() IsNot GetType(Point) Then Return False
            Return Equals(TryCast(obj, Point))
        End Function

        Public Overloads Function Equals(other As Point) As Boolean Implements IEquatable(Of Point).Equals
            If ReferenceEquals(Nothing, other) Then Return False
            If ReferenceEquals(Me, other) Then Return True
            Return other.X.Equals(X) AndAlso other.Y.Equals(Y) AndAlso other.Z.Equals(Z)
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return (X.GetHashCode() * 397) Xor Y.GetHashCode() Xor Z.GetHashCode()
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("{0} {1} {2}", X, Y, Z)
        End Function
    End Class
End Namespace
