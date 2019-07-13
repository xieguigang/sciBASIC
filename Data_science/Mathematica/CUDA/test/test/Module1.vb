#Region "Microsoft.VisualBasic::e85963fb49b4e27cd84ff84757290198, Data_science\Mathematica\CUDA\test\test\Module1.vb"

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

    ' Module Module1
    ' 
    '     Function: test2
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

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

