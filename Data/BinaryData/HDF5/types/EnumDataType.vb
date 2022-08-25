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
    Public Class EnumDataType : Inherits DataType

        Public Property BaseType As DataType

        Public Property EnumMapping As IDictionary(Of Integer?, String)

        Public Overrides ReadOnly Property TypeInfo As System.Type
            Get
                Return GetType(String)
            End Get
        End Property
    End Class

End Namespace
