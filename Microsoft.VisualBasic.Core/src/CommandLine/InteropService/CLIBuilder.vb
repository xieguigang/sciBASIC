#Region "Microsoft.VisualBasic::148d340e1974785080809fd0f39de3df, Microsoft.VisualBasic.Core\src\CommandLine\InteropService\CLIBuilder.vb"

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


    ' Code Statistics:

    '   Total Lines: 284
    '    Code Lines: 163 (57.39%)
    ' Comment Lines: 89 (31.34%)
    '    - Xml Docs: 85.39%
    ' 
    '   Blank Lines: 32 (11.27%)
    '     File Size: 11.87 KB


    '     Module CLIBuildMethod
    ' 
    '         Function: GetCLI, (+2 Overloads) GetPrefix, SimpleBuilder
    '         Delegate Function
    ' 
    '             Function: booleanRule, ClearParameters, formatToken, pathRule, stringEnumRule
    '                       stringRule
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

        ReadOnly Argument As Type = GetType(Argv)

        <Extension>
        Public Function GetPrefix([property] As PropertyInfo) As String
            With [property].GetCustomAttribute(Of Prefix)
                If .IsNothing Then
                    Return Nothing
                Else
                    Return .Value
                End If
            End With
        End Function

        <Extension>
        Public Function GetPrefix(Of T As Class)(obj As T) As String
            With obj.GetType.GetCustomAttribute(Of Prefix)
                If .IsNothing Then
                    Return Nothing
                Else
                    Return .Value
                End If
            End With
        End Function

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
        Public Function GetCLI(Of TInteropService As Class)(app As TInteropService, Optional prefix$ = "") As String
            Dim prop As PropertyInfo = Nothing
            Dim argv As Argv
            Dim args As BindProperty(Of Argv)() = LinqAPI.Exec(Of BindProperty(Of Argv)) _
 _
                () <= From [property] As PropertyInfo
                      In GetType(TInteropService).GetProperties()
                      Let attrs As Object() = [property].GetCustomAttributes(attributeType:=Argument, inherit:=True)
                      Where Not attrs.IsNullOrEmpty
                      Let attr As Argv = DirectCast(attrs.First, Argv)
                      Select New BindProperty(Of Argv)(
                          attr:=attr,
                          prop:=[property],
                          getName:=Function(a) a.Name
                      )

            Dim sb As New StringBuilder()

            ' undefined类型需要用户自己进行处理
            For Each argum As BindProperty(Of Argv) In args.Where(Function(a) a.field.Type <> CLITypes.Undefined)
                Dim getToken As getValue = convertMethods(argum.field.Type)
                Dim value As Object = argum.GetValue(app)

                ' integer, double这些类型都用nullable类型
                ' 所以如果value是nothing，就说明该属性肯定没有赋值
                If value Is Nothing Then
                    ' 如果是nothing则表示没有赋值
                    ' 跳过
                    Continue For
                Else
                    prop = DirectCast(argum.member, PropertyInfo)
                    argv = argum.field
                End If

                With getToken(value, argv, prop)
                    ' 有些类型是会返回空字符串结果的
                    ' 例如boolean类型的数据，false的时候是返回空字符串
                    ' 所以会需要在这里判断一下

                    If Not .StringEmpty Then
                        ' 如果prefix参数不为空，则会添加统一的前缀
                        sb.AppendLine(prefix & .ByRef)
                    End If
                End With
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
        ReadOnly convertMethods As New Dictionary(Of CLITypes, getValue) From {
 _
            {CLITypes.Boolean, AddressOf CLIBuildMethod.booleanRule},
            {CLITypes.Double, AddressOf CLIBuildMethod.stringRule},
            {CLITypes.File, AddressOf CLIBuildMethod.pathRule},
            {CLITypes.Integer, AddressOf CLIBuildMethod.stringRule},
            {CLITypes.String, AddressOf CLIBuildMethod.stringRule}
        }

        ''' <summary>
        ''' Convert property to cli parameter value string
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="attr"></param>
        ''' <param name="prop"></param>
        ''' <returns></returns>
        Private Delegate Function getValue(value As Object, attr As Argv, prop As PropertyInfo) As String

        ''' <summary>
        ''' The different between the String and Path is that applying <see cref="CLIToken"/> or <see cref="CLIPath"/>.
        ''' </summary>
        ''' <param name="value">只能是<see cref="String"/>类型的</param>
        ''' <param name="attr"></param>
        ''' <param name="prop"></param>
        ''' <returns></returns>
        Private Function pathRule(value As Object, attr As Argv, prop As PropertyInfo) As String
            With DirectCast(value, String)
                If Not .StringEmpty Then
                    Return attr.formatToken(.CLIPath)
                Else
                    Return Nothing
                End If
            End With
        End Function

        ''' <summary>
        ''' 这个方法不会影响逻辑值类型
        ''' </summary>
        ''' <param name="attr"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        <Extension>
        Private Function formatToken(attr As Argv, value As String) As String
            If attr.Format.StringEmpty Then
                Return $"{attr.Name} {value}"
            Else
                Return attr.Format.Replace("%s", value)
            End If
        End Function

        ''' <summary>
        ''' 可能包含有枚举值
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="attr"></param>
        ''' <param name="prop"></param>
        ''' <returns></returns>
        Private Function stringRule(value As Object, attr As Argv, prop As PropertyInfo) As String
            If prop.PropertyType.Equals(GetType(String)) Then
                With Scripting.ToString(value)
                    If Not .StringEmpty Then
                        Return attr.formatToken(.CLIToken)
                    Else
                        Return Nothing
                    End If
                End With
            ElseIf prop.PropertyType.IsInheritsFrom(GetType([Enum])) Then
                Return stringEnumRule(value, attr, prop)
            Else
                ' 数值类型
                Return attr.formatToken(Scripting.ToString(value))
            End If
        End Function

        ''' <summary>
        ''' 将枚举类型的属性值转换为命令行的参数值，这个转换过程与<see cref="Argv.Type"/>的值相关
        ''' 
        ''' + <see cref="CLITypes.String"/>的时候，会直接调用ToString生成参数值
        ''' + <see cref="CLITypes.Integer"/>的时候，会将枚举值的数值作为命令行参数值
        ''' 
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="attr"></param>
        ''' <param name="prop"></param>
        ''' <returns></returns>
        Private Function stringEnumRule(value As Object, attr As Argv, prop As PropertyInfo) As String
            Dim enumValue As [Enum] = DirectCast(value, [Enum])

            If attr.Type = CLITypes.String Then
                Return attr.formatToken(enumValue.Description.CLIToken)
            ElseIf attr.Type = CLITypes.Integer Then
                Return attr.formatToken(Convert.ToInt32(enumValue))
            Else
                Throw New InvalidCastException($"Unable cast {enumValue.GetType.FullName} enum value to such type: {attr.Type.ToString}")
            End If
        End Function

        ''' <summary>
        ''' Property value to boolean flag in the CLI
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="attr"></param>
        ''' <param name="prop"></param>
        ''' <returns></returns>
        Private Function booleanRule(value As Object, attr As Argv, prop As PropertyInfo) As String
            Dim name As String = attr.Name
            Dim b As Boolean

            If prop.PropertyType.Equals(GetType(Boolean)) Then
                b = DirectCast(value, Boolean)
            Else
                b = Scripting.ToString(value).ParseBoolean
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
            Dim properties As PropertyInfo() = inst.GetType().GetProperties()

            Try
                For Each [property] As PropertyInfo In properties
                    Dim attrs As Object() = [property].GetCustomAttributes(Argument, inherit:=False)

                    If Not (attrs Is Nothing OrElse attrs.Length = 0) Then
                        Call [property].SetValue(inst, Nothing, Nothing)
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
