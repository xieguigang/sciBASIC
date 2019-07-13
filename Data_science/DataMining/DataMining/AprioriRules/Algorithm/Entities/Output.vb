#Region "Microsoft.VisualBasic::bfd4ac4e8043e96b93b6f99ddd9a197f, Data_science\DataMining\DataMining\AprioriRules\Algorithm\Entities\Output.vb"

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

    '     Class Output
    ' 
    '         Properties: ClosedItemSets, FrequentItems, MaximalItemSets, StrongRules
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Xml

Namespace AprioriRules.Entities

    Public Class Output

        ''' <summary>
        ''' The output result for this AprioriRules data mining
        ''' </summary>
        ''' <returns></returns>
        Public Property StrongRules() As List(Of Rule)
        Public Property MaximalItemSets() As List(Of String)
        Public Property ClosedItemSets() As Dictionary(Of String, Dictionary(Of String, Double))
        Public Property FrequentItems() As Dictionary(Of String, TransactionTokensItem)

        Public Overrides Function ToString() As String
            Dim html As New StringBuilder()
            Dim rules = StrongRules _
                .Select(Function(rule)
                            Return <tr>
                                       <td><%= $"{{{rule.X}}} -> {{{rule.Y}}}" %></td>
                                       <td><%= rule.SupportXY %></td>
                                       <td><%= rule.SupportX %></td>
                                       <td><%= rule.Confidence %></td>
                                   </tr>
                        End Function) _
                .Select(Function(r) r.ToString) _
                .JoinBy(ASCII.LF)

            html.AppendLine(
                <thead>
                    <tr>
                        <th>Rules</th>
                        <th>Support(X Y)</th>
                        <th>Support(X)</th>
                        <th>Confidence</th>
                    </tr>
                </thead>)
            html.AppendLine(<tbody>
                                %s
                            </tbody>, rules)

            Return sprintf(<table>%s</table>, html)
        End Function
    End Class
End Namespace
