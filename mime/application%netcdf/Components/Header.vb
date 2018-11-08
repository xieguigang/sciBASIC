Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Language

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
    Public Property recordDimension As recordDimension
    ''' <summary>
    ''' List of dimensions
    ''' </summary>
    ''' <returns></returns>
    Public Property dimensions As Dimension()
    ''' <summary>
    ''' List of global attributes
    ''' </summary>
    ''' <returns></returns>
    Public Property globalAttributes As attribute()
    ''' <summary>
    ''' List of variables
    ''' </summary>
    ''' <returns></returns>
    Public Property variables As variable()
    Public Property version As Byte

    Sub New()
    End Sub

    ''' <summary>
    ''' Read the header of the file
    ''' </summary>
    ''' <param name="buffer">Buffer for the file data</param>
    ''' <param name="version">Version of the file</param>
    Friend Sub New(buffer As BinaryDataReader, version As Byte)
        ' Length of record dimension
        ' sum of the varSize's of all the record variables.
        Me.recordDimension = New recordDimension With {.length = buffer.ReadUInt32}
        Me.version = version

        ' List of dimensions
        Dim dimList = dimensionsList(buffer)

        Me.recordDimension.id = dimList.recordId
        Me.recordDimension.name = dimList.recordName
        Me.dimensions = dimList.dimensions

        ' List of global attributes
        Me.globalAttributes = attributesList(buffer)

        ' List of variables
        Dim variables = variablesList(buffer, dimList.recordId, version)
        Me.variables = variables.variables
        Me.recordDimension.recordStep = variables.recordStep
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
    Private Shared Function dimensionsList(buffer As BinaryDataReader) As DimensionList
        Dim dimList = buffer.ReadUInt32()

        If (dimList = ZERO) Then
            Utils.notNetcdf((buffer.ReadUInt32() <> ZERO), "wrong empty tag for list of dimensions")
            Return Nothing
        Else
            Utils.notNetcdf((dimList <> NC_DIMENSION), "wrong tag for list of dimensions")
        End If

        Dim recordId%? = Nothing
        Dim recordName$ = "NA"
        ' Length of dimensions
        Dim dimensionSize = buffer.ReadUInt32()
        Dim dimensions = New List(Of Dimension)

        For [dim] As Integer = 0 To dimensionSize - 1
            ' Read name
            Dim name = Utils.readName(buffer)
            ' Read dimension size
            Dim size = buffer.ReadUInt32()

            If (size = NC_UNLIMITED) Then
                ' In netcdf 3 one field can be Of size unlimmited
                recordId = [dim]
                recordName = name
            End If

            dimensions += New Dimension With {
                .name = name,
                .size = size
            }
        Next

        Return New DimensionList With {
            .dimensions = dimensions,
            .recordId = recordId,
            .recordName = recordName
        }
    End Function

    ''' <summary>
    ''' List of attributes
    ''' </summary>
    ''' <param name="buffer">Buffer for the file data</param>
    ''' <returns>
    ''' List of attributes with:
    ''' 
    '''  + `name` String with the name of the attribute
    '''  + `type`: String with the type of the attribute
    '''  + `value`: A number Or String With the value Of the attribute
    '''  
    ''' </returns>
    Private Shared Function attributesList(buffer) As attribute()
        Dim gAttList = buffer.readUint32()

        If (gAttList = ZERO) Then
            Utils.notNetcdf((buffer.readUint32() <> ZERO), "wrong empty tag for list of attributes")
            Return {}
        Else
            Utils.notNetcdf((gAttList <> NC_ATTRIBUTE), "wrong tag for list of attributes")
        End If

        ' Length of attributes
        Dim attributeSize = buffer.readUint32()
        Dim attributes As New List(Of attribute)

        For gAtt As Integer = 0 To attributeSize - 1
            ' Read name
            Dim name = Utils.readName(buffer)
            ' Read type
            Dim type = buffer.readUint32()

            Utils.notNetcdf(((type < 1) Or (type > 6)), $"non valid type {type}")

            ' Read attribute
            Dim size = buffer.readUint32()
            Dim Value = TypeExtensions.readType(buffer, type, size)

            ' Apply padding
            Utils.padding(buffer)

            attributes += New attribute With {
                .name = name,
                .type = TypeExtensions.num2str(type),
                .value = Value
            }
        Next

        Return attributes
    End Function

    ''' <summary>
    ''' List of variables
    ''' </summary>
    ''' <param name="buffer">Buffer for the file data</param>
    ''' <param name="recordId%">Id of the unlimited dimension (also called record dimension)
    ''' This value may be undefined if there Is no unlimited dimension</param>
    ''' <param name="version">Version of the file</param>
    ''' <returns>
    ''' Number of recordStep And list of variables with:
    ''' 
    '''  + `name`: String with the name of the variable
    '''  + `dimensions`: Array with the dimension IDs of the variable
    '''  + `attributes`: Array with the attributes of the variable
    '''  + `type`: String with the type of the variable
    '''  + `size`: Number with the size of the variable
    '''  + `offset`: Number with the offset where of the variable begins
    '''  + `record`: True if Is a record variable, false otherwise (unlimited size)
    '''  </returns>
    Private Shared Function variablesList(buffer As BinaryDataReader, recordId As Integer?, version As Byte) As (variables As variable(), recordStep As Integer)
        Dim varList = buffer.ReadUInt32()
        Dim recordStep = 0

        If (varList = ZERO) Then
            Utils.notNetcdf((buffer.ReadUInt32() <> ZERO), "wrong empty tag for list of variables")
            Return Nothing
        Else
            Utils.notNetcdf((varList <> NC_VARIABLE), "wrong tag for list of variables")
        End If

        ' Length of variables
        Dim variableSize = buffer.ReadUInt32()
        Dim variables As New List(Of variable)

        For v As Integer = 0 To variableSize - 1
            ' Read name
            Dim name = Utils.readName(buffer)
            ' Read dimensionality of the variable
            Dim dimensionality = buffer.ReadUInt32()
            ' Index into the list of dimensions
            Dim dimensionsIds = New Integer(dimensionality - 1) {}

            For [dim] As Integer = 0 To dimensionality - 1
                dimensionsIds([dim]) = buffer.ReadUInt32()
            Next

            ' Read variables size
            Dim attributes = attributesList(buffer)
            ' Read type
            Dim type = buffer.ReadUInt32()

            Utils.notNetcdf(((type < 1) AndAlso (type > 6)), $"non valid type {type}")

            ' Read variable size
            ' The 32-bit varSize field Is Not large enough to contain the size of variables that require
            ' more than 2^32 - 4 bytes, so 2^32 - 1 Is used in the varSize field for such variables.
            Dim varSize = buffer.ReadUInt32()
            ' Read offset
            Dim offset = buffer.ReadUInt32()

            If (version = 2) Then
                Utils.notNetcdf((offset > 0), "offsets larger than 4GB not supported")
                offset = buffer.ReadUInt32()
            End If

            Dim record = False

            ' Count amount of record variables
            If ((recordId IsNot Nothing) AndAlso (dimensionsIds(0) = recordId)) Then
                recordStep += varSize
                record = True
            End If

            variables += New variable With {
                .name = name,
                .dimensions = dimensionsIds,
                .attributes = attributes,
                .type = TypeExtensions.num2str(type),
                .size = varSize,
                .offset = offset,
                .record = record
            }
        Next

        Return (variables:=variables, recordStep:=recordStep)
    End Function
End Class
