#Region "Microsoft.VisualBasic::1727659579cb78a56cfd6c57ce9fb077, Data\BinaryData\HDF5\structure\DataObjects\Headers\Messages\FilterPipelineMessage.vb"

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

    '   Total Lines: 137
    '    Code Lines: 74
    ' Comment Lines: 40
    '   Blank Lines: 23
    '     File Size: 5.72 KB


    '     Class FilterPipelineMessage
    ' 
    '         Properties: description, numberOfFilters, version
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: decode, ToString
    ' 
    '         Sub: printValues
    ' 
    '     Class FilterDescription
    ' 
    '         Properties: clientData, filter, flags, id, name
    '                     nameLength, numberOfClientDataValues
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: doDecode, ToString
    ' 
    '         Sub: printValues
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.HDF5.dataset.filters
Imports Microsoft.VisualBasic.Language
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace struct.messages

    ''' <summary>
    ''' This message describes the filter pipeline which should be applied to 
    ''' the data stream by providing filter identification numbers, flags, 
    ''' a name, and client data.
    '''
    ''' This message may be present In the Object headers Of both dataset And 
    ''' group objects. For datasets, it specifies the filters To apply To 
    ''' raw data. For groups, it specifies the filters To apply To the group's 
    ''' fractal heap. Currently, only datasets using chunked data storage use 
    ''' the filter pipeline on their raw data.
    ''' </summary>
    Public Class FilterPipelineMessage : Inherits Message

        Public ReadOnly Property version As Integer
        Public ReadOnly Property numberOfFilters As Integer
        Public ReadOnly Property description As New List(Of FilterDescription)

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            MyBase.New(address)

            Me.version = [in].readByte()
            Me.numberOfFilters = [in].readByte

            ' skip 6 reserved zero bytes
            Dim reserved = [in].readBytes(6)

            If reserved.Any(Function(b) b <> 0) Then
                Throw New InvalidProgramException
            End If

            ' read filter descriptions
            For i As Integer = 0 To numberOfFilters - 1
                description += New FilterDescription([in], version, [in].offset)
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return description.Select(Function(filter) filter.name).JoinBy(" -> ")
        End Function

        Protected Friend Overrides Sub printValues(console As TextWriter)
            Throw New NotImplementedException()
        End Sub

        Public Function decode(encodedBytes() As Byte) As Byte()
            For Each filter As FilterDescription In description
                encodedBytes = filter.doDecode(encodedBytes)
            Next

            Return encodedBytes
        End Function
    End Class

    Public Class FilterDescription : Inherits HDF5Ptr

        ''' <summary>
        ''' This value, often referred to as a filter identifier, is designed to be 
        ''' a unique identifier for the filter. Values from zero through 32,767 are 
        ''' reserved for filters supported by The HDF Group in the HDF5 Library and 
        ''' for filters requested and supported by third parties. Filters supported 
        ''' by The HDF Group are documented immediately below. Information on 3rd-party 
        ''' filters can be found at The HDF Group’s Contributions page.
        ''' 
        ''' Values from 32768 to 65535 are reserved for non-distributed uses 
        ''' (for example, internal company usage) or for application usage when testing 
        ''' a feature. The HDF Group does not track or document the use of the filters 
        ''' with identifiers from this range.
        ''' </summary>
        ''' <returns></returns>
        Public Property id As Short
        ''' <summary>
        ''' Each filter has an optional null-terminated ASCII name and this field holds 
        ''' the length of the name including the null termination padded with nulls to 
        ''' be a multiple of eight. If the filter has no name then a value of zero is 
        ''' stored in this field.
        ''' </summary>
        ''' <returns></returns>
        Public Property nameLength As Integer
        Public Property flags As Short
        Public Property numberOfClientDataValues As Short

        Public ReadOnly Property name As String
        ''' <summary>
        ''' This is an array of four-byte integers which will be passed to the filter 
        ''' function. The Client Data Number of Values determines the number of elements
        ''' in the array.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property clientData As Integer()

        Public ReadOnly Property filter As IFilter

        Sub New([in] As BinaryReader, version%, address&)
            Call MyBase.New(address)

            If version = 1 Then
                id = [in].readShort
                nameLength = [in].readShort
                flags = [in].readShort
                numberOfClientDataValues = [in].readShort
                name = [in].readASCIIString
                clientData = New Integer(numberOfClientDataValues - 1) {}

                For i As Integer = 0 To numberOfClientDataValues - 1
                    clientData(i) = [in].readInt
                Next
            Else
                Throw New NotImplementedException
            End If

            If id = ReservedFilters.deflate Then
                filter = New DeflatePipelineFilter
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function doDecode(encodedBytes As IEnumerable(Of Byte)) As Byte()
            Return filter.decode(encodedBytes, clientData)
        End Function

        Public Overrides Function ToString() As String
            Return $"Call {name}({clientData.Select(Function(i) CStr(i)).JoinBy(", ")})"
        End Function

        Protected Friend Overrides Sub printValues(console As TextWriter)
            Throw New NotImplementedException()
        End Sub
    End Class
End Namespace
