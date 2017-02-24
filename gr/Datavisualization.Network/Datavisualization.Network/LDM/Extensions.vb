Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Language

Namespace Abstract

    Public Module Extensions

        ''' <summary>
        ''' 移除的重复的边
        ''' </summary>
        ''' <remarks></remarks>
        ''' <param name="directed">是否忽略方向？</param>
        ''' <param name="ignoreTypes">是否忽略边的类型？</param>
        <Extension> Public Function RemoveDuplicated(Of T As NetworkEdge)(edges As IEnumerable(Of T), Optional directed As Boolean = True, Optional ignoreTypes As Boolean = False) As T()
            Dim uid = Function(edge As T) As String
                          If directed Then
                              Return edge.GetDirectedGuid(ignoreTypes)
                          Else
                              Return edge.GetNullDirectedGuid(ignoreTypes)
                          End If
                      End Function
            Dim LQuery = edges _
                .GroupBy(uid) _
                .Select(Function(g) g.First) _
                .ToArray

            Return LQuery
        End Function
    End Module
End Namespace