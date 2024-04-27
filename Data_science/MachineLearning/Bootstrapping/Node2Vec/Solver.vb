﻿#Region "Microsoft.VisualBasic::44cb9d3a68d37fe00b0d8b637c4b12c5, G:/GCModeller/src/runtime/sciBASIC#/Data_science/MachineLearning/Bootstrapping//Node2Vec/Solver.vb"

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

    '   Total Lines: 85
    '    Code Lines: 47
    ' Comment Lines: 23
    '   Blank Lines: 15
    '     File Size: 3.14 KB


    '     Module Solver
    ' 
    '         Function: CreateEmbedding, CreateGraph
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.NLP.Word2Vec
Imports Microsoft.VisualBasic.Linq

Namespace node2vec

    ''' <summary>
    ''' Created by freemso on 17-3-14.
    ''' </summary>
    Public Module Solver

        ''' <summary>
        ''' create graph from a dataframe?
        ''' </summary>
        ''' <param name="u"></param>
        ''' <param name="v"></param>
        ''' <param name="w"></param>
        ''' <returns></returns>
        Public Function CreateGraph(u As String(), v As String(), Optional w As Double() = Nothing) As Graph
            Dim graph As New Graph

            If u.TryCount <> v.TryCount Then
                Throw New InvalidProgramException("the dimension size of the source node is mis-matched with the target nodes!")
            End If
            If w Is Nothing Then
                w = 1.0.Replicate(u.Length).ToArray
            End If

            Dim idencoder As Index(Of String) = u.JoinIterates(v).Distinct.Indexing

            For i As Integer = 0 To w.Length - 1
                ' add the nodes to the graph
                Dim node1 = graph.addNode(idencoder(u(i)), u(i))
                Dim node2 = graph.addNode(idencoder(v(i)), v(i))
                Dim weight As Double = w(i)

                ' add the edge to the graph
                Call graph.addEdge(node1, node2, weight)
            Next

            Call graph.preprocess()

            Return graph
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="graph"></param>
        ''' <param name="numWalks"></param>
        ''' <param name="walkLength"></param>
        ''' <param name="dimensions"></param>
        ''' <param name="windowSize"></param>
        ''' <returns>node mapping to a vector</returns>
        <Extension>
        Public Function CreateEmbedding(graph As Graph,
                                        Optional numWalks As Integer = 10,
                                        Optional walkLength As Integer = 80,
                                        Optional dimensions As Integer = 20,
                                        Optional windowSize As Integer = 10) As VectorModel

            ' use word2vec to do word embedding
            Dim model As New Word2VecFactory
            Dim engine As Word2Vec = model _
                .setMethod(TrainMethod.Skip_Gram) _
                .setWindow(windowSize) _
                .setVectorSize(dimensions) _
                .build()

            For Each path As IList(Of Vertex) In graph.simulateWalks(numWalks, walkLength)
                ' convert path list to string
                engine.readTokens(path.Select(Function(v) v.label).ToArray)
            Next

            VBDebugger.EchoLine("Learning Embedding...")
            engine.training()

            Dim vectors = engine.outputVector
            Return vectors
        End Function
    End Module

End Namespace
