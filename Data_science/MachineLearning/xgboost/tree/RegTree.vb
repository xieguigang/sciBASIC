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
        ''' <paramname="reader"> input stream </param>
        ''' <exceptioncref="IOException"> If an I/O error occurs </exception>
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
        ''' <paramname="feat">    feature vector </param>
        ''' <paramname="root_id"> starting root index </param>
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
        ''' <paramname="feat">    feature vector </param>
        ''' <paramname="root_id"> starting root index </param>
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

        <Serializable>
        Friend Class Node
            ' pointer to parent, highest bit is used to
            ' indicate whether it's a left child or not
            Friend ReadOnly parent_ As Integer
            ' pointer to left, right
            Friend ReadOnly cleft_, cright_ As Integer
            ' split feature index, left split or right split depends on the highest bit
            Friend ReadOnly sindex_ As Integer
            ' extra info (leaf_value or split_cond)
            Friend ReadOnly leaf_value As Double
            Friend ReadOnly split_cond As Double
            Friend ReadOnly _defaultNext As Integer
            Friend ReadOnly _splitIndex As Integer
            Friend ReadOnly _isLeaf As Boolean

            ''' <summary>
            ''' set parent
            ''' </summary>
            ''' <param name="reader"></param>
            Friend Sub New(reader As ModelReader)
                parent_ = reader.readInt()
                cleft_ = reader.readInt()
                cright_ = reader.readInt()
                sindex_ = reader.readInt()

                If is_leaf() Then
                    leaf_value = reader.readFloat()
                    split_cond = Double.NaN
                Else
                    split_cond = reader.readFloat()
                    leaf_value = Double.NaN
                End If

                _defaultNext = cdefault()
                _splitIndex = split_index()
                _isLeaf = is_leaf()
            End Sub

            Friend Overridable Function is_leaf() As Boolean
                Return cleft_ = -1
            End Function

            Friend Overridable Function split_index() As Integer
                Return sindex_ And (1L << 31) - 1L
            End Function

            Friend Overridable Function cdefault() As Integer
                Return If(default_left(), cleft_, cright_)
            End Function

            Friend Overridable Function default_left() As Boolean
                Return CInt(CUInt(sindex_) >> 31) <> 0
            End Function

            Friend Overridable Function [next](feat As FVec) As Integer
                Dim fvalue = feat.fvalue(_splitIndex)

                If fvalue <> fvalue Then ' is NaN?
                    Return _defaultNext
                End If

                Return If(fvalue < split_cond, cleft_, cright_)
            End Function
        End Class

        ''' <summary>
        ''' Statistics each node in tree.
        ''' </summary>
        <Serializable>
        Friend Class RTreeNodeStat
            ' ! \brief loss chg caused by current split 
            Friend ReadOnly loss_chg As Single
            ' ! \brief sum of hessian values, used to measure coverage of data 
            Friend ReadOnly sum_hess As Single
            ' ! \brief weight of current node 
            Friend ReadOnly base_weight As Single
            ' ! \brief number of child that is leaf node known up to now 
            Friend ReadOnly leaf_child_cnt As Integer

            Friend Sub New(reader As ModelReader)
                loss_chg = reader.readFloat()
                sum_hess = reader.readFloat()
                base_weight = reader.readFloat()
                leaf_child_cnt = reader.readInt()
            End Sub
        End Class
    End Class
End Namespace
