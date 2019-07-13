#Region "Microsoft.VisualBasic::d0bd8ff98c817e8b3ab04032760e6d74, Microsoft.VisualBasic.Core\CommandLine\CLIMapper.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module CLIMapper
    ' 
    '         Function: GetName, Maps
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection

Namespace CommandLine

    ''' <summary>
    ''' 从可写属性之中赋值
    ''' </summary>
    Public Module CLIMapper

        ''' <summary>
        ''' Assign the argument value in the commandline into the target argument container object.
        ''' The properties in the container class type needs decorating with attribute
        ''' 
        ''' (这个拓展函数是将命令行对象反序列化为参数对象)
        ''' <see cref="Argv"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="args"></param>
        ''' <param name="strict"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Maps(Of T As Class)(args As CommandLine, Optional strict As Boolean = False) As T
            Dim type As Type = GetType(T)
            Dim obj As Object = Activator.CreateInstance(type)
            Dim props = From prop As PropertyInfo
                        In type.GetProperties
                        Where prop.CanWrite AndAlso
                            Scripting.IsPrimitive(prop.PropertyType)
                        Select prop

            For Each prop As PropertyInfo In props
                Dim name As String = prop.GetName

                If Not args.ContainsParameter(name, Not strict) Then
                    Continue For
                Else
                    type = prop.PropertyType
                End If

                If type.Equals(GetType(Boolean)) Then
                    ' 由于是逻辑值，所以只要存在就是真，不存在就是False
                    Call prop.SetValue(obj, True, Nothing)
                Else
                    Dim s As String = args(name)
                    Dim value = Scripting.CTypeDynamic(s, type)

                    Call prop.SetValue(obj, value, Nothing)
                End If
            Next

            Return DirectCast(obj, T)
        End Function

        ''' <summary>
        ''' If the property <paramref name="prop"/> have the custom attribute <see cref="Argv"/>
        ''' then the name value in <see cref="Argv"/> will be used, otherwise, 
        ''' <see cref="PropertyInfo.Name"/> will be used. 
        ''' </summary>
        ''' <param name="prop"></param>
        ''' <returns></returns>
        <Extension> Public Function GetName(prop As PropertyInfo) As String
            Dim attr As Argv = prop.GetAttribute(Of Argv)

            If attr Is Nothing OrElse attr.Name.StringEmpty Then
                Return prop.Name
            Else
                Return attr.Name
            End If
        End Function
    End Module
End Namespace
