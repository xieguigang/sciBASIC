Imports System
Imports System.Reflection

Namespace Microsoft.VisualBasic.CompilerServices
    Friend Interface IRecordEnum
        ' Methods
        Function Callback(FieldInfo As FieldInfo, ByRef Value As Object) As Boolean
    End Interface
End Namespace

