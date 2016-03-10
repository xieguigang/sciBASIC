Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection

Namespace CommandLine

    ''' <summary>
    ''' 从可写属性之中赋值
    ''' </summary>
    Public Module CLIMapper

        <Extension>
        Public Function Maps(Of T As Class)(args As CommandLine, Optional strict As Boolean = False) As T
            Dim type As Type = GetType(T)
            Dim obj As Object = Activator.CreateInstance(type)
            Dim props = (From prop As PropertyInfo
                         In type.GetProperties
                         Where prop.CanWrite AndAlso
                             Scripting.CasterString.ContainsKey(prop.PropertyType)
                         Select prop)

            For Each prop As PropertyInfo In props
                Dim name As String = prop.GetName

                If Not args.ContainsParameter(name, Not strict) Then
                    Continue For
                End If

                If prop.PropertyType.Equals(GetType(Boolean)) Then
                    Dim b As Boolean = True  ' 由于是逻辑值，所以只要存在就是真
                    Call prop.SetValue(obj, b)
                Else
                    Dim cast As Func(Of String, Object) = Scripting.CasterString(prop.PropertyType)
                    Dim s As String = args(name)
                    Dim value As Object = cast(s)
                    Call prop.SetValue(obj, value)
                End If
            Next

            Return DirectCast(obj, T)
        End Function

        <Extension>
        Public Function GetName(prop As PropertyInfo) As String
            Dim attr As CLIParameter = prop.GetAttribute(Of CLIParameter)
            If attr Is Nothing Then
                Return prop.Name
            Else
                Return attr.Name
            End If
        End Function
    End Module
End Namespace