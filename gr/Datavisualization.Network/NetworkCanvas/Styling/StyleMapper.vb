

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream

Public Module StyleMapper

    Public Function GetProperty(Of T)() As Dictionary(Of String, Func(Of T, Object))
        Dim type As Type = GetType(T)
        Dim properties As PropertyInfo() = type _
            .GetProperties(PublicProperty) _
            .Where(Function(p) p.GetIndexParameters.IsNullOrEmpty) _
            .ToArray
        Dim out As New Dictionary(Of String, Func(Of T, Object))

        For Each prop As PropertyInfo In properties
            Dim getMethod = Emit.Delegates.PropertyGet(type, prop)
            Call out.Add(prop.Name, Function(x As T) getMethod(x))
        Next

        Return out
    End Function

    <Extension>
    Public Function NumericMapping(Of T As INetComponent)(source As IEnumerable(Of T), key$, range As DoubleRange) As Map(Of T, Double)

    End Function
End Module
