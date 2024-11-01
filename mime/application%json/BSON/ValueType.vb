#Region "Microsoft.VisualBasic::80bb9318b8dd0a873ee3ea9eab63e99a, mime\application%json\BSON\ValueType.vb"

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

    '   Total Lines: 47
    '    Code Lines: 30 (63.83%)
    ' Comment Lines: 9 (19.15%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 8 (17.02%)
    '     File Size: 1.17 KB


    '     Enum ValueType
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class ObjectId
    ' 
    '         Properties: value
    ' 
    '         Function: ReadIdValue, ToString
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

        Public Overrides Function ToString() As String
            Return value.Select(Function(b) b.ToString("x2")).JoinBy("")
        End Function

        Public Shared Function ReadIdValue(s As BinaryReader) As JsonValue
            Dim byts = s.ReadBytes(12)
            Dim id As New ObjectId With {.value = byts}

            Return New JsonValue(id)
        End Function

    End Class
End Namespace
