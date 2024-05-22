#Region "Microsoft.VisualBasic::f5452fb0194b94b2389551ab0c3472be, Data\BinaryData\HDF5\structure\DataObjects\Headers\ObjectHeaderMessage.vb"

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

    '   Total Lines: 156
    '    Code Lines: 128 (82.05%)
    ' Comment Lines: 8 (5.13%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 20 (12.82%)
    '     File Size: 8.37 KB


    '     Class ObjectHeaderMessage
    ' 
    '         Properties: attributeMessage, continueMessage, dataspaceMessage, dataTypeMessage, fillValueMessage
    '                     fillValueOldMessage, filterPipelineMessage, groupMessage, headerLength, headerMessageData
    '                     headerMessageFlags, headerMessageType, headerMessageTypeNumber, lastModifiedMessage, layoutMessage
    '                     linkMessage, sizeOfHeaderMessageData
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: printValues
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

Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct.messages
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace struct


    Public Class ObjectHeaderMessage : Inherits HDF5Ptr

        Public ReadOnly Property headerMessageType As ObjectHeaderMessageType
        Public ReadOnly Property sizeOfHeaderMessageData As Integer
        Public ReadOnly Property headerMessageFlags As Byte
        Public ReadOnly Property headerLength As Integer
        Public ReadOnly Property headerMessageData As Byte()

        Public ReadOnly Property headerMessageTypeNumber As ObjectHeaderMessages
            Get
                Return headerMessageType.type
            End Get
        End Property

#Region "message data"

        Public ReadOnly Property groupMessage As GroupMessage
        Public ReadOnly Property continueMessage As ContinueMessage
        Public ReadOnly Property fillValueMessage As FillValueMessage
        Public ReadOnly Property fillValueOldMessage As FillValueOldMessage
        Public ReadOnly Property dataTypeMessage As DataTypeMessage
        Public ReadOnly Property attributeMessage As AttributeMessage
        Public ReadOnly Property linkMessage As LinkMessage
        Public ReadOnly Property layoutMessage As DataLayoutMessage
        Public ReadOnly Property lastModifiedMessage As LastModifiedMessage
        Public ReadOnly Property dataspaceMessage As DataspaceMessage
        Public ReadOnly Property filterPipelineMessage As FilterPipelineMessage
#End Region

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            Call MyBase.New(address)

            [in].offset = address

            Dim messageTypeNo As Short = [in].readShort()
            Me.headerMessageType = ObjectHeaderMessageType.[getType](messageTypeNo)
            If Me.headerMessageType Is Nothing Then
                Throw New IOException("message type no (" & messageTypeNo & ") not supported")
            End If
            Me.sizeOfHeaderMessageData = [in].readShort()
            Me.headerMessageFlags = [in].readByte()

            [in].skipBytes(3)

            Me.headerLength = 8

            If (Me.headerMessageFlags And &H2) <> 0 Then
                ' shared
                Throw New IOException("shared message is not implemented")
            End If

            If Me.headerMessageType Is ObjectHeaderMessageType.ObjectHeaderContinuation Then
                Me.continueMessage = New ContinueMessage(sb, [in].offset)
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.Group Then
                Me.groupMessage = New GroupMessage(sb, [in].offset)
                ' do nothing
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.NIL Then
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.Bogus Then
                Throw New InvalidDataException("Invalid HDF5 file!")
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.SimpleDataspace Then
                Me.dataspaceMessage = New DataspaceMessage(sb, [in].offset)
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.GroupNew Then
                Throw New IOException("Group New not implemented")
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.Datatype Then
                Me.dataTypeMessage = New DataTypeMessage(sb, [in].offset)
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.FillValueOld Then
                Me.fillValueOldMessage = New FillValueOldMessage(sb, [in].offset)
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.FillValue Then
                Me.fillValueMessage = New FillValueMessage(sb, [in].offset)
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.Link Then
                Me.linkMessage = New LinkMessage(sb, [in].offset)
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.Layout Then
                Me.layoutMessage = New DataLayoutMessage(sb, [in].offset)
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.GroupInfo Then
                Throw New IOException("Group Info not implemented")
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.FilterPipeline Then
                Me.filterPipelineMessage = New FilterPipelineMessage([in], sb, [in].offset)
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.Attribute Then
                Me.attributeMessage = New AttributeMessage([in], sb, [in].offset)
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.Comment Then
                Throw New IOException("Comment not implemented")
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.LastModifiedOld Then
                Throw New IOException("Last Modified Old not implemented")
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.LastModified Then
                Me.lastModifiedMessage = New LastModifiedMessage([in], sb, [in].offset)
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.AttributeInfo Then
                Throw New IOException("Attribute Info not implemented")
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.ObjectReferenceCount Then
                Throw New IOException("Object Reference Count not implemented")
            Else
                Me.headerMessageData = [in].readBytes(Me.sizeOfHeaderMessageData)
            End If
        End Sub

        Public Overrides Function ToString() As String
            Select Case CType(headerMessageType.num, ObjectHeaderMessages)
                Case ObjectHeaderMessages.SymbolTableMessage
                    Return groupMessage.ToString
                Case ObjectHeaderMessages.Attribute
                    Return attributeMessage.ToString
                Case Else
                    Return headerMessageType.ToString
            End Select
        End Function

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("ObjectHeaderMessage >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("header message type : " & Convert.ToString(Me.headerMessageType))
            console.WriteLine("size of header message data : " & Me.sizeOfHeaderMessageData)
            console.WriteLine("header message flags : " & Me.headerMessageFlags)

            If Me.headerMessageType Is ObjectHeaderMessageType.ObjectHeaderContinuation Then
                console.WriteLine("header message continue")
                Me.continueMessage.printValues(console)
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.Group Then
                Me.groupMessage.printValues(console)
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.SimpleDataspace Then
                Me.dataspaceMessage.printValues(console)
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.FillValue Then
                Me.fillValueMessage.printValues(console)
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.FillValueOld Then
                Me.fillValueOldMessage.printValues(console)
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.Datatype Then
                Me.dataTypeMessage.printValues(console)
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.Attribute Then
                Me.attributeMessage.printValues(console)
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.Link Then
                Me.linkMessage.printValues(console)
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.Layout Then
                Me.layoutMessage.printValues(console)
            ElseIf Me.headerMessageType Is ObjectHeaderMessageType.LastModified Then
                Me.lastModifiedMessage.printValues(console)
            Else
                console.WriteLine("header message data : " & Convert.ToString(Me.headerMessageData))
            End If

            console.WriteLine("ObjectHeaderMessage <<<")
        End Sub
    End Class

End Namespace
