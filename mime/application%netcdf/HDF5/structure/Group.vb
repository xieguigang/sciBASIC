
'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 


Imports Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO

Namespace HDF5.[Structure]


    Public Class Group

        Private Shared NESTED_OBJECTS As New List(Of DataObjectFacade)()

        Private m_facade As DataObjectFacade

        Public Sub New([in] As BinaryReader, sb As Superblock, facade As DataObjectFacade)
            Me.m_facade = facade

            If facade.dataObject.groupMessage IsNot Nothing Then
                Dim gm As GroupMessage = facade.dataObject.groupMessage
                readGroup([in], sb, gm.bTreeAddress, gm.nameHeapAddress)
            End If
        End Sub

        Private Sub readGroup([in] As BinaryReader, sb As Superblock, bTreeAddress As Long, nameHeapAddress As Long)
            Dim nameHeap As New LocalHeap([in], sb, nameHeapAddress)
            Dim btree As New GroupBTree([in], sb, bTreeAddress)

            For Each s As SymbolTableEntry In btree.symbolTableEntries
                Dim sname As String = nameHeap.getString(CInt(s.linkNameOffset))
                If s.cacheType = 2 Then
                    Dim linkName As String = nameHeap.getString(CInt(s.linkNameOffset))
                    Dim dobj As New DataObjectFacade([in], sb, sname, linkName)
                    NESTED_OBJECTS.Add(dobj)
                Else
                    Dim dobj As New DataObjectFacade([in], sb, sname, s.objectHeaderAddress)
                    NESTED_OBJECTS.Add(dobj)
                End If
            Next
        End Sub

        Public Overridable ReadOnly Property objects() As List(Of DataObjectFacade)
            Get
                Return NESTED_OBJECTS
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("Group >>>")

            If NESTED_OBJECTS IsNot Nothing Then
                For Each dobj As DataObjectFacade In NESTED_OBJECTS
                    dobj.printValues()
                Next
            End If

            Console.WriteLine("Group <<<")
        End Sub
    End Class

End Namespace
