#Region "Microsoft.VisualBasic::0e4e5f843232eb29e85bce863518ccd3, gr\network-visualization\network_layout\EdgeBundling\Mingle\Accessor.vb"

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

    '   Total Lines: 51
    '    Code Lines: 42 (82.35%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (17.65%)
    '     File Size: 1.80 KB


    '     Class Accessor
    ' 
    '         Function: activate, getByDimension, GetDimensions, metric, nodeIs
    ' 
    '         Sub: setByDimensin
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree
Imports stdNum = System.Math

Namespace EdgeBundling.Mingle

    Public Class Accessor : Inherits KdNodeAccessor(Of GraphKdNode)

        Public Overrides Function GetDimensions() As String()
            Return {"x", "y", "z", "w"}
        End Function

        Public Overrides Sub setByDimension(x As GraphKdNode, dimName As String, value As Double)
            Select Case dimName
                Case "x" : x.x = value
                Case "y" : x.y = value
                Case "z" : x.z = value
                Case "w" : x.w = value
                Case Else
                    Throw New InvalidCastException(dimName)
            End Select
        End Sub

        Public Overrides Function metric(a As GraphKdNode, b As GraphKdNode) As Double
            Dim diff0 = a.x - b.x
            Dim diff1 = a.y - b.y
            Dim diff2 = a.z - b.z
            Dim diff3 = a.w - b.w

            Return stdNum.Sqrt(diff0 * diff0 + diff1 * diff1 + diff2 * diff2 + diff3 * diff3)
        End Function

        Public Overrides Function getByDimension(x As GraphKdNode, dimName As String) As Double
            Select Case dimName
                Case "x" : Return x.x
                Case "y" : Return x.y
                Case "z" : Return x.z
                Case "w" : Return x.w
                Case Else
                    Throw New InvalidCastException(dimName)
            End Select
        End Function

        Public Overrides Function nodeIs(a As GraphKdNode, b As GraphKdNode) As Boolean
            Return a Is b
        End Function

        Public Overrides Function activate() As GraphKdNode
            Return New GraphKdNode
        End Function
    End Class
End Namespace
