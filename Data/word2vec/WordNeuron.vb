#Region "Microsoft.VisualBasic::01cfe022684ce1b8bf09430b16b86c57, Data\word2vec\WordNeuron.vb"

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

    '   Total Lines: 39
    '    Code Lines: 26 (66.67%)
    ' Comment Lines: 4 (10.26%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (23.08%)
    '     File Size: 1.10 KB


    ' Class WordNeuron
    ' 
    '     Properties: name, pathNeurons
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.GraphTheory.HuffmanTree
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

''' <summary>
''' Created by fangy on 13-12-17.
''' 词神经元
''' </summary>
Public Class WordNeuron : Inherits HuffmanNeuron

    Public Property name As String

    Dim m_pathNeurons As IList(Of HuffmanNode)

    Public ReadOnly Property pathNeurons As IList(Of HuffmanNode)
        Get
            If m_pathNeurons IsNot Nothing Then
                Return m_pathNeurons
            End If

            m_pathNeurons = HuffmanTree.getPath(Me)
            Return m_pathNeurons
        End Get
    End Property

    Public Sub New(name As String, freq As Integer, vectorSize As Integer)
        MyBase.New(freq, vectorSize)

        Me.name = name

        For i = 0 To vector.Length - 1
            vector(i) = (randf.seeds.NextDouble - 0.5) / vectorSize
        Next
    End Sub

    Public Overrides Function ToString() As String
        Return $"{name}={frequency}"
    End Function
End Class
