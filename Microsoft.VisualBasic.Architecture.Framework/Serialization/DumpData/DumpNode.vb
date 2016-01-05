Imports System.Data.Linq.Mapping

<AttributeUsage(AttributeTargets.Property Or AttributeTargets.Field, allowmultiple:=False, inherited:=True)>
Public Class DumpNode : Inherits Attribute
    Public Shared ReadOnly [GetTypeId] As System.Type = GetType(DumpNode)
End Class