Module sqltest

    Sub Main()
        Dim names = "C:\Users\Administrator\Desktop\list.txt".ReadAllLines.Select(AddressOf Strings.LCase).ToArray
        Dim queryMeta = $"SELECT * FROM biodeepDB.metadb where lower(`name`) in ({names.Select(Function(a) $"""{a}""").JoinBy(", ")});"


        Call queryMeta.SaveTo("C:\Users\Administrator\Desktop\list_meta.sql")
    End Sub
End Module
