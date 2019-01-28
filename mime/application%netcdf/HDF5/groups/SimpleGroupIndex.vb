Imports System.Collections.Generic

Namespace org.renjin.hdf5.groups



	Public Class SimpleGroupIndex
		Implements GroupIndex

	  Private file As org.renjin.hdf5.Hdf5Data
        Private links As New List(Of org.renjin.hdf5.message.LinkMessage)()

        Public Sub New(file As org.renjin.hdf5.Hdf5Data, messages As IEnumerable(Of org.renjin.hdf5.message.LinkMessage))
		Me.file = file
		org.renjin.repackaged.guava.collect.Iterables.addAll(links, messages)
	  End Sub

        'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public org.renjin.hdf5.DataObject getObject(String name) throws java.io.IOException
        Public Function getObject(name As String) As org.renjin.hdf5.DataObject Implements GroupIndex.getObject
            For Each link As org.renjin.hdf5.message.LinkMessage In links
                If link.LinkName.Equals(name) Then
                    Return file.objectAt(link.Address)
                End If
            Next link
            Throw New System.ArgumentException("No such link: " & name)
        End Function
    End Class

End Namespace