Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Linq

    ''' <summary>
    ''' Set value linq expression helper
    ''' </summary>
    Public Class SetValue(Of T)

        ReadOnly __type As Type = GetType(T)
        ReadOnly __props As SortedDictionary(Of String, PropertyInfo)

        Sub New()
            __props = New SortedDictionary(Of String, PropertyInfo)(
                DataFramework.Schema(Of T)(
                PropertyAccessibilityControls.Writeable))
        End Sub

        Public Overrides Function ToString() As String
            Return __type.ToString
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name">Using NameOf</param>
        ''' <returns></returns>
        Public Function GetSet(name As String) As Func(Of T, Object, T)
            Dim setValue As PropertyInfo = __props(name)
            Return Function(x, v)
                       Call setValue.SetValue(x, v)
                       Return x
                   End Function
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="name">Using NameOf</param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Function InvokeSetValue(x As T, name As String, value As Object) As T
            Dim setValue As PropertyInfo = __props(name)
            Call setValue.SetValue(x, value)
            Return x
        End Function
    End Class
End Namespace