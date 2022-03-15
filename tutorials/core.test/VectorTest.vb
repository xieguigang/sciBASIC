#Region "Microsoft.VisualBasic::52424e3194b490adddd7e82cf13d3359, sciBASIC#\tutorials\core.test\VectorTest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 269
    '    Code Lines: 117
    ' Comment Lines: 74
    '   Blank Lines: 78
    '     File Size: 6.73 KB


    ' Module VectorTest
    ' 
    '     Sub: linqTest, Main, testssss
    '     Structure fdddd
    ' 
    '         Operators: -, *, \, ^, (+2 Overloads) And
    '                    (+2 Overloads) Like, (+2 Overloads) Mod, (+2 Overloads) Or, (+2 Overloads) Xor
    ' 
    '     Structure str
    ' 
    '         Properties: pattern
    '         Operators: (+2 Overloads) *, +, (+2 Overloads) Like
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#Region "Microsoft.VisualBasic::d9ae45bdb8829d28941c6b8d97ffc2ba, core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module VectorTest
    ' 
    '     Sub: Main
    ' 
    '     Structure fdddd
    ' 
    '         Operators: -, *, \, ^, (+2 Overloads) And
    '                    (+2 Overloads) Like, (+2 Overloads) Mod, (+2 Overloads) Or, (+2 Overloads) Xor
    ' 
    ' 
    '     Structure str
    ' 
    '         Properties: pattern
    ' 
    '         Operators: (+2 Overloads) *, +, (+2 Overloads) Like
    ' 
    ' 
    ' 
    ' 

#End Region

#Region "Microsoft.VisualBasic::760437edec4ce7666647d4a07c89a70b, core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module VectorTest
    ' 
    '     Sub: Main
    ' 
    ' 
    '     Structure fdddd
    ' 
    '         Operators: -, *, \, ^, (+2 Overloads) And
    '                    (+2 Overloads) Like, (+2 Overloads) Mod, (+2 Overloads) Or, (+2 Overloads) Xor
    ' 
    ' 
    ' 
    '     Structure str
    ' 
    '         Properties: pattern
    ' 
    ' 
    '         Operators: (+2 Overloads) *, +, (+2 Overloads) Like
    ' 
    ' 
    ' 
    ' 
    ' 

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml.Models

Module VectorTest

    Structure fdddd
        Public Shared Operator &(a As fdddd, b$) As String

        End Operator

        Public Shared Operator *(a As fdddd, b$) As String

        End Operator
        Public Shared Operator ^(a As fdddd, b$) As String

        End Operator
        Public Shared Operator Mod(a As fdddd, b$) As String

        End Operator
        Public Shared Operator Like(a As fdddd, b$) As String

        End Operator
        Public Shared Operator Xor(a As fdddd, b$) As String

        End Operator
        Public Shared Operator And(a As fdddd, b$) As String

        End Operator
        Public Shared Operator Or(a As fdddd, b$) As String

        End Operator
        Public Shared Operator \(a As fdddd, b$) As String

        End Operator

        Public Shared Operator -(a As fdddd) As Boolean

        End Operator

    End Structure

    Structure str

        Public Property pattern$

        Public Shared Operator Like(str As str, s$) As Boolean
            Return s.IsPattern(str.pattern, RegexICSng)
        End Operator

        Public Shared Operator *(n%, str As str) As str
            Return New str With {.pattern = str.pattern.RepeatString(n)}
        End Operator

        Public Shared Operator *(str As str, n%) As str
            Return New str With {.pattern = str.pattern.RepeatString(n)}
        End Operator

        Public Shared Operator +(a As str, b As str) As str
            Return New str With {.pattern = a.pattern & b.pattern}
        End Operator
    End Structure

    Sub testssss()

        Dim a = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}

        Dim r5 = a.ToArray.Delete(5)
        Dim r0 = a.ToArray.Delete(0)
        Dim rlast = a.ToArray.Delete(a.Length - 1)

        Dim f4 = a.ToArray.Fill(10, 4)
        Dim f0 = a.ToArray.Fill(10, 0)

        Dim i7 = a.ToArray

        Call i7.InsertAt(100, 7)

        Dim i0 = a.ToArray

        Call i0.InsertAt(100, 0)

        Dim ilast = a.ToArray
        Call ilast.InsertAt(100, ilast.Length - 1)

        Pause()
    End Sub

    Sub linqTest()
        Dim pop1 = Iterator Function() As IEnumerable(Of String)
                       Yield "1"
                       Yield "A"
                       Yield "B"
                       Yield "C"
                   End Function
        Dim pop2 = Iterator Function() As IEnumerable(Of String)
                       Yield "D"
                       Yield "E"
                       Yield "F"
                   End Function
        Dim pop3 = Iterator Function() As IEnumerable(Of String)
                       Yield "G"
                       Yield "H"
                   End Function
        Dim pop4 = Iterator Function() As IEnumerable(Of String)
                       Yield "2"
                   End Function
        Dim pop5 = Iterator Function() As IEnumerable(Of String)
                       Yield "X"
                       Yield "Y"
                       Yield "Z"
                   End Function

        Dim source = Iterator Function() As IEnumerable(Of IEnumerable(Of String))

                         Yield pop1()
                         Yield pop2()
                         Yield pop3()
                         Yield pop4()
                         Yield pop5()

                     End Function

        For Each s In source().IteratesALL
            Console.WriteLine(s)
        Next

        Pause()
    End Sub

    Sub Main()


        Call linqTest()

        Call testssss()


        'Dim strings = {"", "sdafa", "sssssss"}.VectorShadows

        'Dim newStrings$() = strings & strings

        'Call newStrings.GetJson.__DEBUG_ECHO


        '        Pause()

        Dim patterns = {
            New str With {.pattern = "\d+"},
            New str With {.pattern = "\s+"},
            New str With {.pattern = "\S+"}
        }.VectorShadows

        ' Dim test2 As str() = patterns * 10

        Dim dddddddddd As str() = patterns + CObj(New str With {.pattern = "44444"})

        Dim index%() = Which.IsTrue(patterns Like "123")
        Dim patternList = patterns.pattern

        Call index.GetJson.__DEBUG_ECHO
        Call CType(patternList, IEnumerable(Of String)).ToArray.GetJson.__DEBUG_ECHO


        Dim vector = {1.0R, 2.0R, 3.0R, 4.0R, 5.0R, 6.0R, 7.0R, 8.0R, 9.0R, 10.0R}.VectorShadows
        Dim list As New List(Of String) From {"A1", "B2", "C3", "D4", "E5", "F6", "G7", "H8", "I9", "J10"}


        Dim sublist As String() = list(vector > 5.0R)

        Call sublist.GetJson.__DEBUG_ECHO

        'Dim dddd = vector Like 1234
        'Dim fffff = vector \ 33333

        'Dim ddddddd = vector Or fffff

        'Pause()

        'Dim textArray As Integer() = vector.value 'As Integer()

        'Call textArray.GetJson.__DEBUG_ECHO

        'Dim newText%() = {4, 5, 6, 7}

        'vector.value = newText

        'Dim gt = vector > 3

        Pause()
    End Sub
End Module
