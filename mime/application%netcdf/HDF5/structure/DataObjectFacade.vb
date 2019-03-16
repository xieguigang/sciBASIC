#Region "Microsoft.VisualBasic::d3b1042c48efd84ecfd340894fd869a5, mime\application%netcdf\HDF5\structure\DataObjectFacade.vb"

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

    '     Class DataObjectFacade
    ' 
    '         Properties: address, dataObject, layout, linkName, symbolName
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: readDataObject
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


Imports Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO

Namespace HDF5.[Structure]


    Public Class DataObjectFacade

        Private Shared ObjectAddressMap As New Dictionary(Of Long, DataObject)()

        Private m_address As Long
        Private m_dataObject As DataObject

        Private m_symbolName As String
        Private m_linkName As String

        Private m_layout As Layout

        Public Sub New([in] As BinaryReader, sb As Superblock, symbolName As String, address As Long)
            Me.m_address = address

            Dim dobj As DataObject = readDataObject([in], sb, address)
            Me.m_dataObject = dobj

            Me.m_symbolName = symbolName
            Me.m_linkName = Nothing

            Me.m_layout = Nothing
        End Sub

        Public Sub New([in] As BinaryReader, sb As Superblock, symbolName As String, linkName As String)
            Me.m_symbolName = symbolName
            Me.m_linkName = linkName

            Me.m_layout = Nothing
        End Sub

        Public Overridable ReadOnly Property address() As Long
            Get
                Return Me.m_address
            End Get
        End Property

        Private Function readDataObject([in] As BinaryReader, sb As Superblock, address As Long) As DataObject
            Dim dobj As DataObject = ObjectAddressMap.GetValueOrNull(address)
            If dobj Is Nothing Then
                dobj = New DataObject([in], sb, address)
                ObjectAddressMap(address) = dobj
            End If
            Return dobj
        End Function

        Public Overridable ReadOnly Property dataObject() As DataObject
            Get
                Return Me.m_dataObject
            End Get
        End Property

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


        Public Overridable ReadOnly Property layout() As Layout
            Get
                If Me.m_layout Is Nothing Then
                    Dim readLayout As New Layout()
                    If Me.m_dataObject IsNot Nothing Then
                        Dim msgs As List(Of ObjectHeaderMessage) = Me.m_dataObject.messages
                        If msgs IsNot Nothing Then
                            For Each msg As ObjectHeaderMessage In msgs
                                If msg.headerMessageType Is ObjectHeaderMessageType.Layout Then
                                    Dim lm As LayoutMessage = msg.layoutMessage

                                    Dim numberOfDimensions As Integer = lm.numberOfDimensions
                                    Dim chunkSize As Integer() = lm.chunkSize
                                    Dim dataAddress As Long = lm.dataAddress

                                    readLayout.numberOfDimensions = numberOfDimensions
                                    readLayout.chunkSize = chunkSize
                                    readLayout.dataAddress = dataAddress
                                ElseIf msg.headerMessageType Is ObjectHeaderMessageType.Datatype Then
                                    Dim dm As DataTypeMessage = msg.dataTypeMessage

                                    If dm.type = DataTypeMessage.DATATYPE_COMPOUND Then
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

                                        'int ndims = Math.min(dimensionLength.length, maxDimensionLength.length);
                                        'layout.setNumberOfDimensions(ndims);
                                        readLayout.maxDimensionLength = maxDimensionLength
                                    End If
                                    '
                                    '                                else if(msg.getHeaderMessageType() == ObjectHeaderMessageType.Attribute) {
                                    '                                	AttributeMessage am = msg.getAttributeMessage();
                                    '                                	
                                    '                                	DataTypeMessage dm = am.getDataType();
                                    '                                	if(dm != null) {
                                    '                                		dm.getType()
                                    '                                	}
                                    '                                }
                                    '                                

                                End If
                            Next
                        End If
                    End If
                    Me.m_layout = readLayout
                    Return readLayout
                End If

                Return Me.m_layout
            End Get
        End Property

        Public Overridable ReadOnly Property symbolName() As String
            Get
                Return Me.m_symbolName
            End Get
        End Property

        Public Overridable ReadOnly Property linkName() As String
            Get
                Return Me.m_linkName
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("DataObjectFacade >>>")

            Console.WriteLine("address : " & Me.m_address)

            If Me.m_dataObject IsNot Nothing Then
                Me.m_dataObject.printValues()
            End If

            If Not String.ReferenceEquals(Me.m_symbolName, Nothing) Then
                Console.WriteLine("symbol name : " & Me.m_symbolName)
            End If

            If Not String.ReferenceEquals(Me.m_linkName, Nothing) Then
                Console.WriteLine("link name : " & Me.m_linkName)
            End If

            Console.WriteLine("DataObjectFacade <<<")
        End Sub
    End Class

End Namespace
