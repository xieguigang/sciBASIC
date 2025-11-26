#Region "Microsoft.VisualBasic::47dd11ec46eb25f6179c98400f3ae2e3, Data_science\Mathematica\Math\Math\Scripting\DataGenerator.vb"

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

    '   Total Lines: 40
    '    Code Lines: 29 (72.50%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (27.50%)
    '     File Size: 1.24 KB


    '     Module DataGenerator
    ' 
    ' 
    '         Delegate Function
    ' 
    '             Function: generateDataTuples
    ' 
    '     Class DataPoint
    ' 
    '         Properties: X, Y
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Imaging

Namespace Scripting

    Public Module DataGenerator

        Public Delegate Function DataFunction(x As Double) As Double

        Public Iterator Function generateDataTuples([function] As DataFunction, from As Double, [to] As Double, size As Integer) As IEnumerable(Of DataPoint)
            For Each xi As Double In seq(from, [to], by:=([to] - from) / size)
                Yield New DataPoint(xi, [function](xi))
            Next
        End Function

    End Module

    Public Class DataPoint : Implements Layout2D

        <XmlAttribute("x")> Public Property X As Double Implements Layout2D.X
        <XmlAttribute("y")> Public Overridable Property Y As Double Implements Layout2D.Y

        Sub New()
        End Sub

        Sub New(x As Double, y As Double)
            Me.X = x
            Me.Y = y
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{X}, {Y}]"
        End Function

        Public Shared Narrowing Operator CType(point As DataPoint) As PointF
            Return New PointF(point.X, point.Y)
        End Operator
    End Class
End Namespace
