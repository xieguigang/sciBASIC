#Region "Microsoft.VisualBasic::9d16ad80ea4faeb4bcc2fd82bc8a0e12, Data_science\Visualization\Plots\g\Theme\Layout.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:

    '     Class Layout
    ' 
    ' 
    ' 
    '     Class LayoutDependency
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetTarget, ToString
    ' 
    '     Class Absolute
    ' 
    '         Properties: isEmpty, x, y
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetLocation
    ' 
    '     Class PercentageRelative
    ' 
    '         Properties: isEmpty, target, x, y
    ' 
    '         Function: GetLocation
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Graphic.Canvas

    Public MustInherit Class Layout : Implements IsEmpty

        Friend MustOverride ReadOnly Property isEmpty As Boolean Implements IsEmpty.IsEmpty

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

        Friend Overrides ReadOnly Property isEmpty As Boolean
            Get
                Return (x.IsNaNImaginary OrElse x = 0.0) AndAlso (y.IsNaNImaginary OrElse y = 0.0)
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(location As Point)
            x = location.X
            y = location.Y
        End Sub

        Sub New(location As PointF)
            x = location.X
            y = location.Y
        End Sub

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

        Friend Overrides ReadOnly Property isEmpty As Boolean
            Get
                Return (x.IsNaNImaginary OrElse x = 0.0) AndAlso (y.IsNaNImaginary OrElse y = 0.0) AndAlso target.StringEmpty
            End Get
        End Property

        Public Overrides Function GetLocation(canvas As GraphicsRegion, dependency As LayoutDependency) As PointF
            Dim rect As RectangleF = dependency.GetTarget(target)
            Dim x = rect.Left + Me.x * rect.Width
            Dim y = rect.Top + Me.y * rect.Height

            Return New PointF(x, y)
        End Function
    End Class
End Namespace
