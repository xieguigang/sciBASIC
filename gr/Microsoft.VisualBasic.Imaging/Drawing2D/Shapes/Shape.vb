#Region "Microsoft.VisualBasic::652f66b58df34b5c4a2f74523246163f, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Shapes\Shape.vb"

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

Namespace Drawing2D.Vector.Shapes

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
