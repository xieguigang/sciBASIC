Imports Microsoft.VisualBasic.MachineLearning.XGBoost.util

Namespace tree

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
End Namespace