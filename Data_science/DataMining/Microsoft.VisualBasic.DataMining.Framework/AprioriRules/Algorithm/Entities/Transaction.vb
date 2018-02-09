Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq

Namespace AprioriRules.Entities

    Public Structure Transaction

        Public Property Name As String
        Public Property Items As String()

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
        Public Function BuildTransactions(data As IEnumerable(Of EntityObject)) As IEnumerable(Of Transaction)
            Return data _
                .SafeQuery _
                .Select(Function(t)
                            Return New Transaction With {
                                .Name = t.ID,
                                .Items = t.Properties.Values.ToArray
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