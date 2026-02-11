#Region "Microsoft.VisualBasic::65949713bd5eee46632daab99233d465, Microsoft.VisualBasic.Core\src\Net\Wget\Axel\HashHelper.vb"

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

    '   Total Lines: 71
    '    Code Lines: 54 (76.06%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 17 (23.94%)
    '     File Size: 2.42 KB


    '     Class HashHelper
    ' 
    '         Function: Check, Load
    ' 
    '         Sub: Add, (+2 Overloads) Save
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Data.Repository

Namespace Net.WebClient

    Public Class HashHelper

        Dim hashcode As New Dictionary(Of String, (checksum As String, size As Long))
        Dim dbfile As String

        Public Sub Add(filepath As String)
            hashcode(filepath.ToLower.MD5) = (FileHashCheckSum.ComputeHash(filepath), filepath.FileLength)
        End Sub

        Public Function Check(filepath As String) As Boolean
            Dim key As String = filepath.ToLower.MD5

            If Not hashcode.ContainsKey(key) Then
                Return False
            End If

            Dim checksum As String = FileHashCheckSum.ComputeHash(filepath)
            Dim len As Long = filepath.FileLength

            With hashcode(key)
                Return .size = len AndAlso .checksum = checksum
            End With
        End Function

        Public Sub Save()
            Call Save(dbfile)
        End Sub

        Public Sub Save(filepath As String)
            Using bin As New BinaryWriter(filepath.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False))
                Call bin.Write(hashcode.Count)

                For Each file In hashcode
                    Call bin.Write(file.Key)
                    Call bin.Write(file.Value.checksum)
                    Call bin.Write(file.Value.size)
                Next
            End Using
        End Sub

        Public Shared Function Load(filepath As String) As HashHelper
            If Not filepath.FileExists(True) Then
                Return New HashHelper With {.dbfile = filepath}
            End If

            Using bin As New BinaryReader(filepath.Open(FileMode.Open, doClear:=False, [readOnly]:=True))
                Dim hashset As New Dictionary(Of String, (String, Long))
                Dim n As Integer = bin.ReadInt32

                For i As Integer = 1 To n
                    Dim key As String = bin.ReadString
                    Dim checksum As String = bin.ReadString
                    Dim len As Long = bin.ReadInt64

                    hashset(key) = (checksum, len)
                Next

                Return New HashHelper With {
                    .hashcode = hashset,
                    .dbfile = filepath
                }
            End Using
        End Function

    End Class
End Namespace
