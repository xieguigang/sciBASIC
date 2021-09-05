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
