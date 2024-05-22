#Region "Microsoft.VisualBasic::d4c050cbac8e64d98698b9981c98df7d, Data_science\DataMining\DataMining\test\AprioriTest.vb"

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

    '   Total Lines: 73
    '    Code Lines: 34 (46.58%)
    ' Comment Lines: 21 (28.77%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 18 (24.66%)
    '     File Size: 2.66 KB


    ' Module AprioriTest
    ' 
    '     Sub: BasicQuickTest, Main, test2
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.DataMining.AprioriRules
Imports Microsoft.VisualBasic.DataMining.AprioriRules.Entities
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Module AprioriTest
    Sub Main()

        Call test2()
        Call BasicQuickTest()



        'Dim data = "D:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\arules\Adult.csv" _
        '    .ReadAllLines _
        '    .Skip(1) _
        '    .Select(Function(s) Regex.Replace(s, ",""\d+""", "").Trim(""""c).Trim("{"c, "}"c)) _
        '    .ToArray

        'Dim datasets = data.Select(Function(s, i) New EntityObject With {.ID = i, .Properties = s.Split(","c).Select(Function(t) t.GetTagValue("=")).ToDictionary(Function(n) n.Name, Function(v) v.Value)}).ToArray
        'Dim transactions = datasets.BuildTransactions.ToArray
        'Dim encoder As New Encoding(transactions.AllItems)
        'Dim list = encoder.TransactionEncoding(transactions).ToArray



        Pause()
    End Sub


    Sub test2()
        Dim trans As Transaction() = {
            New Transaction("ABC"),
             New Transaction("ABD"),
             New Transaction("ACD"),
             New Transaction("ABCE"),
             New Transaction("ACE"),
             New Transaction("BDE"),
             New Transaction("ABCD")
        }

        Dim result = trans.AnalysisTransactions(3 / 7, 5 / 7, minlen:=2)

        Call result.ToString.SaveTo("./test2.html")
        Pause()
    End Sub

    Sub BasicQuickTest()
        Dim database As Transaction() = {New Transaction("ACD", {"A", "C", "D"}), New Transaction("BCE", {"B", "C", "E"}), New Transaction("ABCE", {"A", "B", "C", "E"}), New Transaction("BE", {"B", "E"})}
        Dim result As Output = database.AnalysisTransactions(minSupport:=0, minConfidence:=60)

        Call result.ToString.SaveTo("./test.html")
        Call Console.WriteLine(result.StrongRules.Where(Function(r) r.SupportX >= 2).ToArray.GetJson)

        Pause()
    End Sub


    '<MethodImpl(MethodImplOptions.AggressiveInlining)>
    '<Extension>
    'Public Function BuildTransactions(data As IEnumerable(Of EntityObject)) As IEnumerable(Of Transaction)
    '    Return data _
    '        .SafeQuery _
    '        .Select(Function(t)
    '                    Return New Transaction With {
    '                        .Name = t.ID,
    '                        .Items = t.Properties.Values.ToArray
    '                    }
    '                End Function)
    'End Function
End Module
