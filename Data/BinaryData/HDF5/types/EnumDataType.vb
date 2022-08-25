' 
'  This file is part of jHDF. A pure Java library for accessing HDF5 files.
' 
'  http://jhdf.io
' 
'  Copyright (c) 2022 James Mudd
' 
'  MIT License see 'LICENSE' file
' 
Namespace type


    ''' <summary>
    ''' Class for reading enum data type messages.
    ''' 
    ''' @author James Mudd
    ''' </summary>
    Public Class EnumDataType
        Inherits DataType

        Private ReadOnly baseTypeField As DataType
        Private ReadOnly ordinalToName As IDictionary(Of Integer?, String)

        Public Overridable ReadOnly Property BaseType As DataType
            Get
                Return baseTypeField
            End Get
        End Property

        Public Overridable ReadOnly Property EnumMapping As IDictionary(Of Integer?, String)
            Get
                Return ordinalToName
            End Get
        End Property

        Public Overrides ReadOnly Property TypeInfo As System.Type
            Get
                Return GetType(String)
            End Get
        End Property
    End Class

End Namespace
