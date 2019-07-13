#Region "Microsoft.VisualBasic::b06bba936ea8ea8f174f15fb8d3b466e, Data_science\DataMining\DataMining\test\AprioriTest.vb"

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

    ' Module AprioriTest
    ' 
    '     Function: BuildTransactions
    ' 
    '     Sub: BasicQuickTest, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.AprioriRules
Imports Microsoft.VisualBasic.DataMining.AprioriRules.Entities
Imports Microsoft.VisualBasic.Linq

Module AprioriTest
    Sub Main()
        Call BasicQuickTest()



        Dim data = "D:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\arules\Adult.csv" _
            .ReadAllLines _
            .Skip(1) _
            .Select(Function(s) Regex.Replace(s, ",""\d+""", "").Trim(""""c).Trim("{"c, "}"c)) _
            .ToArray

        Dim datasets = data.Select(Function(s, i) New EntityObject With {.ID = i, .Properties = s.Split(","c).Select(Function(t) t.GetTagValue("=")).ToDictionary(Function(n) n.Name, Function(v) v.Value)}).ToArray
        Dim transactions = datasets.BuildTransactions.ToArray
        Dim encoder As New Encoding(transactions.AllItems)
        Dim list = encoder.TransactionEncoding(transactions).ToArray



        Pause()
    End Sub

    Sub BasicQuickTest()
        Dim database$() = {"ACD", "BCE", "ABCE", "BE"}
        Dim result As Output = database.AnalysisTransactions(minSupport:=0, minConfidence:=60)

        Call result.ToString.SaveTo("./test.html")
        Call result.StrongRules.Where(Function(r) r.SupportX >= 2).SaveTo("./rules.csv")

        Pause()
    End Sub


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
End Module
