#Region "Microsoft.VisualBasic::56d056d2ab346e48034388638d38b322, Data_science\Visualization\data\PCA\PCA\Module1.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module Module1
    ' 
    '     Sub: Main, methodTest, test2
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Module Module1

    Sub Main()
        Call test2()
        Call methodTest()

        Pause()
    End Sub

    Sub test2()
        Dim data = DataSet.LoadDataSet("D:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\PCA\data.csv")
        Dim pca As New PCA(data.Matrix, scale:=True)

        ' Call pca.ExplainedVariance.ToString.__DEBUG_ECHO
        Call Console.WriteLine(pca.Summary)
        Call pca.Project(data.Matrix.Select(Function(v) v.AsVector).ToArray, nPC:=2)

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

        For Each x In pca.Project(newPoints, 2)
            Call x.ToString.__DEBUG_ECHO
        Next

        Pause()
    End Sub
End Module
