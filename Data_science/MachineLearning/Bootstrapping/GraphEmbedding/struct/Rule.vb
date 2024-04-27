#Region "Microsoft.VisualBasic::428ae1946ec7dd47da63f33dc1c908d4, G:/GCModeller/src/runtime/sciBASIC#/Data_science/MachineLearning/Bootstrapping//GraphEmbedding/struct/Rule.vb"

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

    '   Total Lines: 22
    '    Code Lines: 15
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 517 B


    '     Class Rule
    ' 
    '         Properties: confidence
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: relations
    ' 
    '         Sub: add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace GraphEmbedding.struct

    Public Class Rule

        ReadOnly m_relations As List(Of Relation)

        Public Property confidence() As Double

        Public Sub New()
            m_relations = New List(Of Relation)()
        End Sub

        Public Overridable Function relations() As List(Of Relation)
            Return m_relations
        End Function

        Public Overridable Sub add(r As Relation)
            m_relations.Add(r)
        End Sub
    End Class

End Namespace

