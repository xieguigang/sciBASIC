Imports System
Imports System.ComponentModel

Namespace Microsoft.VisualBasic.CompilerServices
    <AttributeUsage(AttributeTargets.Parameter, Inherited:=False, AllowMultiple:=False), EditorBrowsable(EditorBrowsableState.Never), DynamicallyInvokableAttribute> _
    Public NotInheritable Class OptionCompareAttribute
        Inherits Attribute
    End Class
End Namespace

