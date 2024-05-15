#Region "Microsoft.VisualBasic::b6a2718d16fab8fbe6f6e2befd110a38, Data_science\MachineLearning\Bootstrapping\Graph2Vec.vb"

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

    '   Total Lines: 65
    '    Code Lines: 52
    ' Comment Lines: 0
    '   Blank Lines: 13
    '     File Size: 2.40 KB


    ' Class Graph2Vec
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: GraphVector, Setup
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.GraphTheory.SequenceGraphTransform
Imports Microsoft.VisualBasic.Data.NLP.Word2Vec
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class Graph2Vec

    ReadOnly vocabulary As New Dictionary(Of String, Char)
    ReadOnly getVocabulary As Func(Of Vertex, String)
    ReadOnly other As String

    Const offset As Integer = 33

    Dim sgt As SequenceGraphTransform

    Sub New(getVocabulary As Func(Of Vertex, String), Optional other As String = "*")
        Me.other = other
        Me.getVocabulary = getVocabulary
    End Sub

    Public Function Setup(terms As IEnumerable(Of String)) As Graph2Vec
        For Each term As String In terms.JoinIterates({other})
            Call vocabulary.Add(term, Chr(offset + vocabulary.Count))
        Next

        If vocabulary.Count >= (127 - offset) Then
            Call VBDebugger.EchoLine("alphabet set too much chars, the embedding vector will be very very long!")
        End If

        Dim char_corpus As String() = vocabulary.Values _
            .Select(Function(c) c.ToString) _
            .ToArray

        sgt = New SequenceGraphTransform(mode:=Modes.Fast)
        sgt.set_alphabets(char_corpus)

        Return Me
    End Function

    Public Function GraphVector(g As Graph) As Double()
        Dim graph As New node2vec.Graph(g)
        Dim nodes As VectorModel = node2vec.CreateEmbedding(graph, dimensions:=3)
        Dim sort As NamedCollection(Of Single)() = nodes _
            .AsEnumerable _
            .OrderBy(Function(v) (New Vector(v) ^ 2).Sum) _
            .ToArray
        Dim vertexSet = g.vertex.ToDictionary(Function(v) v.label)
        Dim chars As Char() = sort _
            .Select(Function(lb) getVocabulary(vertexSet(lb.name))) _
            .Select(Function(key)
                        If vocabulary.ContainsKey(key) Then
                            Return vocabulary(key)
                        Else
                            Return vocabulary(other)
                        End If
                    End Function) _
            .ToArray
        Dim vector As Double() = sgt.fitVector(New String(chars))

        Return vector
    End Function

End Class
