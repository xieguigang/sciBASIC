#Region "Microsoft.VisualBasic::577036fccb7e44589b404dc2681907e1, gr\network-visualization\network_layout\Orthogonal\optimization\SegmentLengthEmbeddingComparator.vb"

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

    '   Total Lines: 58
    '    Code Lines: 30 (51.72%)
    ' Comment Lines: 12 (20.69%)
    '    - Xml Docs: 25.00%
    ' 
    '   Blank Lines: 16 (27.59%)
    '     File Size: 2.00 KB


    '     Class SegmentLengthEmbeddingComparator
    ' 
    '         Function: (+2 Overloads) compare, resultScore
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
'  To change this license header, choose License Headers in Project Properties.
'  To change this template file, choose Tools | Templates
'  and open the template in the editor.
' 


Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Orthogonal.util

Namespace Orthogonal.optimization

    ''' 
    ''' <summary>
    ''' @author santi
    ''' </summary>
    Public Class SegmentLengthEmbeddingComparator
        Implements EmbeddingComparator

        Public Overridable Function compare(oer1 As OrthographicEmbeddingResult, oer2 As OrthographicEmbeddingResult) As Integer Implements EmbeddingComparator.compare
            Return compare(resultScore(oer1), resultScore(oer2))
        End Function

        ''' <summary>
        ''' true is s1 is better than s2
        ''' </summary>
        ''' <param name="s1"></param>
        ''' <param name="s2"></param>
        ''' <returns></returns>
        Public Overridable Function compare(s1 As Pair(Of Double, Integer), s2 As Pair(Of Double, Integer)) As Integer
            Dim tmp = s1.m_a.CompareTo(s2.m_a)
            If tmp <> 0 Then
                Return tmp
            End If
            Return s1.m_b.CompareTo(s2.m_b)
        End Function

        ''' <summary>
        ''' the score of a result is a pair with the accumulated length of the segments and the number of segments
        ''' </summary>
        ''' <param name="o"></param>
        ''' <returns></returns>
        Public Overridable Function resultScore(o As OrthographicEmbeddingResult) As Pair(Of Double, Integer)
            Dim length As Double = 0
            Dim nsegments = 0

            Dim nv = o.x.Length
            For i = 0 To nv - 1
                For j = 0 To nv - 1
                    If o.edges(i)(j) Then
                        nsegments += 1
                        length += (o.x(i) - o.x(j)) * (o.x(i) - o.x(j)) + (o.y(i) - o.y(j)) * (o.y(i) - o.y(j))
                    End If
                Next
            Next

            Return New Pair(Of Double, Integer)(length, nsegments)
        End Function

    End Class

End Namespace
