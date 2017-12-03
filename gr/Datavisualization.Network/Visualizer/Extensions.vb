Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Language.Default

Module Extensions

    <Extension>
    Public Function NodeBrushAssert(node As Node) As Assert(Of Object)
        Return Function(null)
                   Return node Is Nothing OrElse
                          node.Data Is Nothing OrElse
                          node.Data.Color Is Nothing
               End Function
    End Function
End Module
