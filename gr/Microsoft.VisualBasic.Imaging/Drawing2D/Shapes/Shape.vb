#Region "Microsoft.VisualBasic::308bf091e449b13a390f0744254b7c1e, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Shapes\Shape.vb"

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


    ' Code Statistics:

    '   Total Lines: 62
    '    Code Lines: 35
    ' Comment Lines: 15
    '   Blank Lines: 12
    '     File Size: 1.95 KB


    '     Class Shape
    ' 
    '         Properties: DrawingRegion, EnableAutoLayout, Location, TooltipTag
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Draw, MoveOffset, MoveTo, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

Namespace Drawing2D.Shapes

    ''' <summary>
    ''' 矢量图形
    ''' </summary>
    Public MustInherit Class Shape

        Public Property Location As Point
        Public Property TooltipTag As String

        Public MustOverride ReadOnly Property Size As Size

        Public ReadOnly Property DrawingRegion As Rectangle
            Get
                Return New Rectangle(Location, Size)
            End Get
        End Property

        ''' <summary>
        ''' 默认是允许自动组织布局的
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property EnableAutoLayout As Boolean = True

        Sub New(initLoci As Point)
            Location = initLoci
        End Sub

        Public Function MoveTo(pt As Point) As Shape
            Location = pt
            Return Me
        End Function

        Public Function MoveOffset(offset As Point) As Shape
            Location = New Point(Location.X + offset.X, Location.Y + offset.Y)
            Return Me
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="OverridesLoci">假若需要进行绘制到的时候复写当前的元素的位置，则请使用这个参数</param>
        ''' <returns>函数返回当前元素在绘制之后所占据的区域</returns>
        ''' <remarks></remarks>
        Public Overridable Function Draw(ByRef g As IGraphics, Optional overridesLoci As Point = Nothing) As RectangleF
            If Not overridesLoci.IsEmpty Then
                Me.Location = overridesLoci
            End If

            Return DrawingRegion
        End Function

        Public Overrides Function ToString() As String
            Return DrawingRegion.ToString
        End Function
    End Class
End Namespace
