Namespace BSON

    Public Enum ValueType As Byte
        [Double] = &H1
        [String] = &H2
        Document = &H3
        Array = &H4
        Binary = &H5
        [Boolean] = &H8
        UTCDateTime = &H9
        None = &HA
        Int32 = &H10
        Int64 = &H12
        [Object] = Document
    End Enum
End Namespace