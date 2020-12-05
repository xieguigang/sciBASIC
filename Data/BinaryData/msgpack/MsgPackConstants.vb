
Namespace scopely.msgpacksharp
    Friend Module MsgPackConstants
        Public Const MAX_PROPERTY_COUNT As Integer = 15

        Public NotInheritable Class Formats
            Public Const NIL As Byte = &Hc0
            Public Const FLOAT_32 As Byte = &Hca
            Public Const FLOAT_64 As Byte = &Hcb
            Public Const [DOUBLE] As Byte = &Hcb
            Public Const UINT_8 As Byte = &Hcc
            Public Const UNSIGNED_INTEGER_8 As Byte = &Hcc
            Public Const UINT_16 As Byte = &HcD
            Public Const UNSIGNED_INTEGER_16 As Byte = &HcD
            Public Const UINT_32 As Byte = &Hce
            Public Const UNSIGNED_INTEGER_32 As Byte = &Hce
            Public Const UINT_64 As Byte = &HcF
            Public Const UNSIGNED_INTEGER_64 As Byte = &HcF
            Public Const INT_8 As Byte = &Hd0
            Public Const INTEGER_8 As Byte = &Hd0
            Public Const INT_16 As Byte = &Hd1
            Public Const INTEGER_16 As Byte = &Hd1
            Public Const INT_32 As Byte = &Hd2
            Public Const INTEGER_32 As Byte = &Hd2
            Public Const INT_64 As Byte = &Hd3
            Public Const INTEGER_64 As Byte = &Hd3
            Public Const STR_8 As Byte = &Hd9
            Public Const STRING_8 As Byte = &Hd9
            Public Const STR_16 As Byte = &Hda
            Public Const STRING_16 As Byte = &Hda
            Public Const STR_32 As Byte = &Hdb
            Public Const STRING_32 As Byte = &Hdb
            Public Const ARRAY_16 As Byte = &Hdc
            Public Const ARRAY_32 As Byte = &HdD
            Public Const MAP_16 As Byte = &Hde
            Public Const MAP_32 As Byte = &HdF
        End Class

        Public NotInheritable Class FixedInteger
            Public Const POSITIVE_MIN As Byte = &H00
            Public Const POSITIVE_MAX As Byte = &H7F
            Public Const NEGATIVE_MIN As Byte = &He0
            Public Const NEGATIVE_MAX As Byte = &HfF
        End Class

        Public NotInheritable Class FixedString
            Public Const MIN As Byte = &Ha0
            Public Const MAX As Byte = &HbF
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
            Public Const [FALSE] As Byte = &Hc2
            Public Const [TRUE] As Byte = &Hc3
        End Class
    End Module
End Namespace
