#Region "Microsoft.VisualBasic::74239067c5b952cec25022e06e5f1315, Data\BinaryData\BinaryData\SQLite3\Objects\Enums\SqliteDataType.vb"

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

    '     Enum SqliteDataType
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module DataTypeParser
    ' 
    '         Function: TryParse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ManagedSqlite.Core.Objects.Enums

    Public Enum SqliteDataType As Byte
        ''' <summary>
        ''' The value is a NULL value.
        ''' </summary>
        Null = 0
        ''' <summary>
        ''' The value is a signed integer, stored in 1, 2, 3, 4, 6, or 8 bytes depending on the magnitude of the value.
        ''' </summary>
        [Integer] = 1
        Float = 7
        Boolean0 = 8
        Boolean1 = 9
        Blob = 12
        Text = 13
    End Enum

    Module DataTypeParser

        Public Function TryParse(type As String) As SqliteDataType
            Select Case Strings.LCase(type)
                Case "integer", "int", "int64"
                    Return SqliteDataType.Integer
                Case "float", "double"
                    Return SqliteDataType.Float
                Case "text", "blob_text"
                    Return SqliteDataType.Text
                Case "blob"
                    Return SqliteDataType.Blob
                Case "null"
                    Return SqliteDataType.Null
                Case Else
                    If type = "varchar" OrElse type.IsPattern("varchar\(\d+\)") Then
                        Return SqliteDataType.Text
                    Else
                        Throw New NotImplementedException(type)
                    End If
            End Select
        End Function
    End Module
End Namespace
