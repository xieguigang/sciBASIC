Namespace org.renjin.hdf5.message



    ''' <summary>
    ''' Each "old style" group has a v1 B-tree and a local heap for storing symbol table entries,
    ''' which are located with this message.
    ''' </summary>
    Public Class SymbolTableMessage
        Inherits Message

        Public Const MESSAGE_TYPE As Integer = &H11
        Private ReadOnly bTreeAddress As Long

        'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public SymbolTableMessage(org.renjin.hdf5.HeaderReader reader) throws java.io.IOException
        Public Sub New(reader As org.renjin.hdf5.HeaderReader)
            bTreeAddress = reader.readOffset()
            LocalHeapAddress = reader.readOffset()
        End Sub

        Public Overridable Function getbTreeAddress() As Long
            Return bTreeAddress
        End Function

        Public Overridable Property LocalHeapAddress As Long

    End Class

End Namespace