Imports System
Imports System.ComponentModel

Namespace Microsoft.VisualBasic.ApplicationServices
    <TypeConverter(GetType(BuiltInRoleConverter))> _
    Public Enum BuiltInRole
        ' Fields
        AccountOperator = &H224
        Administrator = &H220
        BackupOperator = &H227
        Guest = &H222
        PowerUser = &H223
        PrintOperator = 550
        Replicator = &H228
        SystemOperator = &H225
        User = &H221
    End Enum
End Namespace

