Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports NetGraph = Microsoft.VisualBasic.Data.visualize.Network.FileStream.Network

Namespace Analysis

    Public Module Extensions

        <Extension>
        Public Function SearchIndex(net As NetGraph, from As Boolean) As Dictionary(Of String, IndexOf(Of String))
            Dim getKey = Function(e As NetworkEdge)
                             If from Then
                                 Return e.FromNode
                             Else
                                 Return e.ToNode
                             End If
                         End Function
            Dim getValue = Function(e As NetworkEdge)
                               If from Then
                                   Return e.ToNode
                               Else
                                   Return e.FromNode
                               End If
                           End Function
            Dim index = net.Edges _
                .Select(Function(edge)
                            Return (key:=getKey(edge), value:=getValue(edge))
                        End Function) _
                .GroupBy(Function(t) t.key) _
                .ToDictionary(Function(k) k.Key,
                              Function(g) New IndexOf(Of String)(g.Select(Function(o) o.value).Distinct))
            Return index
        End Function
    End Module
End Namespace