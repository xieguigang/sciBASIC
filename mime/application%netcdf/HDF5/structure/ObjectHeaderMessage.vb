#Region "Microsoft.VisualBasic::6849725f6c24345bcf2c9e736d1ece61, mime\application%netcdf\HDF5\structure\ObjectHeaderMessage.vb"

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

    ' 	Class ObjectHeaderMessage
    ' 
    ' 	    Properties: address, attributeMessage, continueMessage, dataspaceMessage, dataTypeMessage
    '                  fillValueMessage, fillValueOldMessage, groupMessage, headerLength, headerMessageData
    '                  headerMessageFlags, headerMessageType, headerMessageTypeNo, lastModifiedMessage, layoutMessage
    '                  linkMessage, sizeOfHeaderMessageData
    ' 
    ' 	    Constructor: (+1 Overloads) Sub New
    ' 	    Sub: printValues
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
Imports BinaryReader = Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO.BinaryReader

Namespace HDF5.[Structure]


	Public Class ObjectHeaderMessage
		Private m_address As Long

		Private m_headerMessageType As ObjectHeaderMessageType
		Private m_sizeOfHeaderMessageData As Integer
		Private m_headerMessageFlags As Byte
		Private m_groupMessage As GroupMessage
		Private m_fillvalueMessage As FillValueMessage
		Private m_fillvalueoldMessage As FillValueOldMessage
		Private m_continueMessage As ContinueMessage
		Private m_datatypeMessage As DataTypeMessage
		Private m_dataspaceMessage As DataspaceMessage
		Private m_attributeMessage As AttributeMessage
		Private m_linkMessage As LinkMessage
		Private m_layoutMessage As LayoutMessage
		Private m_lastmodifiedMessage As LastModifiedMessage
		Private m_headerLength As Integer

		Private m_headerMessageData As Byte()

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
			[in].offset = address

			Me.m_address = address

			Dim messageTypeNo As Short = [in].readShort()
			Me.m_headerMessageType = ObjectHeaderMessageType.[getType](messageTypeNo)
			If Me.m_headerMessageType Is Nothing Then
				Throw New IOException("message type no (" & messageTypeNo & ") not supported")
			End If
			Me.m_sizeOfHeaderMessageData = [in].readShort()
			Me.m_headerMessageFlags = [in].readByte()

			[in].skipBytes(3)

			Me.m_headerLength = 8

			If (Me.m_headerMessageFlags And &H2) <> 0 Then
				' shared
				Throw New IOException("shared message is not implemented")
			End If

			If Me.m_headerMessageType Is ObjectHeaderMessageType.ObjectHeaderContinuation Then
				Me.m_continueMessage = New ContinueMessage([in], sb, [in].offset)
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.Group Then
				Me.m_groupMessage = New GroupMessage([in], sb, [in].offset)
					' do nothing
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.NIL Then
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.SimpleDataspace Then
				Me.m_dataspaceMessage = New DataspaceMessage([in], sb, [in].offset)
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.GroupNew Then
				Throw New IOException("Group New not implemented")
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.Datatype Then
				Me.m_datatypeMessage = New DataTypeMessage([in], sb, [in].offset)
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.FillValueOld Then
				Me.m_fillvalueoldMessage = New FillValueOldMessage([in], sb, [in].offset)
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.FillValue Then
				Me.m_fillvalueMessage = New FillValueMessage([in], sb, [in].offset)
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.Link Then
				Me.m_linkMessage = New LinkMessage([in], sb, [in].offset)
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.Layout Then
				Me.m_layoutMessage = New LayoutMessage([in], sb, [in].offset)
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.GroupInfo Then
				Throw New IOException("Group Info not implemented")
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.FilterPipeline Then
				Throw New IOException("Filter Pipeline not implemented")
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.Attribute Then
				Me.m_attributeMessage = New AttributeMessage([in], sb, [in].offset)
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.Comment Then
				Throw New IOException("Comment not implemented")
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.LastModifiedOld Then
				Throw New IOException("Last Modified Old not implemented")
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.LastModified Then
				Me.m_lastmodifiedMessage = New LastModifiedMessage([in], sb, [in].offset)
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.AttributeInfo Then
				Throw New IOException("Attribute Info not implemented")
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.ObjectReferenceCount Then
				Throw New IOException("Object Reference Count not implemented")
			Else
				Me.m_headerMessageData = [in].readBytes(Me.m_sizeOfHeaderMessageData)
			End If
		End Sub

		Public Overridable ReadOnly Property address() As Long
			Get
				Return Me.m_address
			End Get
		End Property

		Public Overridable ReadOnly Property headerMessageTypeNo() As Integer
			Get
				Return Me.m_headerMessageType.num
			End Get
		End Property

		Public Overridable ReadOnly Property headerMessageType() As ObjectHeaderMessageType
			Get
				Return Me.m_headerMessageType
			End Get
		End Property

		Public Overridable ReadOnly Property sizeOfHeaderMessageData() As Integer
			Get
				Return Me.m_sizeOfHeaderMessageData
			End Get
		End Property

		Public Overridable ReadOnly Property headerMessageFlags() As Byte
			Get
				Return Me.m_headerMessageFlags
			End Get
		End Property

		Public Overridable ReadOnly Property headerLength() As Integer
			Get
				Return Me.m_headerLength
			End Get
		End Property

		Public Overridable ReadOnly Property headerMessageData() As Byte()
			Get
				Return Me.m_headerMessageData
			End Get
		End Property

		Public Overridable ReadOnly Property groupMessage() As GroupMessage
			Get
				Return Me.m_groupMessage
			End Get
		End Property

		Public Overridable ReadOnly Property continueMessage() As ContinueMessage
			Get
				Return Me.m_continueMessage
			End Get
		End Property

		Public Overridable ReadOnly Property fillValueMessage() As FillValueMessage
			Get
				Return Me.m_fillvalueMessage
			End Get
		End Property

		Public Overridable ReadOnly Property fillValueOldMessage() As FillValueOldMessage
			Get
				Return Me.m_fillvalueoldMessage
			End Get
		End Property

		Public Overridable ReadOnly Property dataTypeMessage() As DataTypeMessage
			Get
				Return Me.m_datatypeMessage
			End Get
		End Property

		Public Overridable ReadOnly Property attributeMessage() As AttributeMessage
			Get
				Return Me.m_attributeMessage
			End Get
		End Property

		Public Overridable ReadOnly Property linkMessage() As LinkMessage
			Get
				Return Me.m_linkMessage
			End Get
		End Property

		Public Overridable ReadOnly Property layoutMessage() As LayoutMessage
			Get
				Return Me.m_layoutMessage
			End Get
		End Property

		Public Overridable ReadOnly Property lastModifiedMessage() As LastModifiedMessage
			Get
				Return Me.m_lastmodifiedMessage
			End Get
		End Property

		Public Overridable ReadOnly Property dataspaceMessage() As DataspaceMessage
			Get
				Return Me.m_dataspaceMessage
			End Get
		End Property

		Public Overridable Sub printValues()
			Console.WriteLine("ObjectHeaderMessage >>>")
			Console.WriteLine("address : " & Me.m_address)
			Console.WriteLine("header message type : " & Convert.ToString(Me.m_headerMessageType))
			Console.WriteLine("size of header message data : " & Me.m_sizeOfHeaderMessageData)
			Console.WriteLine("header message flags : " & Me.m_headerMessageFlags)

			If Me.m_headerMessageType Is ObjectHeaderMessageType.ObjectHeaderContinuation Then
				Console.WriteLine("header message continue")
				Me.m_continueMessage.printValues()
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.Group Then
				Me.m_groupMessage.printValues()
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.SimpleDataspace Then
				Me.m_dataspaceMessage.printValues()
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.FillValue Then
				Me.m_fillvalueMessage.printValues()
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.FillValueOld Then
				Me.m_fillvalueoldMessage.printValues()
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.Datatype Then
				Me.m_datatypeMessage.printValues()
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.Attribute Then
				Me.m_attributeMessage.printValues()
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.Link Then
				Me.m_linkMessage.printValues()
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.Layout Then
				Me.m_layoutMessage.printValues()
			ElseIf Me.m_headerMessageType Is ObjectHeaderMessageType.LastModified Then
				Me.m_lastmodifiedMessage.printValues()
			Else
				Console.WriteLine("header message data : " & Convert.ToString(Me.m_headerMessageData))
			End If

			Console.WriteLine("ObjectHeaderMessage <<<")
		End Sub
	End Class

End Namespace

