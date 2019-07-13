#Region "Microsoft.VisualBasic::364af627922019fa5b8ee65261ef3093, tutorials\core.test\Module1.vb"

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
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Main, matchTest, takesTest
    '     Structure Foo
    ' 
    '         Operators: (+2 Overloads) Like
    ' 
    '     Structure Lazy
    ' 
    '         Function: LongLoad
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#Region "Microsoft.VisualBasic::61a7d586cdb61bfcb962e1aae4ec475f, core.test"

' Author:
' 
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 


' Source file summaries:

' Module Module1
' 
'     Sub: Main, New
' 
'     Structure Foo
' 
'         Operators: (+2 Overloads) Like
' 
' 
' 
'     Structure Lazy
' 
'         Function: LongLoad
' 
' 
' 
' 

#End Region

#Region "Microsoft.VisualBasic::12b87139cec3a6fada99e7fcd6855f3d, core.test"

' Author:
' 
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 


' Source file summaries:

' Module Module1
' 
'     Sub: Main, New
' 
' 
'     Structure Foo
' 
'         Operators: (+2 Overloads) Like
' 
' 
' 
' 
'     Structure Lazy
' 
'         Function: LongLoad
' 
' 
' 
' 
' 

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Language

Module Module1

    Public Structure Foo

        Dim s$

        Public Shared Operator Like(f As Foo, s$) As Double
            Return Levenshtein.ComputeDistance(f.s, s).MatchSimilarity
        End Operator
    End Structure

    Sub matchTest()
        Dim code = "<CLI(""AAAAAAAAA"", 44, TRUE)> Public Class testCLASS"
        Dim s = Regex.Match(code, "<.+?>\s*", RegexICSng).Value

        s = Regex.Replace(code, "<.+?>", "", RegexICSng)

        Pause()

    End Sub

    Sub New()

        ' Call matchTest()


        ' Dim xx As String = "" Or DefaultValue("hello world!") + Function() False


        '  Console.WriteLine(xx)

        '  Pause()

        'Dim test As Boolean

        '' If test Then Call method()
        '' Or
        'If test Then
        '    Call method()
        'End If

        'Dim i%

        'If i = 10 Then
        '    Call method()
        'End If

        'Dim test2 = Function(str$) As Boolean
        '                ' blabla
        '            End Function

        'If test2(str:="blabla") Then
        '    Call method()
        'End If

        'If Not test2(str:="blabla") Then
        '    Call Sub()
        '             ' blabla
        '         End Sub
        'End If


        '        Dim test As Boolean

        '        Call test ? method()

        '  Dim i%

        '        Call (i = 10) ? method()

        'Dim test2 = Function(str$) As Boolean
        '                ' blabla
        '            End Function

        '        Call test2(str:="blablabla") ? method()

        'Call (Not test2(str:="blabla")) ? Sub()
        '                                      ' balbala
        '                                  End Sub



    End Sub

    Sub Main()

        Call takesTest()

        Call New Lazy().show()


        Pause()
    End Sub


    Sub takesTest()
        Dim seq = "abcdefghijklmnopqrstuvwxyz"
        Dim subsss = seq.Takes({0, 3, 2, 1, 9, 13, 12}, offSet:=1)

        Call subsss.CharString.__DEBUG_ECHO

        Pause()
    End Sub

    Public Structure Lazy

        Public Async Sub show()

            Console.WriteLine(Await Load())
            Console.WriteLine("c")
        End Sub

        Public Async Function Load() As Task(Of String)
            Return 1234 + Await LongLoad()
        End Function

        Private Function LongLoad() As Task(Of Integer)
            Dim t As New Task(Of Integer)(Function()
                                              Console.WriteLine("a")
                                              Threading.Thread.Sleep(10000)
                                              Console.WriteLine("b")
                                              Return 100
                                          End Function)
            t.Start()

            Return t
        End Function

    End Structure

End Module
