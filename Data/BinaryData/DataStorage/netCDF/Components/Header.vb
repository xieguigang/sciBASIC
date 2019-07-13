#Region "Microsoft.VisualBasic::8b8a45d8307c30f1fc663da7547ad78d, Data\BinaryData\DataStorage\netCDF\Components\Header.vb"

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

    '     Class Header
    ' 
    '         Properties: dimensions, globalAttributes, recordDimension, variables, version
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO

Namespace netCDF.Components

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
        Public Const NC_DIMENSION = 10
        Public Const NC_VARIABLE = 11
        Public Const NC_ATTRIBUTE = 12
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
        Public Property globalAttributes As Attribute()
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
            Dim dimList As DimensionList = buffer.dimensionsList()

            Me.recordDimension.id = If(dimList.recordId, dimList.recordId, -100)
            Me.recordDimension.name = dimList.recordName
            Me.dimensions = dimList.dimensions

            ' List of global attributes
            Me.globalAttributes = buffer.attributesList().ToArray

            ' List of variables
            Dim variables = buffer.variablesList(dimList.recordId, version)
            Me.variables = variables.variables
            Me.recordDimension.recordStep = variables.recordStep
        End Sub
    End Class
End Namespace
