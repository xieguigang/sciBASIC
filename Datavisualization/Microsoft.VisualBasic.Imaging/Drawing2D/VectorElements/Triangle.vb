#Region "Microsoft.VisualBasic::b1bf8f934c233be3ae017bf784753feb, ..\visualbasic_App\Datavisualization\Microsoft.VisualBasic.Imaging\Drawing2D\VectorElements\Triangle.vb"

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

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

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
