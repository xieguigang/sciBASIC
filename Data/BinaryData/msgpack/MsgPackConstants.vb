
Friend Module MsgPackConstants

    Public Const MAX_PROPERTY_COUNT As Integer = 15

    Public NotInheritable Class Formats
        Public Const NIL As Byte = &HC0
        Public Const FLOAT_32 As Byte = &HCA
        Public Const FLOAT_64 As Byte = &HCB
        Public Const [DOUBLE] As Byte = &HCB
        Public Const UINT_8 As Byte = &HCC
        Public Const UNSIGNED_INTEGER_8 As Byte = &HCC
        Public Const UINT_16 As Byte = &HCD
        Public Const UNSIGNED_INTEGER_16 As Byte = &HCD
        Public Const UINT_32 As Byte = &HCE
        Public Const UNSIGNED_INTEGER_32 As Byte = &HCE
        Public Const UINT_64 As Byte = &HCF
        Public Const UNSIGNED_INTEGER_64 As Byte = &HCF
        Public Const INT_8 As Byte = &HD0
        Public Const INTEGER_8 As Byte = &HD0
        Public Const INT_16 As Byte = &HD1
        Public Const INTEGER_16 As Byte = &HD1
        Public Const INT_32 As Byte = &HD2
        Public Const INTEGER_32 As Byte = &HD2
        Public Const INT_64 As Byte = &HD3
        Public Const INTEGER_64 As Byte = &HD3
        Public Const STR_8 As Byte = &HD9
        Public Const STRING_8 As Byte = &HD9
        Public Const STR_16 As Byte = &HDA
        Public Const STRING_16 As Byte = &HDA
        Public Const STR_32 As Byte = &HDB
        Public Const STRING_32 As Byte = &HDB
        Public Const ARRAY_16 As Byte = &HDC
        Public Const ARRAY_32 As Byte = &HDD
        Public Const MAP_16 As Byte = &HDE
        Public Const MAP_32 As Byte = &HDF
    End Class

    Public NotInheritable Class FixedInteger
        Public Const POSITIVE_MIN As Byte = &H0
        Public Const POSITIVE_MAX As Byte = &H7F
        Public Const NEGATIVE_MIN As Byte = &HE0
        Public Const NEGATIVE_MAX As Byte = &HFF
    End Class

    Public NotInheritable Class FixedString
        Public Const MIN As Byte = &HA0
        Public Const MAX As Byte = &HBF
        Public Const MAX_LENGTH As Integer = 31
    End Class

    Public NotInheritable Class FixedMap
        Public Const MIN As Byte = &H80
        Public Const MAX As Byte = &H8F
    End Class

    Public NotInheritable Class FixedArray
        Public Const MIN As Byte = &H90
        Public Const MAX As Byte = &H9F
    End Class

    Public NotInheritable Class Bool
        Public Const [FALSE] As Byte = &HC2
        Public Const [TRUE] As Byte = &HC3
    End Class
End Module
