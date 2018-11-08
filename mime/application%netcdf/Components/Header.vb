Imports Microsoft.VisualBasic.Data.IO

''' <summary>
''' Object with the fields:
''' 
'''  + `recordDimension` Number with the length of record dimension
'''  + `dimensions`: List of dimensions
'''  + `globalAttributes`: List of global attributes
'''  + `variables`: List of variables
'''  
''' </summary>
Public Class Header

#Region "Grammar constants"
    Const ZERO = 0
    Const NC_DIMENSION = 10
    Const NC_VARIABLE = 11
    Const NC_ATTRIBUTE = 12
#End Region

    ''' <summary>
    ''' Number with the length of record dimension
    ''' </summary>
    ''' <returns></returns>
    Public Property recordDimension As Integer
    ''' <summary>
    ''' List of dimensions
    ''' </summary>
    ''' <returns></returns>
    Public Property dimensions
    ''' <summary>
    ''' List of global attributes
    ''' </summary>
    ''' <returns></returns>
    Public Property globalAttributes
    ''' <summary>
    ''' List of variables
    ''' </summary>
    ''' <returns></returns>
    Public Property variables
    Public Property version As Byte

    ''' <summary>
    ''' Read the header of the file
    ''' </summary>
    ''' <param name="buffer">Buffer for the file data</param>
    ''' <param name="version">Version of the file</param>
    Sub New(buffer As BinaryDataReader, version As Byte)
        ' Length of record dimension
        ' sum of the varSize's of all the record variables.
        Me.recordDimension = buffer.ReadUInt32
        Me.version = version

        ' List of dimensions
    End Sub

    Const NC_UNLIMITED = 0

    ''' <summary>
    ''' List of dimensions
    ''' </summary>
    ''' <param name="buffer">Buffer for the file data</param>
    ''' <returns>
    ''' Ojbect containing the following properties:
    ''' 
    '''  + `dimensions` that Is an array of dimension object
    '''  + `name` String with the name of the dimension
    '''  + `size`: Number with the size of the dimension dimensions: dimensions
    '''  + `recordId`: the id Of the dimension that has unlimited size Or undefined,
    '''  + `recordName`: name of the dimension that has unlimited size
    '''  
    ''' </returns>
    Public Function dimensionsList(buffer As BinaryDataReader)

    End Function

End Class
