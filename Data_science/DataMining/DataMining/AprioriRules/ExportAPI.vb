#Region "Microsoft.VisualBasic::537e2ceb6350f84164a1fafc67df66bf, G:/GCModeller/src/runtime/sciBASIC#/Data_science/DataMining/DataMining//AprioriRules/ExportAPI.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:


' Code Statistics:

'   Total Lines: 72
'    Code Lines: 59
' Comment Lines: 3
'   Blank Lines: 10
'     File Size: 3.31 KB


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

        ReadOnly aprioriDefaultWorker As [Default](Of AprioriPredictions) = New AprioriPredictions(AddressOf Apriori.GetAssociateRules)

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
                                             Optional impl As AprioriPredictions = Nothing) As Output
            With transactions.ToArray
                Dim encoding As New Encoding(.AllItems)
                Dim out As Output = encoding _
                    .TransactionEncoding(.ByRef) _
                    .AnalysisTransactions(
                        items:=encoding.AllCodes,
                        minSupport:=minSupport,
                        minConfidence:=minConfidence,
                        impl:=impl
                    )

                out.MaximalItemSets = out.MaximalItemSets.Select(Function(s) encoding.Decode)

                Return out
            End With
        End Function


    End Module
End Namespace
