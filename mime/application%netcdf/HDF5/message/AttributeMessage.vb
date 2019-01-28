Namespace org.renjin.hdf5.message


	''' <summary>
	''' The Attribute message is used to store objects in the HDF5 file which are used as attributes, or "metadata" about
	''' the current object. An attribute is a small dataset; it has a name, a datatype, a dataspace, and raw data.
	'''  Since attributes are stored in the object header, they should be relatively small (in other words, less than 64KB).
	'''  They can be associated with any type of object which has an object header (groups, datasets, or
	'''  committed (named) datatypes).
	''' 
	''' <p>In 1.8.x versions of the library, attributes can be larger than 64KB. See the "Special Issues" section of the
	''' Attributes chapter in the HDF5 User’s Guide for more information.
	''' 
	''' <p>Note: Attributes on an object must have unique names: the HDF5 Library currently enforces this by causing the
	''' creation of an attribute with a duplicate name to fail. Attributes on different objects may have the same name,
	''' however.
	''' </summary>
	Public Class AttributeMessage
		Inherits Message

	  Public Const MESSAGE_TYPE As Integer = &HC

	  Public Sub New(ByVal reader As org.renjin.hdf5.HeaderReader)

	  End Sub
	End Class

End Namespace