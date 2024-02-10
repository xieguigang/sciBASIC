Imports Affinity_Propagation_Clustering.Utility

Namespace AffinityPropagation

    Friend Interface ITestDataSet
        Function DataSet() As Point()
        Function Centers() As Integer()
    End Interface

    Public Class ToyDataset
        Implements ITestDataSet
        Public Function Centers() As Integer() Implements ITestDataSet.Centers
            Return {0, 0, 0, 1, 1, 1}
        End Function

        Public Function DataSet() As Point() Implements ITestDataSet.DataSet
            Return {
                New Point(1, 2),
                New Point(1, 4),
                New Point(1, 0),
                New Point(4, 2),
                New Point(4, 4),
                New Point(4, 0)
            }
        End Function
    End Class

End Namespace
