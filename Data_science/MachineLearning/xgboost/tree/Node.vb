Imports Microsoft.VisualBasic.MachineLearning.XGBoost.util

Namespace tree

    <Serializable> Friend Class Node
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

End Namespace