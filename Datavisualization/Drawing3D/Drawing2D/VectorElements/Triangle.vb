Imports System.Drawing

Namespace Drawing2D.VectorElements

    Public Class Triangle : Inherits LayoutsElement

        Public Property Color As Color

        Public Property Vertex1 As Point
        Public Property Vertex2 As Point
        Public Property Vertex3 As Point
        Public Property Angle As Double

        Sub New(Location As Point, GDI As GDIPlusDeviceHandle, Color As Color)
            Call MyBase.New(GDI, Location)
            Me.Color = Color
        End Sub

        ''' <summary>
        ''' 直角三角形
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function DrawAsRightTriangle(a As Integer, b As Integer) As Triangle
            Throw New NotImplementedException
        End Function

        Protected Overloads Overrides Sub InvokeDrawing()

        End Sub

        Public Overrides ReadOnly Property Size As Size
            Get
                Throw New NotImplementedException
            End Get
        End Property
    End Class
End Namespace