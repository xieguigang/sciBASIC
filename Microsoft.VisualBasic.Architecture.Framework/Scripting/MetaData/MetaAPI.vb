Imports System.Reflection
Imports System.Runtime.CompilerServices

Namespace Scripting.MetaData

    Public Module MetaAPI

        <Extension> Public Function GetCLIMod(assm As Assembly) As Type
            Dim types As Type() = assm.GetTypes
            Dim LQuery = (From type As Type In types
                          Let attrs As Object() = type.GetCustomAttributes(PackageNamespace.TypeInfo, inherit:=True)
                          Where Not attrs.IsNullOrEmpty
                          Let attr = DirectCast(attrs(Scan0), PackageNamespace)
                          Select attr, type)
            Dim GetCLI = (From x In LQuery
                          Where x.attr.Category = APICategories.CLI_MAN
                          Select x.type).FirstOrDefault
            Return GetCLI
        End Function

        Public Function GetCLIMod(path As String) As Type
            Try
                Dim assm As Assembly = Assembly.LoadFile(path)
                Return assm.GetCLIMod
            Catch ex As Exception
                ex = New Exception(path.ToFileURL, ex)
                Call App.LogException(ex)
                Return Nothing
            End Try
        End Function
    End Module
End Namespace