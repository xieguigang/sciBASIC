Namespace org.renjin.hdf5



	''' <summary>
	''' Entry point for Renjin-specific functionality.
	''' </summary>
	Public Class RenjinHdf5

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public static org.renjin.sexp.Vector readArray(@Current org.renjin.eval.Context context, String file, String objectName) throws java.io.IOException
'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
		Public Shared Function readArray(ByVal context As org.renjin.eval.Context, ByVal file As String, ByVal objectName As String) As org.renjin.sexp.Vector

			Dim fileObject As org.apache.commons.vfs2.FileObject = context.resolveFile(file)
			If Not(TypeOf fileObject Is org.apache.commons.vfs2.provider.local.LocalFile) Then
				Throw New org.renjin.eval.EvalException("Can only open HDF files from the local file system.")
			End If
			Dim localFile As org.apache.commons.vfs2.provider.local.LocalFile = CType(fileObject, org.apache.commons.vfs2.provider.local.LocalFile)
			If Not localFile.exists() Then
				Throw New org.renjin.eval.EvalException("%s does not exist", localFile)
			End If

			Dim url As java.net.URL = localFile.URL

			Dim hdf5 As New Hdf5File(New File(url.File))

			Dim [object] As DataObject = hdf5.getObject(objectName)
			Dim datatype As org.renjin.hdf5.message.DatatypeMessage = [object].getMessage(GetType(org.renjin.hdf5.message.DatatypeMessage))
			If Not datatype.DoubleIEE754 Then
				Throw New org.renjin.eval.EvalException("Unsupported data type. Currently only 64-bit floating point is implemented")
			End If

			Dim dataset As New org.renjin.hdf5.vector.ChunkedDataset(hdf5, [object])
			Return New org.renjin.hdf5.vector.ChunkedDoubleVector(dataset)
		End Function

	End Class

End Namespace