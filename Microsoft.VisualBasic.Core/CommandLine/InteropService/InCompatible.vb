Imports System.Reflection

Namespace CommandLine.InteropService

    ''' <summary>
    ''' 这个CLI方法是和.NET的<see cref="System.Diagnostics.Process"/>调用不兼容的
    ''' </summary>
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public Class InCompatibleAttribute : Inherits Attribute

        ''' <summary>
        ''' 判断目标方法是否是和CLR调用兼容？
        ''' </summary>
        ''' <param name="CLI"></param>
        ''' <returns></returns>
        Public Shared Function CLRProcessCompatible(CLI As MethodInfo) As Boolean
            Dim attrs = CLI.GetCustomAttribute(Of InCompatibleAttribute)

            If attrs Is Nothing Then
                Return True
            Else
                Return False
            End If
        End Function

        Public Overrides Function ToString() As String
            Return "Incompatible with CLR Process Calls"
        End Function
    End Class
End Namespace