#Region "Microsoft.VisualBasic::e70ce8b6aaf07441886e4dbc2bacfb01, ..\sciBASIC#\Data_science\Microsoft.VisualBasic.DataMining.Framework\AprioriAlgorithm\AlgorithmInvoker.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Collections.Generic
Imports System.Linq
Imports System.Collections.ObjectModel
Imports System.Windows.Input
Imports System.ComponentModel.DataAnnotations
Imports Microsoft.VisualBasic.DataMining.AprioriAlgorithm.Entities
Imports Microsoft.VisualBasic.Linq

Namespace AprioriAlgorithm

    Public Class AlgorithmInvoker

        Private ReadOnly _aprioriWorkerAnalysis As Apriori.AprioriPredictions

        Public Sub New(apriori As Apriori.AprioriPredictions)
            Me._aprioriWorkerAnalysis = apriori
        End Sub

        Public Function AnalysisTransactions(Transactions As IEnumerable(Of String),
                                             Items As Char(),
                                             Optional MinSupport As Double = 1,
                                             Optional MinConfidence As Double = 1) As Output
            Dim items__1 As IEnumerable(Of String) = (From ch In Items Select str = ch.ToString).ToArray
            Dim output As Output = _aprioriWorkerAnalysis(MinSupport / 100, MinConfidence / 100, items__1, Transactions.ToArray())
            Return output
        End Function

        Public Function AnalysisTransactions(Transactions As IEnumerable(Of String),
                                             Optional MinSupport As Double = 1,
                                             Optional MinConfidence As Double = 1) As Output
            Dim source = (From Transaction As String In Transactions Select Transaction.ToArray).IteratesALL
            Dim items__1 As IEnumerable(Of String) = (From ch In source Select str = ch.ToString Distinct).ToArray
            Dim output As Output = _aprioriWorkerAnalysis(MinSupport / 100, MinConfidence / 100, items__1, Transactions.ToArray())
            Return output
        End Function

        Public Shared Function CreateObject() As AlgorithmInvoker
            Return New AlgorithmInvoker(apriori:=AddressOf Apriori.InvokeAnalysis)
        End Function

        Public Overrides Function ToString() As String
            Return _aprioriWorkerAnalysis.ToString
        End Function
    End Class
End Namespace
