#Region "Microsoft.VisualBasic::9c125591dfd2e9f45a2cc3c75978b4ad, Data_science\Mathematica\Math\Math.Statistics\ShapleyValue\TreeShap\PathElement.vb"

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

    '   Total Lines: 29
    '    Code Lines: 11 (37.93%)
    ' Comment Lines: 15 (51.72%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (10.34%)
    '     File Size: 906 B


    '     Class PathElement
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ShapleyValue.TreeShape

    ''' <summary>
    ''' Path element for use in <seealso cref="ShapAlgo2"/>
    ''' </summary>
    Public Class PathElement
        ''' <summary>
        ''' (m.d)
        ''' </summary>
        Public featureIndex As Integer = 0
        ''' <summary>
        ''' (m.z)
        ''' </summary>
        Public zeroFraction As Double = Double.NaN
        ''' <summary>
        ''' (m.o)
        ''' </summary>
        Public oneFraction As Double = Double.NaN
        ''' <summary>
        ''' (m.w)
        ''' </summary>
        Public weight As Double = Double.NaN

        Public Overrides Function ToString() As String
            Return "PE{f=" & featureIndex.ToString() & ", zf=" & zeroFraction.ToString() & ", of=" & oneFraction.ToString() & ", pw=" & weight.ToString() & "}"c.ToString()
        End Function
    End Class

End Namespace
