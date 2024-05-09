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
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq

Namespace AprioriRules

    Public Delegate Function AprioriPredictions(minSupport#, minConfidence#, items As IEnumerable(Of Item), transactions As ItemSet()) As Output

    ''' <summary>
    ''' ``AprioriRules`` API export module
    ''' </summary>
    ''' 
    <HideModuleName>
    Public Module AprioriExport

        ReadOnly aprioriDefaultWorker As [Default](Of AprioriPredictions) = New AprioriPredictions(AddressOf Apriori.GetAssociateRules)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="transactions"></param>
        ''' <param name="items"></param>
        ''' <param name="minSupport">``(0,1]``</param>
        ''' <param name="minConfidence">``(0,1]``</param>
        ''' <param name="minlen">min item count in the generated strong rule</param>
        ''' <param name="impl"></param>
        ''' <returns></returns>
        <Extension>
        Public Function AnalysisTransactions(transactions As IEnumerable(Of ItemSet),
                                             items As Item(),
                                             Optional minSupport# = 0.01,
                                             Optional minConfidence# = 0.01,
                                             Optional minlen As Integer = 2,
                                             Optional impl As AprioriPredictions = Nothing) As Output

            Dim output As Output = (impl Or aprioriDefaultWorker)(
                minSupport:=minSupport,
                minConfidence:=minConfidence,
                items:=items,
                transactions:=transactions.ToArray()
            )

            output.StrongRules = output.StrongRules _
                .Where(Function(r) r.length >= minlen) _
                .OrderByDescending(Function(r) r.Confidence) _
                .AsList

            Return output
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="transactions"></param>
        ''' <param name="minSupport#">``(0,1]``</param>
        ''' <param name="minConfidence">``(0,1]``</param>
        ''' <param name="impl"></param>
        ''' <param name="minlen">min item count in the generated strong rule</param>
        ''' <returns></returns>
        <Extension>
        Public Function AnalysisTransactions(transactions As IEnumerable(Of Transaction),
                                             Optional minSupport# = 0.01,
                                             Optional minConfidence# = 0.01,
                                             Optional minlen As Integer = 2,
                                             Optional impl As AprioriPredictions = Nothing) As Output

            Dim trans_pool As Transaction() = transactions.ToArray
            Dim encoding As New Encoding(trans_pool.AllItems)
            Dim out As Output = encoding _
                .TransactionEncoding(trans_pool) _
                .AnalysisTransactions(
                    items:=encoding.AllItems,
                    minSupport:=minSupport,
                    minConfidence:=minConfidence,
                    impl:=impl,
                    minlen:=minlen
                )

            Return out
        End Function

    End Module
End Namespace
