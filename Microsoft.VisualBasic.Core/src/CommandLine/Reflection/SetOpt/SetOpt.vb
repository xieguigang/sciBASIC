Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace CommandLine.Reflection

    Public Module SetOpt

        Public Function CreateOpt(Of T As {New, Class})(args As CommandLine) As T
            Dim obj As Object = New T
            Dim data As PropertyInfo() = DataFramework.Schema(Of T)(PropertyAccess.Writeable, nonIndex:=True, binds:=PublicProperty).Values.ToArray
            Dim objVal As Object

            For Each field As PropertyInfo In data
                Dim opt As OptAttribute = field.GetCustomAttribute(Of OptAttribute)

                If opt Is Nothing Then
                    Continue For
                End If

                Dim val As String = opt(args)

                If val Is Nothing Then
                    Continue For
                Else
                    objVal = Scripting.CTypeDynamic(val, field.PropertyType)
                    field.SetValue(obj, objVal)
                End If
            Next

            Return obj
        End Function

    End Module
End Namespace