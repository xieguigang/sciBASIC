
'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 


Imports Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO

Namespace HDF5.[Structure]


    Public Class DataObject
        Private m_address As Long
        Private m_objectHeader As ObjectHeader
        Private m_groupMessage As GroupMessage

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            [in].offset = address

            Me.m_address = address

            Me.m_objectHeader = New ObjectHeader([in], sb, address)

            For Each msg As ObjectHeaderMessage In Me.m_objectHeader.headerMessages
                If msg.headerMessageType Is ObjectHeaderMessageType.Group Then
                    Me.m_groupMessage = msg.groupMessage
                End If
            Next
        End Sub

        Public Overridable ReadOnly Property address() As Long
            Get
                Return Me.m_address
            End Get
        End Property

        Public Overridable ReadOnly Property messages() As List(Of ObjectHeaderMessage)
            Get
                If Me.m_objectHeader IsNot Nothing Then
                    Return Me.m_objectHeader.headerMessages
                End If
                Return Nothing
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("DataObject >>>")
            Console.WriteLine("address : " & Me.m_address)
            If Me.m_objectHeader IsNot Nothing Then
                Me.m_objectHeader.printValues()
            End If
            Console.WriteLine("DataObject <<<")
        End Sub

        Public Overridable ReadOnly Property groupMessage() As GroupMessage
            Get
                Return m_groupMessage
            End Get
        End Property
    End Class

End Namespace
