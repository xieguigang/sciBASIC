#Region "Microsoft.VisualBasic::8f5913536bb30fbe944ab05e7939c704, Data_science\Graph\Model\GridGraph\GridCell.vb"

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

    '   Total Lines: 89
    '    Code Lines: 65 (73.03%)
    ' Comment Lines: 8 (8.99%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 16 (17.98%)
    '     File Size: 2.46 KB


    '     Class GridCell
    ' 
    '         Properties: data, index, Layout2D_X, Layout2D_Y, X
    '                     Y
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: Equals, GetHashCode, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

Namespace GridGraph

    Public Class GridCell(Of T) : Implements IPoint2D, Layout2D

        ''' <summary>
        ''' 二维数组之中的索引 
        ''' </summary>
        ''' <returns></returns>
        Public Property index As Point

        Dim m_data As T

        Public Property data As T
            Get
                Return m_data
            End Get
            Protected Set(value As T)
                m_data = value
            End Set
        End Property

#Region "Implements IPoint2D, Layout2D"
        Friend ReadOnly Property X As Integer Implements IPoint2D.X
            Get
                Return index.X
            End Get
        End Property

        Friend ReadOnly Property Y As Integer Implements IPoint2D.Y
            Get
                Return index.Y
            End Get
        End Property

        Private Property Layout2D_X As Double Implements Layout2D.X
            Get
                Return index.X
            End Get
            Set(value As Double)
                index = New Point(value, index.Y)
            End Set
        End Property

        Private Property Layout2D_Y As Double Implements Layout2D.Y
            Get
                Return index.Y
            End Get
            Set(value As Double)
                index = New Point(index.X, value)
            End Set
        End Property
#End Region

        Sub New()
        End Sub

        Sub New(pt As Point, x As T)
            index = pt
            data = x
        End Sub

        Sub New(x As Integer, y As Integer, Optional data As T = Nothing)
            index = New Point(x, y)
            m_data = data
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{index.X}, {index.Y}] {data.ToString}"
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If Not TypeOf obj Is GridCell(Of T) Then Return False
            Dim p = DirectCast(obj, GridCell(Of T))
            Return p.X = X AndAlso p.Y = Y
        End Function

        ''' <summary>
        ''' hashcode overrides for A-star algorithm
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetHashCode() As Integer
            Return (X << 2) Xor Y
        End Function

    End Class
End Namespace
