#Region "Microsoft.VisualBasic::b9a8ea37a6d8a2df83f45fe4bcddcf33, mime\application%json\BSON\ValueType.vb"

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

'   Total Lines: 16
'    Code Lines: 15 (93.75%)
' Comment Lines: 0 (0.00%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 1 (6.25%)
'     File Size: 340 B


'     Enum ValueType
' 
' 
'  
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Namespace BSON

    Public Enum ValueType As Byte
        [Double] = &H1
        [String] = &H2
        Document = &H3
        Array = &H4
        Binary = &H5
        ''' <summary>
        ''' undefined, obsolete in bson
        ''' </summary>
        Undefined = &H6
        ''' <summary>
        ''' 
        ''' </summary>
        ObjectId = &H7
        [Boolean] = &H8
        UTCDateTime = &H9
        ''' <summary>
        ''' NULL
        ''' </summary>
        None = &HA
        Int32 = &H10
        Int64 = &H12
        [Object] = Document
    End Enum

    Public Class ObjectId

        Public Property value As Byte()

        Public Shared Function ReadIdValue(s As BinaryReader) As JsonValue
            Dim byts = s.ReadBytes(12)
            Dim id As New ObjectId With {.value = byts}

            Return New JsonValue(id)
        End Function

    End Class
End Namespace
