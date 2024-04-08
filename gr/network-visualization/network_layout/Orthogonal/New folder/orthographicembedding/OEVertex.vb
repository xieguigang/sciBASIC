Imports System.Collections.Generic

' 
'  To change this license header, choose License Headers in Project Properties.
'  To change this template file, choose Tools | Templates
'  and open the template in the editor.
' 
Namespace Orthogonal.orthographicembedding

    ''' 
    ''' <summary>
    ''' @author santi
    ''' </summary>
    Public Class OEVertex

        Public embedding As IList(Of OEElement)
        Public v As Integer
        Public x, y As Double

        Public Sub New(a_embedding As IList(Of OEElement), a_v As Integer, a_x As Double, a_y As Double)
            embedding = a_embedding
            v = a_v
            x = a_x
            y = a_y
        End Sub

        Public Overrides Function ToString() As String
            Dim tmp As String = "v(" & x.ToString() & "," & y.ToString() & "):"
            For Each e As OEElement In embedding
                tmp += " " & e.ToString()
            Next
            Return tmp
        End Function
    End Class

End Namespace
