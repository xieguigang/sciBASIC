#Region "Microsoft.VisualBasic::b2b34e0303713b7851580efd2040bab3, core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module TableTest
    ' 
    '     Function: integers
    ' 
    '     Sub: assertTest, main, test_dictionary
    ' 
    ' 

#End Region

#Region "Microsoft.VisualBasic::1191a65b2ff94589cc9e086aa8621b65, core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module TableTest
    ' 
    '     Function: integers
    ' 
    ' 
    '     Sub: assertTest, main, test_dictionary
    ' 
    ' 
    ' 

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Module TableTest

    Sub main()

        Call assertTest()
        Call test_dictionary()

        ' Dim dictionary As New Dictionary(Of Integer, Integer)
        Dim dictionary As BucketDictionary(Of Integer, Integer) = integers.CreateBuckets(Function(i) i, Function(i) i)


        Call VBDebugger.BENCHMARK(Sub() Console.WriteLine(55 = dictionary(55)))

        Console.WriteLine(dictionary.Count)

        Pause()
    End Sub

    Sub assertTest()
        Call Microsoft.VisualBasic.Language.Perl.ExceptionHandler.Default(New NamedValue(Of String))
    End Sub

    Private Sub test_dictionary()
        Dim dictionary As New Dictionary(Of Integer, Integer)

        Try
            For i As Integer = 0 To Integer.MaxValue
                dictionary.Add(i, i)
            Next
        Catch ex As Exception

        End Try

        Pause()
    End Sub

    Public Iterator Function integers() As IEnumerable(Of Integer)
        For i As Integer = 0 To Integer.MaxValue / 10 - 1
            Yield i
        Next
    End Function
End Module

