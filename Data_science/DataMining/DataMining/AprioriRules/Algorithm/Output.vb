#Region "Microsoft.VisualBasic::bfd4ac4e8043e96b93b6f99ddd9a197f, G:/GCModeller/src/runtime/sciBASIC#/Data_science/DataMining/DataMining//AprioriRules/Algorithm/Entities/Output.vb"

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

    '   Total Lines: 48
    '    Code Lines: 38
    ' Comment Lines: 4
    '   Blank Lines: 6
    '     File Size: 1.84 KB


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
        Public Property MaximalItemSets() As List(Of ItemSet)
        Public Property ClosedItemSets() As Dictionary(Of ItemSet, Dictionary(Of ItemSet, Double))
        Public Property FrequentItems() As Dictionary(Of ItemSet, TransactionTokensItem)
        Public Property TransactionSize As Integer

        Public Overrides Function ToString() As String
            Dim html As New StringBuilder()
            Dim rules = StrongRules _
                .Select(Function(rule)
                            Return <tr>
                                       <td><%= $"{rule.X} => {rule.Y}" %></td>
                                       <td><%= rule.SupportXY %></td>
                                       <td><%= rule.SupportX %></td>
                                       <td><%= rule.SupportXY / TransactionSize %></td>
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
                        <th>Support</th>
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
