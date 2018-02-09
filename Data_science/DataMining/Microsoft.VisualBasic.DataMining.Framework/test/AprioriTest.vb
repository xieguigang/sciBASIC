Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Data.csv.IO

Module AprioriTest
    Sub Main()
        Dim data = "D:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\arules\Adult.csv" _
            .ReadAllLines _
            .Skip(1) _
            .Select(Function(s) Regex.Replace(s, ",""\d+""", "").Trim(""""c).Trim("{"c, "}"c)) _
            .ToArray

        Dim datasets = data.Select(Function(s, i) New EntityObject With {.ID = i, .Properties = s.Split(","c).Select(Function(t) t.GetTagValue("=")).ToDictionary(Function(n) n.Name, Function(v) v.Value)}).ToArray

        Pause()
    End Sub
End Module
