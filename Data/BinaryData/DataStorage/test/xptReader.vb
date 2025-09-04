#Region "Microsoft.VisualBasic::85f43ab097f659daad9494c07cf4104a, Data\BinaryData\DataStorage\test\xptReader.vb"

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

    '   Total Lines: 37
    '    Code Lines: 29 (78.38%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (21.62%)
    '     File Size: 1.25 KB


    ' Module xptReader
    ' 
    '     Sub: fileTest, Main, tableReader
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.IO.Xpt
Imports Microsoft.VisualBasic.Serialization.JSON

Module xptReader

    Const testfile = "G:\pixelArtist\src\framework\Data\data\ALQ_H.xpt"
    Const test2222 = "G:\pixelArtist\src\framework\Data\data\test.xpt"

    Sub Main()
        Call tableReader()
    End Sub

    Sub fileTest()
        Dim converter As SASXportConverter = New SASXportConverter(testfile)
        converter.Dispose()

        Dim iterator As SASXportFileIterator = New SASXportFileIterator(test2222)
        While iterator.hasNext()
            Dim row As Object() = iterator.next().ToArray
            Call Console.WriteLine(row.GetJson(knownTypes:={GetType(String), GetType(Integer), GetType(Double), GetType(Date), GetType(Single), GetType(Long), GetType(Boolean)}))
        End While
        Console.WriteLine("Total Rows: " & iterator.RowCount.ToString())
        iterator.Dispose()

        Dim cal As Date = New DateTime()
        cal = New DateTime(1960, 1, 1)
        cal.AddDays(19778)
        Console.WriteLine(cal.ToString())
    End Sub

    Sub tableReader()
        Dim tbl = FrameReader.ReadSasXPT(test2222)

        Pause()
    End Sub
End Module
