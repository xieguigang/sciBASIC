#Region "Microsoft.VisualBasic::265f01a4502ce527037de4fe68e69719, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\ConcaveHull\Triangle.vb"

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

    '   Total Lines: 80
    '    Code Lines: 54 (67.50%)
    ' Comment Lines: 12 (15.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (17.50%)
    '     File Size: 2.09 KB


    '     Structure TriangleIndex
    ' 
    '         Function: ToString
    ' 
    '     Structure TriangleVertex
    ' 
    ' 
    ' 
    '     Structure Triangle
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Structure EdgeInfo
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetEdgeType, IsValid
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Namespace Drawing2D.Math2D.ConcaveHull

    ''' <summary>
    ''' the 3d point index
    ''' </summary>
    Public Structure TriangleIndex

        Public vv0 As Long
        Public vv1 As Long
        Public vv2 As Long

        Public Overrides Function ToString() As String
            Return {vv0, vv1, vv2}.JoinBy(" - ")
        End Function
    End Structure

    Public Structure TriangleVertex
        Public p0 As Point
        Public p1 As Point
        Public p2 As Point
    End Structure

    Public Structure Triangle

        ''' <summary>
        ''' A
        ''' </summary>
        Public P0Index As Integer
        ''' <summary>
        ''' B
        ''' </summary>
        Public P1Index As Integer
        ''' <summary>
        ''' C
        ''' </summary>
        Public P2Index As Integer
        Public Index As Integer

        Public Sub New(p0index As Integer, p1index As Integer, p2index As Integer)
            Me.P0Index = p0index
            Me.P1Index = p1index
            Me.P2Index = p2index
            Me.Index = -1
        End Sub

        Public Sub New(p0index As Integer, p1index As Integer, p2index As Integer, index As Integer)
            Me.P0Index = p0index
            Me.P1Index = p1index
            Me.P2Index = p2index
            Me.Index = index
        End Sub
    End Structure

    Public Structure EdgeInfo

        Public P0Index As Integer
        Public P1Index As Integer
        Public AdjTriangle As List(Of Integer)
        Public Flag As Boolean
        Public Length As Double

        Public Function GetEdgeType() As Integer
            Return AdjTriangle.Count
        End Function

        Public Function IsValid() As Boolean
            Return P0Index <> -1
        End Function

        Public Sub New(d As Integer)
            P0Index = -1
            P1Index = -1
            Flag = False
            AdjTriangle = New List(Of Integer)()
            Length = -1
        End Sub
    End Structure
End Namespace
