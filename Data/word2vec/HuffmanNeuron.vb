#Region "Microsoft.VisualBasic::422439da1cace6541b60ea325c08d1f9, sciBASIC#\Data\word2vec\HuffmanNeuron.vb"

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

    '   Total Lines: 55
    '    Code Lines: 43
    ' Comment Lines: 3
    '   Blank Lines: 9
    '     File Size: 1.74 KB


    '     Class HuffmanNeuron
    ' 
    '         Properties: frequency, parent
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: compareTo, merge
    ' 
    '         Sub: SetCode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.NLP.Word2Vec.utils

Namespace NlpVec

    ''' <summary>
    ''' Created by fangy on 13-12-20.
    ''' </summary>
    Public Class HuffmanNeuron
        Implements HuffmanNode

        Protected Friend parentNeuron As HuffmanNode
        Protected Friend code As Integer = 0
        Protected Friend vector As Double()

        Public Sub SetCode(value As Integer) Implements HuffmanNode.SetCode
            code = value
        End Sub

        Public Property frequency As Integer Implements HuffmanNode.frequency

        Public Property parent As HuffmanNode Implements HuffmanNode.parent
            Set(value As HuffmanNode)
                parentNeuron = value
            End Set
            Get
                Return parentNeuron
            End Get
        End Property

        Public Function merge(right As HuffmanNode) As HuffmanNode Implements HuffmanNode.merge
            Dim parent As HuffmanNode = New HuffmanNeuron(frequency + right.frequency, vector.Length)
            parentNeuron = parent
            SetCode(0)
            right.parent = parent
            right.SetCode(1)
            Return parent
        End Function

        Public Function compareTo(hn As HuffmanNode) As Integer Implements IComparable(Of HuffmanNode).CompareTo
            If frequency = hn.frequency Then
                Return 0
            ElseIf frequency > hn.frequency Then
                Return 1
            Else
                Return -1
            End If
        End Function

        Public Sub New(freq As Integer, vectorSize As Integer)
            frequency = freq
            vector = New Double(vectorSize - 1) {}
            parentNeuron = Nothing
        End Sub
    End Class
End Namespace
