# REPARSE_DATA_BUFFER
_namespace: [Microsoft.VisualBasic.FileIO.SymLinker.JunctionPoint](./index.md)_






### Properties

#### PathBuffer
A buffer containing the unicode-encoded path string. The path string contains
 the substitute name string and print name string.
#### PrintNameLength
Length, in bytes, of the print name string. If this string is null-terminated,
 PrintNameLength does not include space for the null character.
#### PrintNameOffset
Offset, in bytes, of the print name string in the PathBuffer array.
#### ReparseDataLength
Size, in bytes, of the data after the Reserved member. This can be calculated by:
 (4 * sizeof(ushort)) + SubstituteNameLength + PrintNameLength +
 (namesAreNullTerminated ? 2 * sizeof(char) : 0);
#### ReparseTag
Reparse point tag. Must be a Microsoft reparse point tag.
#### Reserved
Reserved; do not use.
#### SubstituteNameLength
Length, in bytes, of the substitute name string. If this string is null-terminated,
 SubstituteNameLength does not include space for the null character.
#### SubstituteNameOffset
Offset, in bytes, of the substitute name string in the PathBuffer array.
