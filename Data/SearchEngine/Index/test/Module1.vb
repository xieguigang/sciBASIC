#Region "Microsoft.VisualBasic::3968a8bbad456345560599568d689882, sciBASIC#\Data\SearchEngine\Index\test\Module1.vb"

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

    '   Total Lines: 53
    '    Code Lines: 40
    ' Comment Lines: 1
    '   Blank Lines: 12
    '     File Size: 1.80 KB


    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO.SearchEngine.Index

Module Module1

    Sub Main()
        Dim testfile$ = "./test_trieindex.db"

        Using index As New TrieIndexWriter(testfile.Open(doClear:=True))
            Call index.AddTerm("A Hello World!", 223388888)
            Call index.AddTerm("A", 88)
            Call index.AddTerm("AB", -1111)
            Call index.AddTerm("ABC", 11)
            Call index.AddTerm("ABCD", 222)
            Call index.AddTerm("ABCDE", 3333)
            Call index.AddTerm("ABCDF", 666)

            Call index.AddTerm("XABCCC", 999)
            Call index.AddTerm("ABCCD", 555)
            Call index.AddTerm("ABCEE", 222)
            Call index.AddTerm("XABCDE", 777)
            Call index.AddTerm("TrieIndexWriter", 1234567)
        End Using

        Using dmp = testfile.ChangeSuffix("log").OpenWriter
            Try
                Call DumpView.IndexDumpView(testfile, dmp)
            Catch ex As Exception

            End Try

            Call dmp.WriteLine()
        End Using

        '  Pause()

        Call Console.WriteLine()

        Using index As New TrieIndexReader(testfile)
            Call Console.WriteLine(index.GetData("TrieIndexWriter"))
            Call Console.WriteLine(index.GetData("A Hello World!"))
            Call Console.WriteLine(index.GetData("A"))
            Call Console.WriteLine(index.GetData("AB"))
            Call Console.WriteLine(index.GetData("ABCCD"))
            Call Console.WriteLine(index.GetData("ABCEE"))
            Call Console.WriteLine(index.GetData("XABCCC"))
            Call Console.WriteLine(index.GetData("ABCDF"))
            Call Console.WriteLine(index.GetData("XABCDE"))
        End Using

        Pause()
    End Sub

End Module
