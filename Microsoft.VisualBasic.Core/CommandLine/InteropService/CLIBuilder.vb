#Region "Microsoft.VisualBasic::2444e09cd97a4a154b5b102511f65a4b, Microsoft.VisualBasic.Core\CommandLine\InteropService\CLIBuilder.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Module CLIBuildMethod
    ' 
    '         Function: GetCLI, SimpleBuilder
    '         Delegate Function
    ' 
    '             Function: __booleanRule, __pathRule, __stringEnumRule, __stringRule, ClearParameters
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Language

Namespace CommandLine.InteropService

    Public Module CLIBuildMethod

        ReadOnly _typeInfo As Type = GetType([Optional])

        ''' <summary>
        ''' Generates the command line string value for the invoked target cli program using this interop services object instance.
        ''' (生成命令行参数)
        ''' </summary>
        ''' <typeparam name="TInteropService">
        ''' A class type object for interaction with a commandline program.
        ''' (与命令行程序进行交互的模块对象类型)
        ''' </typeparam>
        ''' <param name="app">目标交互对象的实例</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 依照类型<see cref="CLITypes"/>来生成参数字符串
        ''' 
        ''' <see cref="CLITypes.Boolean"/>, True => 参数名；
        ''' <see cref="CLITypes.Double"/>, <see cref="CLITypes.Integer"/>, <see cref="CLITypes.String"/>, => 参数名 + 参数值，假若字符串为空则不添加；
        ''' （假若是枚举值类型，可能还需要再枚举值之中添加<see cref="DescriptionAttribute"/>属性）
        ''' <see cref="CLITypes.File"/>, 假若字符串为空则不添加，有空格自动添加双引号，相对路径会自动转换为全路径。
        ''' </remarks>
        <Extension>
        Public Function GetCLI(Of TInteropService As Class)(app As TInteropService) As String
            Dim args As BindProperty(Of [Optional])() =
                LinqAPI.Exec(Of BindProperty(Of [Optional])) <=
 _
                From [property] As PropertyInfo
                In GetType(TInteropService).GetProperties
                Let attrs As Object() =
                    [property].GetCustomAttributes(attributeType:=_typeInfo, inherit:=True)
                Where Not attrs.IsNullOrEmpty
                Let attr As [Optional] = DirectCast(attrs.First, [Optional])
                Select New BindProperty(Of [Optional]) With {
                    .field = attr,
                    .member = [property]
                }
            Dim sb As New StringBuilder(1024)

            For Each argum As BindProperty(Of [Optional]) In args
                Dim getCLIToken As __getCLIToken = __getMethods(argum.field.Type)
                Dim value As Object = argum.GetValue(app)
                Dim cliToken As String = getCLIToken(value, argum.field, DirectCast(argum.member, PropertyInfo))

                If Not String.IsNullOrEmpty(cliToken) Then
                    Call sb.Append(cliToken & " ")
                End If
            Next

            Return sb.ToString.TrimEnd
        End Function

        ''' <summary>
        ''' Creates a command line string by using simply fills the name and parameter values
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Function SimpleBuilder(name$, args As IEnumerable(Of KeyValuePair(Of String, String))) As String
            Dim sbr As New StringBuilder(name)

            For Each x In args
                If String.IsNullOrEmpty(x.Value) Then
                    Continue For
                End If

                Call sbr.Append(" ")
                Call sbr.Append(x.Key & " ")
                Call sbr.Append(x.Value.CLIToken)
            Next

            Return sbr.ToString
        End Function

#Region ""

        ''' <summary>
        ''' Converts the property value to a CLI token
        ''' </summary>
        Private ReadOnly __getMethods As IReadOnlyDictionary(Of CLITypes, __getCLIToken) =
            New Dictionary(Of CLITypes, __getCLIToken) From {
 _
            {CLITypes.Boolean, AddressOf CLIBuildMethod.__booleanRule},
            {CLITypes.Double, AddressOf CLIBuildMethod.__stringRule},
            {CLITypes.File, AddressOf CLIBuildMethod.__pathRule},
            {CLITypes.Integer, AddressOf CLIBuildMethod.__stringRule},
            {CLITypes.String, AddressOf CLIBuildMethod.__stringRule}
        }

        Private Delegate Function __getCLIToken(value As Object, attr As [Optional], prop As PropertyInfo) As String

        ''' <summary>
        ''' The different between the String and Path is that applying <see cref="CLIToken"/> or <see cref="CLIPath"/>.
        ''' </summary>
        ''' <param name="value">只能是<see cref="System.String"/>类型的</param>
        ''' <param name="attr"></param>
        ''' <param name="prop"></param>
        ''' <returns></returns>
        Private Function __pathRule(value As Object, attr As [Optional], prop As PropertyInfo) As String
            Dim path As String = DirectCast(value, String)
            If Not String.IsNullOrEmpty(path) Then
                path = $"{attr.Name} {path.CLIPath}"
            End If
            Return path
        End Function

        ''' <summary>
        ''' 可能包含有枚举值
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="attr"></param>
        ''' <param name="prop"></param>
        ''' <returns></returns>
        Private Function __stringRule(value As Object, attr As [Optional], prop As PropertyInfo) As String
            If prop.PropertyType.Equals(GetType(String)) Then
                Dim str As String = Scripting.ToString(value)
                If String.IsNullOrEmpty(str) Then
                    Return ""
                Else
                    Return $"{attr.Name} {str.CLIToken}"
                End If
            ElseIf prop.PropertyType.IsInheritsFrom(GetType([Enum])) Then
                Return __stringEnumRule(value, attr, prop)
            Else
                Dim str As String = Scripting.ToString(value)
                Return $"{attr.Name} {str}"
            End If
        End Function

        Private Function __stringEnumRule(value As Object, attr As [Optional], prop As PropertyInfo) As String
            Dim enumValue As [Enum] = DirectCast(value, System.Enum)
            Dim type As Type = prop.PropertyType
            Dim enumFields As FieldInfo() = type.GetFields
            Dim nullGet = (From x As FieldInfo In enumFields
                           Let flag As NullOrDefault = x.GetAttribute(Of NullOrDefault)
                           Where Not flag Is Nothing
                           Select flag, x).FirstOrDefault
            If nullGet Is Nothing Then  '没有默认值
rtvl:           Dim strValue As String = enumValue.Description
                Return $"{attr.Name} {strValue.CLIToken}"
            Else
                If nullGet.x.GetValue(Nothing).Equals(value) Then
                    Dim str As String = nullGet.flag.value      ' 是默认值，则返回默认值
                    If String.IsNullOrEmpty(str) Then
                        Return ""
                    Else
                        Return $"{attr.Name} {str}"
                    End If
                Else
                    GoTo rtvl
                End If
            End If
        End Function

        ''' <summary>
        ''' Property value to boolean flag in the CLI
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="attr"></param>
        ''' <param name="prop"></param>
        ''' <returns></returns>
        Private Function __booleanRule(value As Object, attr As [Optional], prop As PropertyInfo) As String
            Dim name As String = attr.Name
            Dim b As Boolean

            If prop.PropertyType.Equals(GetType(Boolean)) Then
                b = DirectCast(value, Boolean)
            Else
                Dim str As String = Scripting.ToString(value)
                b = str.ParseBoolean
            End If

            If b Then
                Return name
            Else
                Return ""
            End If
        End Function
#End Region

        ''' <summary>
        ''' Reset the CLI parameters property in the target class object.
        ''' </summary>
        ''' <typeparam name="TInteropService"></typeparam>
        ''' <param name="inst"></param>
        ''' <returns>返回所重置的参数的个数</returns>
        ''' <remarks></remarks>
        Public Function ClearParameters(Of TInteropService As Class)(inst As TInteropService) As Integer
            Dim n As Integer
            Dim lstProperty As PropertyInfo() = inst.GetType().GetProperties()

            Try
                For Each [Property] As PropertyInfo In lstProperty
                    Dim attrs As Object() = [Property].GetCustomAttributes(_typeInfo, inherit:=False)
                    If Not (attrs Is Nothing OrElse attrs.Length = 0) Then
                        Call [Property].SetValue(inst, "", Nothing)
                        n += 1
                    End If
                Next
            Catch ex As Exception
                Throw New InvalidOperationException(InvalidOperation)
            End Try

            Return n
        End Function

        Const InvalidOperation$ = "The target type information is not the 'System.String'!"
    End Module
End Namespace
