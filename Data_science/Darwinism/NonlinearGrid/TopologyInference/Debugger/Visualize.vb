Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Xml.Models

Public Module Visualize

    ''' <summary>
    ''' Create network graph model from the grid system status.
    ''' </summary>
    ''' <param name="grid"></param>
    ''' <param name="cutoff">
    ''' 系统中的变量与结果之间的相关度因子的阈值，低于这个阈值的边都会被删掉，也就是只会留下相关度较高的边
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function CreateGraph(grid As GridMatrix, Optional cutoff# = 1) As NetworkGraph
        Dim g As New NetworkGraph
        Dim node As Node
        Dim variableNames As New List(Of String)
        Dim edge As EdgeData

        For Each factor As NumericVector In grid.correlations
            node = New Node With {
                .data = New NodeData With {
                    .label = factor.name,
                    .origID = factor.name
                },
                .Label = factor.name,
                .ID = 0
            }

            variableNames += factor.name
            g.AddNode(node)
        Next

        For Each factor As NumericVector In grid.correlations
            For i As Integer = 0 To factor.Length - 1
                ' PCC/SPCC相关度等，位于[-1,1]之间
                ' 而这个的相关度则是位于[负无穷, 正无穷]之间
                If factor.name <> variableNames(i) AndAlso Math.Abs(factor(i)) >= cutoff Then
                    ' 跳过自己和自己的链接
                    ' 以及低相关度的节点链接
                    edge = New EdgeData With {
                        .label = $"{factor.name} ^ {variableNames(i)}",
                        .weight = factor(i)
                    }
                    g.CreateEdge(factor.name, variableNames(i), edge)
                End If
            Next
        Next

        Return g
    End Function
End Module
