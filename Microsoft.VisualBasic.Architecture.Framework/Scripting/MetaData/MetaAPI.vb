Imports System.Reflection
Imports System.Runtime.CompilerServices

Namespace Scripting.MetaData

    Public Module MetaAPI

        ''' <summary>
        ''' <see cref="PackageNamespace"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property TypeInfo As Type =
            GetType(PackageNamespace)

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

        ''' <summary>
        ''' 获取定义在类型定义上面的命名空间的标记信息
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function GetEntry(type As Type) As PackageNamespace
#If NET_40 = 0 Then
            Dim attrs = type.GetCustomAttributes(Of PackageNamespace)(inherit:=True)
            Return attrs.FirstOrDefault
#Else
            Throw New NotSupportedException
#End If
        End Function
    End Module
End Namespace