Imports System
Imports Microsoft.VisualBasic.DataStorage.FeatherFormat

Module Program
    Sub Main()
        Dim path = IO.Path.ChangeExtension(IO.Path.GetTempFileName(), ".feather")
        Console.WriteLine("temp file: " & path)

        Try
            Using writer As New FeatherWriter(path)
                writer.AddColumn("city", New String() {"北京", "上海", "广州", "深圳"})
                writer.AddColumn("name", New String() {"张三", "李四", Nothing, "赵六"})
                writer.AddColumn("age", New Integer() {20, 30, 40, 50})
                writer.AddColumn("score", New Double() {1.5, 2.5, 3.5, 4.5})
            End Using

            Dim expectedCity = {"北京", "上海", "广州", "深圳"}
            Dim expectedName = {"张三", "李四", Nothing, "赵六"}
            Dim expectedAge = {20, 30, 40, 50}
            Dim expectedScore = {1.5, 2.5, 3.5, 4.5}

            Dim df = FeatherReader.ReadFromFile(path)
            Console.WriteLine($"Rows={df.RowCount}, Cols={df.ColumnCount}")

            Dim ok = True
            For i = 0 To df.RowCount - 1
                Dim city = df(i, 0).ToString()
                Dim name = df(i, 1).ToString()
                Dim age = df(i, 2).ToString()
                Dim score = df(i, 3).ToString()

                Console.WriteLine($"row{i}: city='{city}' name='{name}' age={age} score={score}")

                If city <> expectedCity(i) Then ok = False : Console.WriteLine($"  !! city mismatch: got '{city}' expected '{expectedCity(i)}'")
                If expectedName(i) Is Nothing Then
                    If name <> "" Then ok = False : Console.WriteLine($"  !! name null mismatch: got '{name}'")
                Else
                    If name <> expectedName(i) Then ok = False : Console.WriteLine($"  !! name mismatch: got '{name}' expected '{expectedName(i)}'")
                End If
                If age <> CStr(expectedAge(i)) Then ok = False : Console.WriteLine($"  !! age mismatch: got '{age}'")
                If score <> CStr(expectedScore(i)) Then ok = False : Console.WriteLine($"  !! score mismatch: got '{score}'")
            Next

            Console.WriteLine(If(ok, "RESULT: PASS", "RESULT: FAIL"))
        Catch ex As Exception
            Console.WriteLine("ERROR: " & ex.ToString())
        Finally
            If IO.File.Exists(path) Then IO.File.Delete(path)
        End Try
    End Sub
End Module
