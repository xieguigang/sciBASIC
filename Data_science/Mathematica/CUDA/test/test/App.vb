Module App

    Declare Function add Lib "math_dll.dll" Alias "add" (a As UShort, b As UShort) As UShort

    Sub Main()
        Call Console.WriteLine($"1+1={add(1, 1)}")
        Call Console.ReadKey()
    End Sub

End Module
