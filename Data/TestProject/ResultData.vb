#Region "Microsoft.VisualBasic::4a4268fea954902a51a20cf1410c07b1, Data\TestProject\ResultData.vb"

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

    '     Class DESeq
    ' 
    '         Properties: eeeeeee
    '         Class ResultData
    ' 
    '             Properties: [enum], array, attr, attr2, baseMean
    '                         ExperimentValues, Gene, lfcSE, list, log2FoldChange
    '                         padj, pvalue, stat
    ' 
    '             Function: ToString
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic


Namespace RTools

    Public Class DESeq

        Public Property eeeeeee As ResultData()

        Public Class ResultData

            Public Property [enum] As Microsoft.VisualBasic.AppWinStyle
            Public Property Gene As String
            Public Property baseMean As Double
            Public Property log2FoldChange As Double
            Public Property lfcSE As Double
            Public Property stat As Double
            Public Property pvalue As Double
            Public Property padj As Double
            Public Property attr As KeyValuePair(Of String, Double)
            Public Property attr2 As Microsoft.VisualBasic.ComponentModel.Collection.Generic.KeyValuePairObject(Of String, Boolean)
            Public Property array As Double()
            Public Property list As List(Of Long)

            <MetaAttribute(GetType(Double))>
            Public Property ExperimentValues As Dictionary(Of String, Double)

            Public Overrides Function ToString() As String
                Return $"{Gene} ---> log2FoldChange  {log2FoldChange}"
            End Function
        End Class
    End Class
End Namespace
