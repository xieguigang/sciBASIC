# uCharUnion
_namespace: [Microsoft.VisualBasic.Terminal.Utility](./index.md)_

' Struct uChar is meant to support the Windows Console API's uChar union.
 ' Unions do not exist in the pure .NET world. We have to use the regular
 ' C# struct and the StructLayout and FieldOffset Attributes to preserve
 ' the memory layout of the unmanaged union.
 '
 ' We specify the "LayoutKind.Explicit" value for the StructLayout attribute
 ' to specify that every field of the struct uChar is marked with a byte offset.
 '
 ' This byte offset is specified by the FieldOffsetAttribute and it indicates
 ' the number of bytes between the beginning of the struct in memory and the
 ' beginning of the field.
 '
 ' As you can see in the struct uChar (below), the fields "UnicodeChar"
 ' and "AsciiChar" have been marked as being of offset 0. This is the only
 ' way that an unmanaged C/C++ union can be represented in C#.
 '




