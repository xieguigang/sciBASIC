#Region "Microsoft.VisualBasic::77c02ebf2c81bb7cf59b97918d9bc15a, ..\visualbasic_App\gr\Microsoft.VisualBasic.Imaging\Drawing2D\GraphicsRegion.vb"

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
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing2D

    ''' <summary>
    ''' 绘图区域的参数
    ''' </summary>
    Public Structure GraphicsRegion

        Public Size As Size
        Public Margin As Size

        Public ReadOnly Property PlotRegion As Rectangle
            Get
                Dim topLeft As New Point(Margin.Width, Margin.Height)
                Dim size As New Size(
                    Me.Size.Width - Margin.Width * 2,
                    Me.Size.Height - Margin.Height * 2)

                Return New Rectangle(topLeft, size)
            End Get
        End Property

        Public ReadOnly Property EntireArea As Rectangle
            Get
                Return New Rectangle(New Point, Size)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace
