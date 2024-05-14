#Region "Microsoft.VisualBasic::cab37472d5a4e1a54e78184a488ddad0, Data_science\MachineLearning\Bootstrapping\GraphEmbedding\struct\Triple.vb"

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

    '   Total Lines: 26
    '    Code Lines: 15
    ' Comment Lines: 6
    '   Blank Lines: 5
    '     File Size: 800 B


    '     Structure Triple
    ' 
    '         Properties: head, relation, tail
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace GraphEmbedding.struct

    Public Structure Triple

        Public ReadOnly Property head() As Integer
        Public ReadOnly Property tail() As Integer
        Public ReadOnly Property relation() As Integer

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="head"></param>
        ''' <param name="tail"></param>
        ''' <param name="relation"></param>
        Public Sub New(head As Integer, tail As Integer, relation As Integer)
            Me.head = head
            Me.tail = tail
            Me.relation = relation
        End Sub

        Public Overrides Function ToString() As String
            Return $"[head:{head()} -> tail:{tail()}] relationship:{relation()}"
        End Function
    End Structure

End Namespace
