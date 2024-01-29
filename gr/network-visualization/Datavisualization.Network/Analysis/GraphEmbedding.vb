Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Analysis

    ''' <summary>
    ''' Embedding of a graph object as numeric vector
    ''' </summary>
    Public Module GraphEmbedding

        <Extension>
        Public Iterator Function ClassNameTuples(classNames As IEnumerable(Of String)) As IEnumerable(Of (String, String))
            For Each tuple As Tuple(Of String, String) In Comb(Of String).CreateObject(classNames).CombList.IteratesALL
                Yield (tuple.Item1, tuple.Item2)
            Next
        End Function

        ''' <summary>
        ''' Embedding a network graph object as a numeric vector
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="classNames">The node class names that used for generates the vector elements</param>
        ''' <returns>
        ''' edge weight as the vector value
        ''' </returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToVector(g As NetworkGraph, classNames As String(), Optional directed As Boolean = True) As Vector
            Return g.ToVector(classNames.ClassNameTuples.ToArray, directed)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function NodeIndex(cls As IEnumerable(Of Node)) As Index(Of String)
            Return cls.Select(Function(v) v.label).Indexing
        End Function

        <Extension>
        Public Function ToVector(g As NetworkGraph, classNames As (u_cls As String, v_cls As String)(), Optional directed As Boolean = True) As Vector
            Dim v As Double() = New Double(classNames.Length - 1) {}
            Dim classNodes = g.vertex _
                .GroupBy(Function(a) a.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE)) _
                .ToDictionary(Function(cls) cls.Key,
                              Function(cls)
                                  Return cls.NodeIndex
                              End Function)
            Dim edges As Edge() = g.graphEdges.ToArray

            For i As Integer = 0 To classNames.Length - 1
                Dim edgeType = classNames(i)
                Dim us = classNodes.TryGetValue(edgeType.u_cls)
                Dim vs = classNodes.TryGetValue(edgeType.v_cls)

                If us Is Nothing OrElse vs Is Nothing Then
                    v(i) = 0
                Else
                    For Each edge As Edge In edges
                        If edge.U.label Like us AndAlso edge.V.label Like vs Then
                            v(i) += edge.weight
                        End If
                        If Not directed Then
                            If edge.V.label Like us AndAlso edge.U.label Like vs Then
                                v(i) += edge.weight
                            End If
                        End If
                    Next
                End If
            Next

            Return New Vector(v)
        End Function
    End Module
End Namespace