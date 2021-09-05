
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.util
Imports stdNum = System.Math

Namespace gbm

    ''' <summary>
    ''' Gradient boosted DART tree implementation.
    ''' </summary>
    <Serializable>
    Public Class Dart : Inherits GBTree

        Private weightDrop As Single()

        Friend Sub New()
            ' do nothing
        End Sub

        Public Overrides Sub loadModel(reader As ModelReader, with_pbuffer As Boolean)
            MyBase.loadModel(reader, with_pbuffer)

            If mparam.num_trees <> 0 Then
                Dim size As Long = reader.readLong()
                weightDrop = reader.readFloatArray(size)
            End If
        End Sub

        Friend Overrides Function pred(feat As FVec, bst_group As Integer, root_index As Integer, ntree_limit As Integer) As Double
            Dim trees = _groupTrees(bst_group)
            Dim treeleft = If(ntree_limit = 0, trees.Length, stdNum.Min(ntree_limit, trees.Length))
            Dim psum As Double = 0

            For i = 0 To treeleft - 1
                psum += weightDrop(i) * trees(i).getLeafValue(feat, root_index)
            Next

            Return psum
        End Function
    End Class
End Namespace
