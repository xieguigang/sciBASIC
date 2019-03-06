Imports System.Collections.Generic

'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 


Namespace HDF5.[Structure]
	Public Class ObjectHeaderMessageType
		Private Shared MAX_MESSAGE As Integer = 23
		Private Shared hash As IDictionary(Of String, ObjectHeaderMessageType) = New Dictionary(Of String, ObjectHeaderMessageType)(10)
		Private Shared mess As ObjectHeaderMessageType() = New ObjectHeaderMessageType(MAX_MESSAGE - 1) {}

		Public Shared ReadOnly NIL As New ObjectHeaderMessageType("NIL", 0)
		Public Shared ReadOnly SimpleDataspace As New ObjectHeaderMessageType("SimpleDataspace", 1)
		Public Shared ReadOnly GroupNew As New ObjectHeaderMessageType("GroupNew", 2)
		Public Shared ReadOnly Datatype As New ObjectHeaderMessageType("Datatype", 3)
		Public Shared ReadOnly FillValueOld As New ObjectHeaderMessageType("FillValueOld", 4)
		Public Shared ReadOnly FillValue As New ObjectHeaderMessageType("FillValue", 5)
		Public Shared ReadOnly Link As New ObjectHeaderMessageType("Link", 6)
		Public Shared ReadOnly ExternalDataFiles As New ObjectHeaderMessageType("ExternalDataFiles", 7)
		Public Shared ReadOnly Layout As New ObjectHeaderMessageType("Layout", 8)
		Public Shared ReadOnly GroupInfo As New ObjectHeaderMessageType("GroupInfo", 10)
		Public Shared ReadOnly FilterPipeline As New ObjectHeaderMessageType("FilterPipeline", 11)
		Public Shared ReadOnly Attribute As New ObjectHeaderMessageType("Attribute", 12)
		Public Shared ReadOnly Comment As New ObjectHeaderMessageType("Comment", 13)
		Public Shared ReadOnly LastModifiedOld As New ObjectHeaderMessageType("LastModifiedOld", 14)
		Public Shared ReadOnly SharedObject As New ObjectHeaderMessageType("SharedObject", 15)
		Public Shared ReadOnly ObjectHeaderContinuation As New ObjectHeaderMessageType("ObjectHeaderContinuation", 16)
		Public Shared ReadOnly Group As New ObjectHeaderMessageType("Group", 17)
		Public Shared ReadOnly LastModified As New ObjectHeaderMessageType("LastModified", 18)
		Public Shared ReadOnly AttributeInfo As New ObjectHeaderMessageType("AttributeInfo", 21)
		Public Shared ReadOnly ObjectReferenceCount As New ObjectHeaderMessageType("ObjectReferenceCount", 22)

		Private name As String

        Private num_Renamed As Integer

		Private Sub New(name As String, num As Integer)
			Me.name = name
			Me.num_Renamed = num
			hash(name) = Me
			mess(num) = Me
		End Sub

		''' <summary>
		''' Find the MessageType that matches this name.
		''' </summary>
		''' <param name="name"> find DataTYpe with this name. </param>
		''' <returns> DataType or null if no match. </returns>
		Public Shared Overloads Function [getType](name As String) As ObjectHeaderMessageType
			If String.ReferenceEquals(name, Nothing) Then
				Return Nothing
			End If
			Return hash.GetValueOrNull(name)
		End Function

		''' <summary>
		''' Get the MessageType by number.
		''' </summary>
		''' <param name="num"> message number. </param>
		''' <returns> the MessageType </returns>
		Public Shared Overloads Function [getType](num As Integer) As ObjectHeaderMessageType
			If (num < 0) OrElse (num >= MAX_MESSAGE) Then
				Return Nothing
			End If
			Return mess(num)
		End Function

		''' <summary>
		''' Message name.
		''' </summary>
		Public Overrides Function ToString() As String
			Return name & "(" & num_Renamed & ")"
		End Function

		''' <returns> Message number. </returns>
		Public Overridable ReadOnly Property num() As Integer
			Get
				Return num_Renamed
			End Get
		End Property
	End Class
End Namespace
