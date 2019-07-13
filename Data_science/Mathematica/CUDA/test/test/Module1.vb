#Region "Microsoft.VisualBasic::c861f206b218535349c57efb289a929b, Data_science\Mathematica\CUDA\test\test\Module1.vb"

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
