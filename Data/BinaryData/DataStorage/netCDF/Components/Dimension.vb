Imports System.Xml.Serialization

Namespace netCDF.Components

    ''' <summary>
    ''' ``[name => size]``
    ''' </summary>
    ''' 
    <XmlType("dim", [Namespace]:=Xml.netCDF)>
    Public Structure Dimension

        ''' <summary>
        ''' String with the name of the dimension
        ''' </summary>
        <XmlAttribute> Dim name As String
        ''' <summary>
        ''' Number with the size of the dimension
        ''' </summary>
        <XmlText>
        Dim size As Integer

        Public Overrides Function ToString() As String
            Return $"{name}(size={size})"
        End Function

        Public Shared ReadOnly Property [Double] As Dimension
            Get
                Return New Dimension With {.name = GetType(Double).FullName, .size = 8}
            End Get
        End Property

        Public Shared ReadOnly Property [Long] As Dimension
            Get
                Return New Dimension With {.name = GetType(Long).FullName, .size = 8}
            End Get
        End Property

        Public Shared ReadOnly Property Float As Dimension
            Get
                Return New Dimension With {.name = GetType(Single).FullName, .size = 4}
            End Get
        End Property

        Public Shared ReadOnly Property [Short] As Dimension
            Get
                Return New Dimension With {.name = GetType(Short).FullName, .size = 2}
            End Get
        End Property

        Public Shared ReadOnly Property [Integer] As Dimension
            Get
                Return New Dimension With {.name = GetType(Integer).FullName, .size = 4}
            End Get
        End Property

        Public Shared ReadOnly Property [Byte] As Dimension
            Get
                Return New Dimension With {.name = GetType(Byte).FullName, .size = 1}
            End Get
        End Property

        Public Shared ReadOnly Property [Boolean] As Dimension
            Get
                Return New Dimension With {.name = GetType(Boolean).FullName, .size = 1}
            End Get
        End Property

        Public Shared ReadOnly Property Text(fixedChars As Integer) As Dimension
            Get
                Return New Dimension With {.name = GetType(String).FullName, .size = fixedChars}
            End Get
        End Property
    End Structure

End Namespace