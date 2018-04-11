Imports System.ComponentModel
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting

Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' Property default value
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class DefaultAttribute : Inherits DefaultValueAttribute

        Sub New(value As Object)
            Call MyBase.New(value)
        End Sub

        Public Overrides Function ToString() As String
            Return InputHandler.ToString(Value)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function GetDefaultValues(type As Type) As Dictionary(Of String, Object)
            Return type.Schema(PropertyAccess.Readable, PublicProperty, nonIndex:=True) _
                       .ToDictionary(Function(p) p.Value.Name,
                                     Function(p)
                                         Return GetDefaultAttribute(p.Value)?.Value
                                     End Function)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function GetDefaultAttribute([property] As PropertyInfo) As DefaultValueAttribute
            Return [property] _
                .GetCustomAttributes(Of DefaultValueAttribute)(inherit:=True) _
                .FirstOrDefault
        End Function
    End Class
End Namespace