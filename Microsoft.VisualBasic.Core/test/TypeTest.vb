Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Module TypeTest

    Sub Main()


        Call test()

        Dim o As UnionType(Of String, Integer(), Char())

        o = "string"

        Console.WriteLine(CStr(o))
        Console.WriteLine(o Like GetType(String))
        Console.WriteLine(o Like GetType(Integer()))
        Console.WriteLine(o Like GetType(Char()))

        o = {1, 2, 1231, 31, 23, 12}
        Console.WriteLine(CType(o, Integer()).GetJson)
        Console.WriteLine(o Like GetType(String))
        Console.WriteLine(o Like GetType(Integer()))
        Console.WriteLine(o Like GetType(Char()))

        o = {"f"c, "s"c, "d"c, "f"c, "s"c, "d"c, "f"c, "s"c, "d"c, "f"c, "s"c, "d"c, "f"c}
        Console.WriteLine(CType(o, Char()).GetJson)
        Console.WriteLine(o Like GetType(String))
        Console.WriteLine(o Like GetType(Integer()))
        Console.WriteLine(o Like GetType(Char()))

        Pause()
    End Sub

    Sub test()

        Dim chars As UnionType(Of Char(), Integer())

        chars = CharArray("Hello world!", False)

        Console.WriteLine(CType(chars, Char()).GetJson)
        Console.WriteLine(chars Like GetType(Integer()))
        Console.WriteLine(chars Like GetType(Char()))

        chars = CharArray("Hello world!", True)

        Console.WriteLine(CType(chars, Integer()).GetJson)
        Console.WriteLine(chars Like GetType(Integer()))
        Console.WriteLine(chars Like GetType(Char()))

        Pause()
    End Sub

    Public Function CharArray(s As String, ascii As Boolean) As UnionType(Of Char(), Integer())
        If ascii Then
            Return s.Select(Function(c) AscW(c)).ToArray
        Else
            Return s.ToArray
        End If
    End Function
End Module
