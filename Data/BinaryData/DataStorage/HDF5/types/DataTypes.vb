#Region "Microsoft.VisualBasic::6c0d72db989da9a34a2cdc948109dfa5, Data\BinaryData\DataStorage\HDF5\types\DataTypes.vb"

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

    '     Class NamespaceDoc
    ' 
    ' 
    ' 
    '     Enum DataTypes
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace HDF5.type

    ''' <summary>
    ''' The HDF5 primitive data types model, classes representing the data types of HDF5 datasets.
    ''' </summary>
    Friend Class NamespaceDoc
    End Class

    ''' <summary>
    ''' 对一些在HDF5文件之中的基础数据类型的枚举，例如长整型，双精度，字符串等
    ''' </summary>
    Public Enum DataTypes
        ''' <summary>
        ''' Fixed-Point
        ''' </summary>
        DATATYPE_FIXED_POINT = 0
        ''' <summary>
        ''' Floating-point
        ''' </summary>
        DATATYPE_FLOATING_POINT = 1
        ''' <summary>
        ''' Time
        ''' </summary>
        DATATYPE_TIME = 2
        ''' <summary>
        ''' String
        ''' </summary>
        DATATYPE_STRING = 3
        ''' <summary>
        ''' Bit field
        ''' </summary>
        DATATYPE_BIT_FIELD = 4
        ''' <summary>
        ''' Opaque
        ''' </summary>
        DATATYPE_OPAQUE = 5
        ''' <summary>
        ''' Compound
        ''' </summary>
        DATATYPE_COMPOUND = 6
        ''' <summary>
        ''' Reference
        ''' </summary>
        DATATYPE_REFERENCE = 7
        ''' <summary>
        ''' Enumerated
        ''' </summary>
        DATATYPE_ENUMS = 8
        ''' <summary>
        ''' Variable-Length
        ''' </summary>
        DATATYPE_VARIABLE_LENGTH = 9
        ''' <summary>
        ''' Array
        ''' </summary>
        DATATYPE_ARRAY = 10
    End Enum
End Namespace
