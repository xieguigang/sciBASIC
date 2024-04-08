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


        ' true is s1 is better than s2:
        Public Overridable Function compare(s1 As Pair(Of Double, Integer), s2 As Pair(Of Double, Integer)) As Integer

            '        System.out.println("<" + s1.m_a + "," + s1.m_b + "> vs <" + s2.m_a + "," + s2.m_b + ">");

            Dim tmp = s1.m_a.CompareTo(s2.m_a)
            If tmp <> 0 Then
                Return tmp
            End If
            Return s1.m_b.CompareTo(s2.m_b)
        End Function


        ' the score of a result is a pair with the accumulated length of the segments and the number of segments
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
