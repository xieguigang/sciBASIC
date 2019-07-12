Imports System.Runtime.InteropServices

Module Module1

    Declare Function dllMain Lib "Dll1.dll" Alias "main" () As Integer

    Declare Function loopTest Lib "Dll1.dll" Alias "loop_test" () As Integer


    Sub Main()
        Dim i As Integer = dllMain
        Dim start = App.NanoTime


        Dim j1 = loopTest
        Dim b1 = App.NanoTime - start

        start = App.NanoTime

        Dim j2 = test2()

        Dim b2 = App.NanoTime - start

        Console.WriteLine(b1)
        Console.WriteLine(b2)

        Console.Read()
    End Sub

    Function test2() As Integer
        Dim j% = 1

        For i As Integer = 0 To 10000000
            j = (i + 5) / j
        Next

        Return j
    End Function
End Module
