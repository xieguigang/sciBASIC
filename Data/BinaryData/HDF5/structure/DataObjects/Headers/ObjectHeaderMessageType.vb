#Region "Microsoft.VisualBasic::4cbb3be3ea9b306a033f1d1c22ea2415, Data\BinaryData\HDF5\structure\DataObjects\Headers\ObjectHeaderMessageType.vb"

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

    '   Total Lines: 183
    '    Code Lines: 83
    ' Comment Lines: 85
    '   Blank Lines: 15
    '     File Size: 8.63 KB


    '     Enum ObjectHeaderMessages
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class ObjectHeaderMessageType
    ' 
    '         Properties: num, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) [getType], ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 

Namespace struct

    Public Enum ObjectHeaderMessages As Integer
        NIL = &H0
        Dataspace = &H1
        LinkInfo = &H2
        Datatype = &H3
        FillValueOld = &H4
        FillValue = &H5
        Link = &H6
        ExternalDataFiles = &H7
        DataLayout = &H8
        Bogus = &H9
        GroupInfo = &HA
        DataStorageFilterPipeline = &HB
        Attribute = &HC
        ObjectComment = &HD
        ObjectModificationTimeOld = &HE
        SharedMessageTable = &HF
        ObjectHeaderContinuation = &H10
        SymbolTableMessage = &H11
        ObjectModificationTime = &H12
        BtreeKValues = &H13
        DriverInfo = &H14
        AttributeInfo = &H15
        ObjectReferenceCount = &H16
    End Enum

    ''' <summary>
    ''' Disk Format: Level 2A2 - Data Object Header Messages
    ''' 
    ''' Data object header messages are small pieces of metadata that are 
    ''' stored in the data object header for each object in an HDF5 file. 
    ''' Data object header messages provide the metadata required to describe 
    ''' an object and its contents, as well as optional pieces of metadata 
    ''' that annotate the meaning or purpose of the object.
    '''
    ''' Data object header messages are either stored directly in the data 
    ''' object header for the object Or are shared between multiple objects 
    ''' in the file. When a message Is shared, a flag in the Message Flags 
    ''' indicates that the actual Message Data portion of that message Is 
    ''' stored in another location (such as another data object header, Or 
    ''' a heap in the file) And the Message Data field contains the information 
    ''' needed to locate the actual information for the message.
    ''' </summary>
    Public Class ObjectHeaderMessageType

        Const MAX_MESSAGE As Integer = 23

        Shared ReadOnly hash As New Dictionary(Of String, ObjectHeaderMessageType)(10)
        Shared ReadOnly mess As ObjectHeaderMessageType() = New ObjectHeaderMessageType(MAX_MESSAGE - 1) {}

        ''' <summary>
        ''' The NIL message is used to indicate a message which is to be ignored when reading 
        ''' the header messages for a data object. [Possibly one which has been deleted for 
        ''' some reason.]
        ''' </summary>
        Public Shared ReadOnly NIL As New ObjectHeaderMessageType("NIL", 0)
        ''' <summary>
        ''' The dataspace message describes the number of dimensions (in other words, "rank") 
        ''' and size of each dimension that the data object has. This message is only used for 
        ''' datasets which have a simple, rectilinear, array-like layout; datasets requiring 
        ''' a more complex layout are not yet supported.
        ''' </summary>
        Public Shared ReadOnly SimpleDataspace As New ObjectHeaderMessageType("SimpleDataspace", 1)
        ''' <summary>
        ''' 
        ''' </summary>
        Public Shared ReadOnly GroupNew As New ObjectHeaderMessageType("GroupNew", 2)
        Public Shared ReadOnly Datatype As New ObjectHeaderMessageType("Datatype", 3)

        ''' <summary>
        ''' The fill value message stores a single data value which is returned to the application 
        ''' when an uninitialized data element is read from a dataset. The fill value is interpreted 
        ''' with the same datatype as the dataset. If no fill value message is present then a fill 
        ''' value of all zero bytes is assumed.
        '''
        ''' This fill value message Is deprecated In favor Of the “new” fill value message 
        ''' (Message Type 0x0005) And Is only written To the file For forward compatibility With 
        ''' versions Of the HDF5 Library before the 1.6.0 version. Additionally, it only appears 
        ''' For datasets With a user-defined fill value (As opposed To the library Default fill 
        ''' value Or an explicitly Set “undefined” fill value).
        ''' </summary>
        Public Shared ReadOnly FillValueOld As New ObjectHeaderMessageType("FillValueOld", 4)
        Public Shared ReadOnly FillValue As New ObjectHeaderMessageType("FillValue", 5)
        ''' <summary>
        ''' This message encodes the information for a link in a group's object header, when the group 
        ''' is storing its links “compactly”, or in the group’s fractal heap, when the group is storing 
        ''' its links “densely”.
        '''
        ''' A group Is storing its links compactly When the fractal heap address In the Link Info 
        ''' Message Is Set To the “undefined address” value.
        ''' </summary>
        Public Shared ReadOnly Link As New ObjectHeaderMessageType("Link", 6)
        Public Shared ReadOnly ExternalDataFiles As New ObjectHeaderMessageType("ExternalDataFiles", 7)
        Public Shared ReadOnly Layout As New ObjectHeaderMessageType("Layout", 8)
        Public Shared ReadOnly GroupInfo As New ObjectHeaderMessageType("GroupInfo", 10)
        Public Shared ReadOnly FilterPipeline As New ObjectHeaderMessageType("FilterPipeline", 11)
        Public Shared ReadOnly Attribute As New ObjectHeaderMessageType("Attribute", 12)
        Public Shared ReadOnly Comment As New ObjectHeaderMessageType("Comment", 13)
        Public Shared ReadOnly LastModifiedOld As New ObjectHeaderMessageType("LastModifiedOld", 14)
        Public Shared ReadOnly SharedObject As New ObjectHeaderMessageType("SharedObject", 15)
        ''' <summary>
        ''' The Object Header Continuation Message
        ''' 
        ''' The object header continuation is the location in the file of a block containing more 
        ''' header messages for the current data object. This can be used when header blocks 
        ''' become too large or are likely to change over time.
        ''' </summary>
        Public Shared ReadOnly ObjectHeaderContinuation As New ObjectHeaderMessageType("ObjectHeaderContinuation", 16)
        Public Shared ReadOnly Bogus As New ObjectHeaderMessageType("Bogus", ObjectHeaderMessages.Bogus)
        ''' <summary>
        ''' Each "old style" group has a v1 B-tree and a local heap for storing symbol table entries, 
        ''' which are located with this message.
        ''' </summary>
        Public Shared ReadOnly Group As New ObjectHeaderMessageType("Group", 17)
        Public Shared ReadOnly LastModified As New ObjectHeaderMessageType("LastModified", 18)
        Public Shared ReadOnly AttributeInfo As New ObjectHeaderMessageType("AttributeInfo", 21)
        Public Shared ReadOnly ObjectReferenceCount As New ObjectHeaderMessageType("ObjectReferenceCount", 22)

        Dim name As String

        ''' <summary>
        ''' Message number.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property num As Integer

        Public ReadOnly Property type As ObjectHeaderMessages
            Get
                Return CType(num, ObjectHeaderMessages)
            End Get
        End Property

        Private Sub New(name As String, num As Integer)
            Me.name = name
            Me.num = num

            hash(name) = Me
            mess(num) = Me
        End Sub

        ''' <summary>
        ''' Find the MessageType that matches this name.
        ''' </summary>
        ''' <param name="name"> find DataTYpe with this name. </param>
        ''' <returns> DataType or null if no match. </returns>
        Public Overloads Shared Function [getType](name As String) As ObjectHeaderMessageType
            If name.StringEmpty Then
                Return Nothing
            Else
                Return hash.GetValueOrNull(name)
            End If
        End Function

        ''' <summary>
        ''' Get the MessageType by number.
        ''' </summary>
        ''' <param name="num"> message number. </param>
        ''' <returns> the MessageType </returns>
        Public Overloads Shared Function [getType](num As Integer) As ObjectHeaderMessageType
            If (num < 0) OrElse (num >= MAX_MESSAGE) Then
                Return Nothing
            Else
                Return mess(num)
            End If
        End Function

        ''' <summary>
        ''' Message name.
        ''' </summary>
        Public Overrides Function ToString() As String
            Return name & "(" & num & ")"
        End Function
    End Class
End Namespace
