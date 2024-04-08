' 
'  To change this license header, choose License Headers in Project Properties.
'  To change this template file, choose Tools | Templates
'  and open the template in the editor.
' 

Namespace Orthogonal.optimization

    ''' 
    ''' <summary>
    ''' @author santi
    ''' </summary>
    Public Interface EmbeddingComparator
        Function compare(oer1 As OrthographicEmbeddingResult, oer2 As OrthographicEmbeddingResult) As Integer
    End Interface

End Namespace
