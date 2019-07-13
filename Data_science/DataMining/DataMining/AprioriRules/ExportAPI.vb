#Region "Microsoft.VisualBasic::537e2ceb6350f84164a1fafc67df66bf, Data_science\DataMining\DataMining\AprioriRules\ExportAPI.vb"

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

    '     Delegate Function
    ' 
    ' 
    '     Module AprioriExport
    ' 
    '         Function: (+3 Overloads) AnalysisTransactions
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.AprioriRules.Entities
Imports Microsoft.VisualBasic.DataMining.AprioriRules.Impl
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq

Namespace AprioriRules

    Public Delegate Function AprioriPredictions(minSupport#, minConfidence#, items As IEnumerable(Of String), transactions$()) As Output

    ''' <summary>
    ''' ``AprioriRules`` API export module
    ''' </summary>
    Public Module AprioriExport

        ReadOnly aprioriDefaultWorker As [Default](Of  AprioriPredictions) = New AprioriPredictions(AddressOf Apriori.GetAssociateRules)

        <Extension>
        Public Function AnalysisTransactions(transactions As IEnumerable(Of String),
                                             items As Char(),
                                             Optional minSupport# = 1,
                                             Optional minConfidence# = 1,
                                             Optional impl As AprioriPredictions = Nothing) As Output

            Dim itemList = (From ch In items Select str = ch.ToString Distinct).ToArray
            Dim output As Output = (impl Or aprioriDefaultWorker)(
                minSupport:=minSupport / 100,
                minConfidence:=minConfidence / 100,
                items:=itemList,
                transactions:=transactions.ToArray()
            )
            Return output
        End Function

        <Extension>
        Public Function AnalysisTransactions(transactions As IEnumerable(Of String),
                                             Optional minSupport# = 1,
                                             Optional minConfidence# = 1,
                                             Optional impl As AprioriPredictions = Nothing) As Output

            Dim data$() = transactions.ToArray
            Dim items = (From transaction In data Select transaction.AsEnumerable) _
                .IteratesALL _
                .Distinct _
                .Select(Function(c) CStr(c)) _
                .ToArray
            Dim output As Output = (impl Or aprioriDefaultWorker)(minSupport / 100, minConfidence / 100, items, transactions:=data)
            Return output
        End Function

        <Extension>
        Public Function AnalysisTransactions(transactions As IEnumerable(Of Transaction),
                                             Optional minSupport# = 1,
                                             Optional minConfidence# = 1,
                                             Optional impl As AprioriPredictions = Nothing) As (encoding As Encoding, rules As Output)
            With transactions.ToArray
                Dim encoding As New Encoding(.AllItems)
                Dim out = encoding _
                    .TransactionEncoding(.ByRef) _
                    .AnalysisTransactions(
                        items:=encoding.AllCodes,
                        minSupport:=minSupport,
                        minConfidence:=minConfidence,
                        impl:=impl
                    )

                Return (encoding, out)
            End With
        End Function
    End Module
End Namespace
