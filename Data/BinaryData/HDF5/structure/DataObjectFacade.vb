#Region "Microsoft.VisualBasic::fec9897acf571402da089a52c1779c89, Data\BinaryData\HDF5\structure\DataObjectFacade.vb"

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

    '   Total Lines: 257
    '    Code Lines: 166
    ' Comment Lines: 42
    '   Blank Lines: 49
    '     File Size: 10.55 KB


    '     Class DataObjectFacade
    ' 
    '         Properties: attributes, dataObject, filterMessage, layout, layoutMessage
    '                     linkName, symbolName
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetMessage, readDataObject, readObjectLayout, ToString
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
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct.messages
Imports Microsoft.VisualBasic.Data.IO.HDF5.type
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace struct

    ''' <summary>
    ''' 可能是一个dataset，也可能是一个<see cref="Group"/>
    ''' </summary>
    Public Class DataObjectFacade : Inherits HDF5Ptr

        Dim m_layout As Layout

        Public ReadOnly Property dataObject As DataObject
        Public ReadOnly Property symbolName As String
        Public ReadOnly Property linkName As String

        Public ReadOnly Property layout As Layout
            Get
                If Me.m_layout Is Nothing Then
                    Me.m_layout = readObjectLayout()
                End If

                Return Me.m_layout
            End Get
        End Property

        Public ReadOnly Property attributes As AttributeMessage()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return dataObject _
                    .messages _
                    .Where(Function(msg) Not msg.attributeMessage Is Nothing) _
                    .Select(Function(a) a.attributeMessage) _
                    .ToArray
            End Get
        End Property

        Public ReadOnly Property layoutMessage As DataLayoutMessage
            Get
                Return GetMessage(ObjectHeaderMessages.DataLayout)
            End Get
        End Property

        Public ReadOnly Property filterMessage As FilterPipelineMessage
            Get
                Return GetMessage(ObjectHeaderMessages.DataStorageFilterPipeline)
            End Get
        End Property

        Public Sub New(sb As Superblock, symbolName As String, address As Long)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(-1)
            Dim dobj As DataObject = readDataObject([in], sb, address)

            Me.dataObject = dobj
            Me.symbolName = symbolName
            Me.linkName = Nothing
            Me.m_layout = Nothing
        End Sub

        Public Sub New(sb As Superblock, symbolName As String, linkName As String)
            Call MyBase.New(Scan0)

            Me.symbolName = symbolName
            Me.linkName = linkName
            Me.m_layout = Nothing
        End Sub

        Private Function readDataObject([in] As BinaryReader, sb As Superblock, address As Long) As DataObject
            Dim dobj As DataObject = sb.GetCacheObject(address)

            If dobj Is Nothing Then
                dobj = New DataObject(sb, address)
                sb.AddCacheObject(dobj)
            End If

            Return dobj
        End Function

        ''' <summary>
        ''' 如果存在重复的话，这个函数只会读取第一条
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function GetMessage(type As ObjectHeaderMessages) As Message
            Dim objMsg = dataObject.messages.FirstOrDefault(Function(msg) msg.headerMessageTypeNumber = type)

            If objMsg Is Nothing Then
                Return Nothing
            End If

            Select Case type
                Case ObjectHeaderMessages.Attribute : Return objMsg.attributeMessage
                Case ObjectHeaderMessages.AttributeInfo : Return Nothing
                Case ObjectHeaderMessages.Bogus : Return Nothing
                Case ObjectHeaderMessages.BtreeKValues : Return Nothing
                Case ObjectHeaderMessages.DataLayout : Return objMsg.layoutMessage
                Case ObjectHeaderMessages.Dataspace : Return objMsg.dataspaceMessage
                Case ObjectHeaderMessages.DataStorageFilterPipeline : Return objMsg.filterPipelineMessage
                Case ObjectHeaderMessages.Datatype : Return objMsg.dataTypeMessage
                Case ObjectHeaderMessages.DriverInfo : Return Nothing
                Case ObjectHeaderMessages.ExternalDataFiles : Return Nothing
                Case ObjectHeaderMessages.FillValue : Return objMsg.fillValueMessage
                Case ObjectHeaderMessages.FillValueOld : Return objMsg.fillValueOldMessage
                Case ObjectHeaderMessages.GroupInfo : Return Nothing
                Case ObjectHeaderMessages.Link : Return objMsg.linkMessage
                Case ObjectHeaderMessages.LinkInfo : Return Nothing
                Case ObjectHeaderMessages.NIL : Return Nothing
                Case ObjectHeaderMessages.ObjectComment : Return Nothing
                Case ObjectHeaderMessages.ObjectHeaderContinuation : Return Nothing
                Case ObjectHeaderMessages.ObjectModificationTime : Return objMsg.lastModifiedMessage
                Case ObjectHeaderMessages.ObjectModificationTimeOld : Return Nothing
                Case ObjectHeaderMessages.ObjectReferenceCount : Return Nothing
                Case ObjectHeaderMessages.SharedMessageTable : Return Nothing
                Case ObjectHeaderMessages.SymbolTableMessage : Return objMsg.groupMessage

                Case Else
                    Return Nothing
            End Select

            Return Nothing
        End Function

        '
        '        public Hashtable<String, String> getAttributes(BinaryReader in, Superblock sb) throws IOException {
        '        	Hashtable<String, String> ht = new Hashtable<String, String>();
        '        	
        '        	if(this.m_dataObject != null) {
        '        		Vector<ObjectHeaderMessage> msgs = this.m_dataObject.getMessages();
        '        		if(msgs != null) {
        '        			for(ObjectHeaderMessage msg : msgs) {
        '        				if(msg.getHeaderMessageType() == ObjectHeaderMessageType.Attribute) {
        '        					AttributeMessage am = msg.getAttributeMessage();
        '        					String key = am.getName();
        '        					
        '        					long doffset = am.getDataPos();
        '        					in.setOffset(doffset);
        '        					
        '        					String value = in.readASCIIString();
        '        					
        '        					ht.put(key, value);
        '        				}
        '        			}
        '        		}
        '        	}
        '        	
        '        	return ht;
        '        }
        '        

        Private Function readObjectLayout() As Layout
            Dim readLayout As New Layout()

            If Me.dataObject Is Nothing Then
                Return readLayout
            End If

            Dim msgs As List(Of ObjectHeaderMessage) = Me.dataObject.messages

            If msgs Is Nothing Then
                Return readLayout
            End If

            For Each msg As ObjectHeaderMessage In msgs
                If msg.headerMessageType Is ObjectHeaderMessageType.Layout Then
                    Dim lm As DataLayoutMessage = msg.layoutMessage

                    Dim numberOfDimensions As Integer = lm.dimensionality - 1
                    Dim chunkSize As Integer() = lm.chunkSize
                    Dim dataAddress As Long = lm.dataAddress

                    readLayout.numberOfDimensions = numberOfDimensions
                    readLayout.chunkSize = chunkSize
                    readLayout.dataAddress = dataAddress
                    readLayout.dataset = lm.dataset

                ElseIf msg.headerMessageType Is ObjectHeaderMessageType.Datatype Then
                    Dim dm As DataTypeMessage = msg.dataTypeMessage

                    If dm.type = DataTypes.DATATYPE_COMPOUND Then
                        Dim sms As List(Of StructureMember) = dm.structureMembers

                        If sms IsNot Nothing Then
                            For Each sm As StructureMember In sms
                                Dim name As String = sm.name
                                Dim offset As Integer = sm.offset
                                Dim dims As Integer = sm.dims

                                Dim dataType As Integer = -1
                                Dim byteLength As Integer = -1
                                Dim dtm As DataTypeMessage = sm.message

                                If dtm IsNot Nothing Then
                                    dataType = dtm.type
                                    byteLength = dtm.byteSize
                                End If

                                readLayout.addField(name, offset, dims, dataType, byteLength)
                            Next
                        End If
                    End If
                ElseIf msg.headerMessageType Is ObjectHeaderMessageType.SimpleDataspace Then
                    Dim dm As DataspaceMessage = msg.dataspaceMessage

                    If dm IsNot Nothing Then
                        Dim dimensionLength As Integer() = dm.dimensionLength
                        Dim maxDimensionLength As Integer() = dm.maxDimensionLength

                        readLayout.dimensionLength = dimensionLength

                        'int ndims = stdNum.min(dimensionLength.length, maxDimensionLength.length);
                        'layout.setNumberOfDimensions(ndims);
                        readLayout.maxDimensionLength = maxDimensionLength
                    End If
                End If
            Next

            Return readLayout
        End Function

        Public Overrides Function ToString() As String
            Return symbolName
        End Function

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("DataObjectFacade >>>")
            console.WriteLine("address : " & Me.m_address)

            If Me.dataObject IsNot Nothing Then
                Me.dataObject.printValues(console)
            End If

            If Not String.ReferenceEquals(Me.symbolName, Nothing) Then
                console.WriteLine("symbol name : " & Me.symbolName)
            End If

            If Not String.ReferenceEquals(Me.linkName, Nothing) Then
                console.WriteLine("link name : " & Me.linkName)
            End If

            console.WriteLine("DataObjectFacade <<<")
        End Sub
    End Class

End Namespace
