#Region "Microsoft.VisualBasic::5c157c208b6659a1ca66836033f2b40e, sciBASIC#\tutorials\core.test\treeTest.vb"

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

    '   Total Lines: 118
    '    Code Lines: 42
    ' Comment Lines: 44
    '   Blank Lines: 32
    '     File Size: 3.64 KB


    ' Module treeTest
    ' 
    '     Sub: Main, sortTest
    '     Structure compares
    ' 
    '         Function: Compare
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#Region "Microsoft.VisualBasic::9756da122d5c75de635b0950ca67a476, core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module treeTest
    ' 
    '     Sub: Main
    ' 
    ' 

#End Region

#Region "Microsoft.VisualBasic::80a3b9fac91571dcd682961d3f81301c, core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module treeTest
    ' 
    '     Sub: Main
    ' 
    ' 
    ' 

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree

Module treeTest
    Sub Main()

        Call sortTest()


        Dim tree As New BinaryTree(Of String)

        For i As Integer = 0 To 100
            Dim s = RandomASCIIString(10).MD5.Substring(0, 6)

            Thread.Sleep(10)

            Call tree.insert(s, s)
        Next


        Pause()
    End Sub

    Sub sortTest()
        Dim randoms = 1000000.SeqRandom.Select(Function(i) i.ToString).ToArray
        Dim compares = New List(Of String)(randoms)

        ' linq的效率最高 》 
        ' linq拓展函数稍慢
        ' list自身的排序方法最慢

        Call ApplicationServices.Time(Sub() randoms.OrderBy(AddressOf Val).ToArray).__DEBUG_ECHO
        Call ApplicationServices.Time(Sub() Call (From t In randoms Select value = Val(t) Order By value Ascending).ToArray).__DEBUG_ECHO
        Call ApplicationServices.Time(Sub() compares.Sort(New compares)).__DEBUG_ECHO

        Call "------------------".__DEBUG_ECHO
        compares = New List(Of String)(randoms)

        Call ApplicationServices.Time(Sub() Call (From t In randoms Select value = Val(t) Order By value Ascending).ToArray).__DEBUG_ECHO
        Call ApplicationServices.Time(Sub() compares.Sort(New compares)).__DEBUG_ECHO
        Call ApplicationServices.Time(Sub() randoms.OrderBy(AddressOf Val).ToArray).__DEBUG_ECHO

        compares = New List(Of String)(randoms)
        Call "------------------".__DEBUG_ECHO

        Call ApplicationServices.Time(Sub() compares.Sort(New compares)).__DEBUG_ECHO
        Call ApplicationServices.Time(Sub() Call (From t In randoms Select value = Val(t) Order By value Ascending).ToArray).__DEBUG_ECHO
        Call ApplicationServices.Time(Sub() randoms.OrderBy(AddressOf Val).ToArray).__DEBUG_ECHO

        '        [DEBUG 27/02/2018 243:21 PM] 674              @sortTest
        '[DEBUG 27/02/2018 2:43:21 PM] 591              @sortTest
        '[DEBUG 27/02/2018 2:43:24 PM] 2882              @sortTest
        '[DEBUG 27/02/2018 2:43:24 PM] ------------------
        '[DEBUG 27/02/2018 2:43:25 PM] 594              @sortTest
        '[DEBUG 27/02/2018 2:43:28 PM] 2790              @sortTest
        '[DEBUG 27/02/2018 2:43:28 PM] 696              @sortTest
        '[DEBUG 27/02/2018 2:43:28 PM] ------------------
        '[DEBUG 27/02/2018 2:43:31 PM] 2762              @sortTest
        '[DEBUG 27/02/2018 2:43:32 PM] 580              @sortTest
        '[DEBUG 27/02/2018 2:43:32 PM] 702              @sortTest
        'Press any key to continute...

        Pause()
    End Sub

    Structure compares
        Implements IComparer(Of String)

        Public Function Compare(x As String, y As String) As Integer Implements IComparer(Of String).Compare
            Return Val(x).CompareTo(Val(y))
        End Function
    End Structure
End Module
