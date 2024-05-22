#Region "Microsoft.VisualBasic::769542c7d83ce1b07da3402a62154671, Data_science\DataMining\DataMining\AprioriRules\Algorithm\Transaction.vb"

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

    '   Total Lines: 69
    '    Code Lines: 44 (63.77%)
    ' Comment Lines: 15 (21.74%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (14.49%)
    '     File Size: 2.31 KB


    '     Structure Transaction
    ' 
    '         Properties: Items, Name
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Module TransactionExtensions
    ' 
    '         Function: AllItems, BuildTransactions
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

Namespace AprioriRules.Entities

    ''' <summary>
    ''' a transaction record
    ''' </summary>
    Public Structure Transaction

        ''' <summary>
        ''' the transaction unique id
        ''' </summary>
        ''' <returns></returns>
        Public Property Name As String
        ''' <summary>
        ''' the item collection that contains inside current transaction.
        ''' </summary>
        ''' <returns></returns>
        Public Property Items As String()

        Sub New(name As String, items As IEnumerable(Of String))
            Me.Name = name
            Me.Items = items.ToArray
        End Sub

        Sub New(name As String, items As String)
            Me.Name = name
            Me.Items = items.Select(Function(c) c.ToString).ToArray
        End Sub

        ''' <summary>
        ''' use the char as transaction items, just used for debug test
        ''' </summary>
        ''' <param name="data"></param>
        Sub New(data As String)
            Call Me.New(data, data)
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Name} = {{ {Items.JoinBy(", ")} }}"
        End Function
    End Structure

    Public Module TransactionExtensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function BuildTransactions(data As IEnumerable(Of NamedValue(Of String()))) As IEnumerable(Of Transaction)
            Return data _
                .SafeQuery _
                .Select(Function(t)
                            Return New Transaction With {
                                .Name = t.Name,
                                .Items = t.Value
                            }
                        End Function)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AllItems(transactions As IEnumerable(Of Transaction)) As IEnumerable(Of String)
            Return transactions _
                .Select(Function(t) t.Items) _
                .IteratesALL
        End Function
    End Module
End Namespace
