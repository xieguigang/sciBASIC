#Region "Microsoft.VisualBasic::2b03a97c1aecfce54928047a206fa71d, Data_science\Graph\Model\Tree\HuffmanTree\HuffmanNeuron.vb"

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

    '   Total Lines: 41
    '    Code Lines: 32
    ' Comment Lines: 3
    '   Blank Lines: 6
    '     File Size: 1.49 KB


    '     Class HuffmanNeuron
    ' 
    '         Properties: code, frequency, parentNeuron, vector
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: compareTo, merge, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace HuffmanTree

    ''' <summary>
    ''' Created by fangy on 13-12-20.
    ''' </summary>
    Public Class HuffmanNeuron : Implements HuffmanNode

        Public Property parentNeuron As HuffmanNode Implements HuffmanNode.parent
        Public Property code As Integer = 0 Implements HuffmanNode.code
        Public Property vector As Double()
        Public Property frequency As Integer Implements HuffmanNode.frequency

        Public Sub New(freq As Integer, vectorSize As Integer)
            frequency = freq
            vector = New Double(vectorSize - 1) {}
        End Sub

        Public Function merge(right As HuffmanNode) As HuffmanNode Implements HuffmanNode.merge
            Dim parent As HuffmanNode = New HuffmanNeuron(frequency + right.frequency, vector.Length)
            parentNeuron = parent
            code = (0)
            right.parent = parent
            right.code = (1)
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

        Public Overrides Function ToString() As String
            Return $"frequency: {frequency}"
        End Function
    End Class
End Namespace
