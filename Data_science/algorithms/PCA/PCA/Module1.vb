Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Module Module1

    Sub Main()
        Call methodTest()

        Pause()
    End Sub

    ''' <summary>
    ''' https://github.com/mljs/pca
    ''' </summary>
    Sub methodTest()
        Dim data = DataSet.LoadDataSet("C:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\PCA\iris.csv", uidMap:="class")

        Dim pca As New PCA(data.Matrix)

        Call pca.ExplainedVariance.ToString.__DEBUG_ECHO

        Dim newPoints = {{4.9, 3.2, 1.2, 0.4}.AsVector, {5.4, 3.3, 1.4, 0.9}.AsVector}

        For Each x In pca.Project(newPoints)
            Call x.ToString.__DEBUG_ECHO
        Next

        Pause()
    End Sub
End Module
