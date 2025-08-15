#Region "Microsoft.VisualBasic::da6fc1e665686990577940bfac671b54, Microsoft.VisualBasic.Core\test\test\memoryTest.vb"

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

    '   Total Lines: 28
    '    Code Lines: 19 (67.86%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (32.14%)
    '     File Size: 863 B


    ' Module memoryTest
    ' 
    '     Sub: runTest
    ' 
    ' Structure TestData
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.BinaryDumping

Module memoryTest

    ReadOnly test As New TestData With {.bits = {2, 3, 4, 23, 53, 5, 34, 64, 5, 6}, .flag = True, .ints = {24342, 53, 453, 463, 456457, 5675}, .name = "[asdasdasfsdf+++++++++++++++++++++++++++++++++++++++++]"}

    Sub runTest()
        Dim bytes1 = test.StructureToByte
        Dim reload = bytes1.ByteToStructure(Of TestData)

        Call Console.WriteLine(test.bits.SequenceEqual(reload.bits))
        Call Console.WriteLine(test.ints.SequenceEqual(reload.ints))
        Call Console.WriteLine(test.flag = reload.flag)
        Call Console.WriteLine(test.name = reload.name)

        Pause()
    End Sub

End Module

Public Structure TestData

    Dim name As String
    Dim bits As Byte()
    Dim flag As Boolean
    Dim ints As Integer()

End Structure
