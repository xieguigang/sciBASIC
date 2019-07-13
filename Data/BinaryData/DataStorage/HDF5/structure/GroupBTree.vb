#Region "Microsoft.VisualBasic::446c2a782a96b83c27a4a88faaa10b2b, Data\BinaryData\DataStorage\HDF5\structure\GroupBTree.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class GroupBTree
    ' 
    '         Properties: magic, symbolTableEntries
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: printValues, readAllEntries
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
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct.BTree
Imports Microsoft.VisualBasic.Language
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace HDF5.struct

    ''' <summary>
    ''' 
    ''' </summary>
    Public Class GroupBTree : Inherits HDF5Ptr
        Implements IMagicBlock

        Public Const signature$ = "TREE"

        Public ReadOnly Property symbolTableEntries As List(Of SymbolTableEntry)
        Public ReadOnly Property magic As String Implements IMagicBlock.magic

        Public Sub New(sb As Superblock, address As Long)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            Me.symbolTableEntries = New List(Of SymbolTableEntry)()

            Dim entryList As New List(Of BTreeEntry)()
            Dim node As GroupNode

            Call readAllEntries(sb, address, entryList)

            For Each e As BTreeEntry In entryList
                node = New GroupNode(sb, e.targetAddress)
                symbolTableEntries.AddRange(node.symbols)
            Next
        End Sub

        Private Sub readAllEntries(sb As Superblock, address As Long, entryList As List(Of BTreeEntry))
            Dim [in] As BinaryReader = sb.FileReader(address)

            _magic = Encoding.ASCII.GetString([in].readBytes(4))

            If Not Me.VerifyMagicSignature(signature) Then
                Throw New IOException("signature is not valid")
            End If

            Dim type As Integer = [in].readByte()
            Dim level As Integer = [in].readByte()
            Dim entryNum As Integer = [in].readShort()

            Dim leftAddress As Long = ReadHelper.readO([in], sb)
            Dim rightAddress As Long = ReadHelper.readO([in], sb)
            Dim myEntries As New List(Of BTreeEntry)()

            For i As Integer = 0 To entryNum - 1
                myEntries.Add(New BTreeEntry(sb, [in].offset))
            Next

            If level = 0 Then
                entryList.AddRange(myEntries)
            Else
                For Each entry As BTreeEntry In myEntries
                    readAllEntries(sb, entry.targetAddress, entryList)
                Next
            End If
        End Sub

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("GroupBTree >>>")
            console.WriteLine("address : " & Me.m_address)

            For i As Integer = 0 To symbolTableEntries.Count - 1
                symbolTableEntries(i).printValues(console)
            Next

            console.WriteLine("GroupBTree <<<")
        End Sub
    End Class

End Namespace
