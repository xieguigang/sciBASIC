#Region "Microsoft.VisualBasic::826c53a6dd199b6ce3c0559a7f335d44, ..\sciBASIC#\Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\AprioriAlgorithm\Algorithm\Entities\Output.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2018 GPL3 Licensed
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

Imports System.Text
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Text

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
                .Select(AddressOf InputHandler.ToString) _
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

            Return sprintf(<table>%s</table>.ToString, html)
        End Function
    End Class
End Namespace
