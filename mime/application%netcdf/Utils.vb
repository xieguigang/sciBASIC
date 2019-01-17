#Region "Microsoft.VisualBasic::9dab233955c3b56aa5d39ffd710697ec, mime\application%netcdf\Utils.vb"

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

' Module Utils
' 
'     Function: notNetcdf, readName
' 
'     Sub: padding
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Language

Module Utils

    ''' <summary>
    ''' Throws a non-valid NetCDF exception if the statement it's true
    ''' </summary>
    ''' <param name="statement">statement - Throws if true</param>
    ''' <param name="reason$">reason - Reason to throw</param>
    Public Function notNetcdf(statement As Boolean, reason$) As Object
        If (statement) Then
            Throw New FormatException($"Not a valid NetCDF v3.x file: {reason}")
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' Moves 1, 2, Or 3 bytes to next 4-byte boundary
    ''' </summary>
    ''' <param name="buffer">
    ''' buffer - Buffer for the file data
    ''' </param>
    <Extension> Public Sub padding(buffer As BinaryDataReader)
        If ((buffer.Position Mod 4) <> 0) Then
            Call buffer.Seek(4 - (buffer.Position Mod 4), SeekOrigin.Current)
        End If
    End Sub

    <Extension> Public Sub writePadding(output As BinaryDataWriter)
        Dim n As Value(Of Long) = 0

        If ((n = (output.Position Mod 4)) <> 0) Then
            For i As Integer = 1 To 4 - CLng(n)
                Call output.Write(CByte(0))
            Next
        End If
    End Sub

    <Extension>
    Public Sub writeName(output As BinaryDataWriter, name$)
        Call output.Write(name, BinaryStringFormat.UInt32LengthPrefix)
        Call output.writePadding
    End Sub

    ''' <summary>
    ''' Reads the name
    ''' </summary>
    ''' <param name="buffer">
    ''' buffer - Buffer for the file data
    ''' </param>
    ''' <returns>Name</returns>
    <Extension> Public Function readName(buffer As BinaryDataReader) As String
        ' Read name
        Dim nameLength = buffer.ReadUInt32()
        Dim name() = buffer.ReadChars(nameLength)

        ' validate name
        ' TODO

        ' Apply padding
        ' 数据的长度应该是4的整数倍,如果不是,则会使用0进行填充
        Call buffer.padding()

        Return New String(name)
    End Function
End Module
