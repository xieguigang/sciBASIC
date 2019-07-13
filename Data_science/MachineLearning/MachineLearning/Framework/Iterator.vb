#Region "Microsoft.VisualBasic::90018dbe4d8f238f7c57407c8ed9b74e, Data_science\MachineLearning\MachineLearning\Framework\Iterator.vb"

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

    '     Class Iterator
    ' 
    '         Sub: Run
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language

Namespace Framework

    Public Class Iterator

        Public Sub Run(Optional iterations% = 10 * 10000)
            Dim i As int = 0

            Do While ++i <= iterations

            Loop
        End Sub
    End Class
End Namespace
