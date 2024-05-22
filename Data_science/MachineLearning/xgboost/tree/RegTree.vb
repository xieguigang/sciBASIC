#Region "Microsoft.VisualBasic::b40fb8dd20fd391aa9f932d3c7900caf, Data_science\MachineLearning\xgboost\tree\RegTree.vb"

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

    '   Total Lines: 105
    '    Code Lines: 56 (53.33%)
    ' Comment Lines: 33 (31.43%)
    '    - Xml Docs: 69.70%
    ' 
    '   Blank Lines: 16 (15.24%)
    '     File Size: 3.73 KB


    '     Class RegTree
    ' 
    '         Function: getLeafIndex, getLeafValue
    ' 
    '         Sub: loadModel
    '         Class Param
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.util

Namespace tree

    ''' <summary>
    ''' Regression tree.
    ''' </summary>
    <Serializable>
    Public Class RegTree

        Private paramField As Param
        Private nodes As Node()
        Private stats As RTreeNodeStat()

        ''' <summary>
        ''' Loads model from stream.
        ''' </summary>
        ''' <param name="reader"> input stream </param>
        ''' <exception cref="IOException"> If an I/O error occurs </exception>
        Public Overridable Sub loadModel(reader As ModelReader)
            paramField = New Param(reader)
            nodes = New Node(paramField.num_nodes - 1) {}

            For i = 0 To paramField.num_nodes - 1
                nodes(i) = New Node(reader)
            Next

            stats = New RTreeNodeStat(paramField.num_nodes - 1) {}

            For i = 0 To paramField.num_nodes - 1
                stats(i) = New RTreeNodeStat(reader)
            Next
        End Sub

        ''' <summary>
        ''' Retrieves nodes from root to leaf and returns leaf index.
        ''' </summary>
        ''' <param name="feat">    feature vector </param>
        ''' <param name="root_id"> starting root index </param>
        ''' <returns> leaf index </returns>
        Public Overridable Function getLeafIndex(feat As FVec, root_id As Integer) As Integer
            Dim pid = root_id
            Dim n As New Value(Of Node)

            While Not (n = nodes(pid))._isLeaf
                pid = n.Value.next(feat)
            End While

            Return pid
        End Function

        ''' <summary>
        ''' Retrieves nodes from root to leaf and returns leaf value.
        ''' </summary>
        ''' <param name="feat">    feature vector </param>
        ''' <param name="root_id"> starting root index </param>
        ''' <returns> leaf value </returns>
        Public Overridable Function getLeafValue(feat As FVec, root_id As Integer) As Double
            Dim n = nodes(root_id)

            While Not n._isLeaf
                n = nodes(n.next(feat))
            End While

            Return n.leaf_value
        End Function

        ''' <summary>
        ''' Parameters.
        ''' </summary>
        <Serializable>
        Friend Class Param
            ' ! \brief number of start root 
            Friend ReadOnly num_roots As Integer
            ' ! \brief total number of nodes 
            Friend ReadOnly num_nodes As Integer
            ' !\brief number of deleted nodes 
            Friend ReadOnly num_deleted As Integer
            ' ! \brief maximum depth, this is a statistics of the tree 
            Friend ReadOnly max_depth As Integer
            ' ! \brief  number of features used for tree construction 
            Friend ReadOnly num_feature As Integer
            ' !
            '  \brief leaf vector size, used for vector tree
            '  used to store more than one dimensional information in tree
            ' 
            Friend ReadOnly size_leaf_vector As Integer
            ' ! \brief reserved part 
            Friend ReadOnly reserved As Integer()

            Friend Sub New(reader As ModelReader)
                num_roots = reader.readInt()
                num_nodes = reader.readInt()
                num_deleted = reader.readInt()
                max_depth = reader.readInt()
                num_feature = reader.readInt()
                size_leaf_vector = reader.readInt()
                reserved = reader.readIntArray(31)
            End Sub
        End Class

    End Class
End Namespace
