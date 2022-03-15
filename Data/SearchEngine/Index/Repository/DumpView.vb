#Region "Microsoft.VisualBasic::74eb77378d4d7474f994b7cf74435e76, sciBASIC#\Data\SearchEngine\Index\Repository\DumpView.vb"

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

    '   Total Lines: 62
    '    Code Lines: 45
    ' Comment Lines: 4
    '   Blank Lines: 13
    '     File Size: 1.94 KB


    ' Module TrieDump
    ' 
    '     Sub: IndexDumpView, ShowCharacters
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Language

Public Module TrieDump

    ReadOnly DataPadLength As Integer = Long.MinValue.ToString.Length

    Public Sub ShowCharacters()
        Dim i As i32 = 1

        For c As Integer = Asc(" "c) To Asc("~"c)
            Call Console.WriteLine($" {Chr(c)} {++i}")
        Next
    End Sub

    Public Sub IndexDumpView(dbFile As String, out As StreamWriter)
        Using index As New BinaryDataReader(dbFile.Open(FileMode.Open, doClear:=False), Encoding.ASCII)
            ' magic
            Call out.Write(index.ReadString(length:=9).PadRight(DataPadLength))
            Call out.Write("| ")

            For c As Integer = Asc(" "c) To Asc("~"c)
                Call out.Write("  ")
                Call out.Write(Chr(c))
            Next

            Call out.WriteLine()

            For i As Integer = 0 To Integer.MaxValue
                ' data value
                Call out.Write(index.ReadInt64.ToString.PadLeft(DataPadLength))
                Call out.Write("| ")

                If index.EndOfStream Then
                    Exit For
                End If

                ' characters offset
                For c As Integer = Asc(" "c) To Asc("~"c)
                    Call out.Write(" ")
                    Call out.Write(index.ReadInt32.ToString.PadLeft(2))
                Next

                ' ZERO terminated flag
                Call out.Write(" ")
                Call out.Write(index.ReadInt32.ToString.PadLeft(2))
                Call out.Write(" |")
                Call out.Write(index.Position)

                If index.EndOfStream Then
                    Exit For
                Else
                    Call out.WriteLine()
                    Call out.Flush()
                End If
            Next

            Call out.Flush()
        End Using
    End Sub
End Module
