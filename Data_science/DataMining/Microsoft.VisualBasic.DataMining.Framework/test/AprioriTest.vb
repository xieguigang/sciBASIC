Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.AprioriRules
Imports Microsoft.VisualBasic.DataMining.AprioriRules.Entities

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
End Module
