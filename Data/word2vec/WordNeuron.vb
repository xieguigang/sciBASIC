#Region "Microsoft.VisualBasic::6d0baebdac07b0d90d629848d101de4b, sciBASIC#\Data\word2vec\WordNeuron.vb"

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

    '   Total Lines: 38
    '    Code Lines: 24
    ' Comment Lines: 4
    '   Blank Lines: 10
    '     File Size: 1.04 KB


    '     Class WordNeuron
    ' 
    '         Properties: name, pathNeurons
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.NLP.Word2Vec.utils
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace NlpVec

    ''' <summary>
    ''' Created by fangy on 13-12-17.
    ''' 词神经元
    ''' </summary>
    Public Class WordNeuron : Inherits HuffmanNeuron

        Public Property name As String

        Public ReadOnly Property pathNeurons As IList(Of HuffmanNode)
            Get

                If m_pathNeurons IsNot Nothing Then
                    Return m_pathNeurons
                End If

                m_pathNeurons = HuffmanTree.getPath(Me)
                Return m_pathNeurons
            End Get
        End Property

        Dim m_pathNeurons As IList(Of HuffmanNode)

        Public Sub New(name As String, freq As Integer, vectorSize As Integer)
            MyBase.New(freq, vectorSize)

            Me.name = name

            For i = 0 To vector.Length - 1
                vector(i) = (randf.seeds.NextDouble - 0.5) / vectorSize
            Next
        End Sub
    End Class
End Namespace
