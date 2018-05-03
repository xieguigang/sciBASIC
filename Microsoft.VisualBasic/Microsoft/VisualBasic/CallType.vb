Imports System

Namespace Microsoft.VisualBasic
    <DynamicallyInvokableAttribute>
    Public Enum CallType
        ' Fields
        <DynamicallyInvokableAttribute>
        [Get] = 2
        <DynamicallyInvokableAttribute>
        [Let] = 4
        <DynamicallyInvokableAttribute>
        Method = 1
        <DynamicallyInvokableAttribute>
        [Set] = 8
    End Enum
End Namespace

