Imports System
Imports System.Linq
Imports Microsoft.VisualBasic.DataStorage.FeatherFormat

Module Program
    Function ReadStringCell(df As DataFrame, r As Long, c As Long) As String
        ' null nullable string comes back as Nothing; non-null as the string
        Return CStr(df(r, c))
    End Function

    Function RunScenario(path As String, cities() As String, names() As String, ages() As Integer, scores() As Double) As Boolean
        Using df As DataFrame = FeatherReader.ReadFromFile(path)
            If df.RowCount <> cities.Length OrElse df.ColumnCount <> 4 Then
                Console.WriteLine($"  !! shape mismatch rows={df.RowCount} cols={df.ColumnCount}")
                Return False
            End If
            Dim ok = True
            For i = 0 To cities.Length - 1
                Dim city = ReadStringCell(df, i, 0)
                Dim name = ReadStringCell(df, i, 1)
                Dim age = CInt(df(i, 2))
                Dim score = CDbl(df(i, 3))

                If city <> cities(i) Then ok = False : Console.WriteLine($"  row{i} city: got '{city}' exp '{cities(i)}'")
                If If(name, "") <> If(names(i), "") Then ok = False : Console.WriteLine($"  row{i} name: got '{name}' exp '{names(i)}'")
                If age <> ages(i) Then ok = False : Console.WriteLine($"  row{i} age: got {age} exp {ages(i)}")
                If score <> scores(i) Then ok = False : Console.WriteLine($"  row{i} score: got {score} exp {scores(i)}")
            Next
            Return ok
        End Using
    End Function

    Sub Main()
        Dim allOk = True

        ' Scenario A: String() arrays (non-null + nullable mixed)
        Dim pathA = IO.Path.ChangeExtension(IO.Path.GetTempFileName(), ".feather")
        Using w As New FeatherWriter(pathA)
            w.AddColumn("city", New String() {"北京", "上海", "广州", "深圳"})
            w.AddColumn("name", New String() {"张三", Nothing, "李四", "赵六"})
            w.AddColumn("age", New Integer() {20, 30, 40, 50})
            w.AddColumn("score", New Double() {1.5, 2.5, 3.5, 4.5})
        End Using
        Dim aOk = RunScenario(pathA, {"北京", "上海", "广州", "深圳"}, {"张三", Nothing, "李四", "赵六"}, {20, 30, 40, 50}, {1.5, 2.5, 3.5, 4.5})
        Console.WriteLine("Scenario A (String() array): " & If(aOk, "PASS", "FAIL"))
        allOk = allOk And aOk
        If IO.File.Exists(pathA) Then IO.File.Delete(pathA)

        ' Scenario B: List(Of String) + List(Of Integer)
        Dim pathB = IO.Path.ChangeExtension(IO.Path.GetTempFileName(), ".feather")
        Using w As New FeatherWriter(pathB)
            w.AddColumn("city", New List(Of String)({"南京", "武汉", "成都", "杭州"}))
            w.AddColumn("age", New List(Of Integer)({1, 2, 3, 4}))
        End Using
        Dim bOk = RunScenario(pathB, {"南京", "武汉", "成都", "杭州"}, {Nothing, Nothing, Nothing, Nothing}, {1, 2, 3, 4}, {0, 0, 0, 0})
        Console.WriteLine("Scenario B (List(Of String)): " & If(bOk, "PASS", "FAIL"))
        allOk = allOk And bOk
        If IO.File.Exists(pathB) Then IO.File.Delete(pathB)

        ' Scenario C: LINQ IEnumerable(Of String)
        Dim pathC = IO.Path.ChangeExtension(IO.Path.GetTempFileName(), ".feather")
        Using w As New FeatherWriter(pathC)
            w.AddColumn("city", (From s In {"重庆", "天津", "苏州", "青岛"} Select s).ToArray().AsEnumerable())
            w.AddColumn("age", {5, 6, 7, 8})
        End Using
        Dim cOk = RunScenario(pathC, {"重庆", "天津", "苏州", "青岛"}, {Nothing, Nothing, Nothing, Nothing}, {5, 6, 7, 8}, {0, 0, 0, 0})
        Console.WriteLine("Scenario C (IEnumerable LINQ): " & If(cOk, "PASS", "FAIL"))
        allOk = allOk And cOk
        If IO.File.Exists(pathC) Then IO.File.Delete(pathC)

        ' Scenario D: large row count, two Chinese string columns interleaved with int
        Dim n = 5000
        Dim citiesD(n - 1) As String
        Dim namesD(n - 1) As String
        Dim agesD(n - 1) As Integer
        Dim scoresD(n - 1) As Double
        For i = 0 To n - 1
            citiesD(i) = "城市" & i.ToString()
            namesD(i) = If(i Mod 7 = 0, Nothing, "姓名" & i.ToString())
            agesD(i) = i
            scoresD(i) = i * 1.1
        Next
        Dim pathD = IO.Path.ChangeExtension(IO.Path.GetTempFileName(), ".feather")
        Using w As New FeatherWriter(pathD)
            w.AddColumn("city", citiesD)
            w.AddColumn("name", namesD)
            w.AddColumn("age", agesD)
            w.AddColumn("score", scoresD)
        End Using
        Dim dOk = RunScenario(pathD, citiesD, namesD, agesD, scoresD)
        Console.WriteLine("Scenario D (5000 rows, 2 cn strings): " & If(dOk, "PASS", "FAIL"))
        allOk = allOk And dOk
        If IO.File.Exists(pathD) Then IO.File.Delete(pathD)

        Console.WriteLine(If(allOk, "ALL PASS", "SOME FAIL"))
    End Sub
End Module
