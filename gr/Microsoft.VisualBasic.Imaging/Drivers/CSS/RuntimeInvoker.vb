#Region "Microsoft.VisualBasic::4c7a47cafb990417bf2f585c8479f951, gr\Microsoft.VisualBasic.Imaging\Drivers\CSS\RuntimeInvoker.vb"

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

    '   Total Lines: 218
    '    Code Lines: 132 (60.55%)
    ' Comment Lines: 56 (25.69%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 30 (13.76%)
    '     File Size: 9.37 KB


    '     Module RuntimeInvoker
    ' 
    '         Function: CSSTemplate, LoadDriver, ParseFieldNames, (+2 Overloads) RunPlot, ScanValue
    ' 
    '         Sub: AppendFields
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Language.CSS

Namespace Driver.CSS

    ''' <summary>
    ''' 因为绘图的函数一般有很多的CSS样式参数用来调整图形上面的元素的样式，
    ''' 通过命令行传递这么多的参数不现实，故而在这里通过CSS文件加反射的形式
    ''' 来传递这些绘图参数，并且同时也保留对函数式编程的兼容性
    ''' </summary>
    Public Module RuntimeInvoker

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function LoadDriver(container As Type, api$) As MethodInfo
            Return DirectCast(container, TypeInfo) _
                .DeclaredMethods _
                .Select(Function(m)
                            Return (entry:=m.GetCustomAttribute(Of Driver), Driver:=m)
                        End Function) _
                .Where(Function(m)
                           Return Not m.entry Is Nothing AndAlso
                                      m.entry.Name.TextEquals(api)
                       End Function) _
                .Select(Function(m) m.Driver) _
                .FirstOrDefault
        End Function

        ''' <summary>
        ''' Get all CSS field names
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function ParseFieldNames(type As Type) As String()
            Return type _
                .Schema(PropertyAccess.ReadWrite, nonIndex:=True) _
                .Values _
                .Select(Function(prop)
                            Return prop.Description Or prop.Name.AsDefault(Function(s) DirectCast(s, String).StringEmpty)
                        End Function) _
                .ToArray
        End Function

        ReadOnly types As New Dictionary(Of Types, String()) From {
            {CSS.Types.Brush, GetType(Fill).ParseFieldNames},
            {CSS.Types.Font, GetType(CSSFont).ParseFieldNames},
            {CSS.Types.Padding, GetType(Padding).ParseFieldNames},
            {CSS.Types.Size, GetType(CSSsize).ParseFieldNames},
            {CSS.Types.Stroke, GetType(Stroke).ParseFieldNames}
        }

        Const Indent$ = vbTab

        ''' <summary>
        ''' Generate CSS template for the plot driver function.
        ''' </summary>
        ''' <param name="driver"></param>
        ''' <returns></returns>
        <Extension> Public Function CSSTemplate(driver As MethodInfo) As String
            Dim args = driver _
                .GetParameters _
                .Where(Function(parm)
                           With parm.ParameterType
                               Return .ByRef Is GetType(String) OrElse DataFramework.IsPrimitive(.ByRef)
                           End With
                       End Function) _
                .Select(Function(parm)
                            Return (
                                Type:=parm.GetCustomAttribute(Of CSSSelector),
                                arg:=parm)
                        End Function) _
                .Where(Function(parm) Not parm.Type Is Nothing) _
                .ToArray

            Dim CSS As New StringBuilder

            Call CSS.AppendLine($"/* CSS template for ""{driver.GetCustomAttribute(Of Driver).Name}"" */")
            Call CSS.AppendLine()

            ' global settings
            Call CSS.AppendLine("@canvas {")

            ' canvas size
            Call CSS.AppendLine()
            Call CSS.AppendLine(Indent & "/* Canvas size */")
            Call CSS.AppendFields(types(Imaging.Driver.CSS.Types.Size))

            ' canvas drawing paddings
            Call CSS.AppendLine()
            Call CSS.AppendLine(Indent & "/* canvas drawing paddings */")
            Call CSS.AppendFields(types(Imaging.Driver.CSS.Types.Padding))

            ' background
            Call CSS.AppendLine()
            Call CSS.AppendLine(Indent & "/* Canvas background */")
            Call CSS.AppendFields(types(Imaging.Driver.CSS.Types.Brush))

            ' default font style
            Call CSS.AppendLine()
            Call CSS.AppendLine(Indent & "/* default CSS font style */")
            Call CSS.AppendFields(types(Imaging.Driver.CSS.Types.Font))

            Call CSS.AppendLine("}")
            Call CSS.AppendLine()

            ' optional function parameters for tweaks of CSS styles
            For Each parm In args
                Call CSS.AppendLine($"#{parm.arg.Name} {{")
                Call CSS.AppendFields(types(parm.Type.Type))
                Call CSS.AppendLine("}")

                Call CSS.AppendLine()
            Next

            Return CSS.ToString
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Sub AppendFields(CSS As StringBuilder, fields$())
            For Each field As String In fields
                Call CSS.AppendLine($"{Indent}{field}: value;")
            Next
        End Sub

        ' CSS文件说明
        ' 
        ' selector为函数参数的名称
        ' 样式属性则是具体的参数值
        '
        ' 例如
        ' #tickFont {
        '     font-size: 14px;
        '     color: red;
        ' }
        '
        ' 定义了绘图函数的tickFont参数的字体大小为14个像素点，并且在进行绘图的时候字体颜色为红色
        ' 如果没有定义字体名称的话，则是使用默认字体

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="driver">绘图函数</param>
        ''' <param name="CSS">
        ''' 定义了该函数之中的CSS元素的样式值，假若没有某一个可选参数的值，则使用默认参数值；
        ''' 或者参数值出现在了<paramref name="args"/>列表之中，则会被<paramref name="args"/>的值所覆盖
        ''' </param>
        ''' <param name="args">必须要包含有所有的必须参数，可选参数可以不包含在其中</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 因为考虑到手动输入参数可能会出现大小写不匹配的问题，故而在这里会首先尝试使用字典查找，
        ''' 没有找到键名的时候才会进行字符串大小写不敏感的字符串比较
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function RunPlot(driver As [Delegate], CSS As CSSFile, ParamArray args As ArgumentReference()) As GraphicsData
            Return driver.Method.RunPlot(driver.Target, CSS, args)
        End Function

        Const RequiredArgvNotFound$ = "Parameter '{0}' which is required by the graphics driver function is not found!"

        <Extension>
        Public Function RunPlot(driver As MethodInfo, target As Object, CSS As CSSFile, ParamArray args As ArgumentReference()) As GraphicsData
            Dim parameters = driver.GetParameters
            Dim arguments As New List(Of Object)
            Dim values As Dictionary(Of ArgumentReference) = args.ToDictionary

            ' 因为args是必须参数，所以要首先进行赋值遍历
            For Each arg As ParameterInfo In parameters
                If values.ContainsKey(arg.Name) Then
                    arguments += values(arg.Name).Value
                Else
                    arguments += arg.ScanValue(values, CSS)
                End If
            Next

            Return driver.Invoke(target, arguments.ToArray)
        End Function

        <Extension>
        Private Function ScanValue(arg As ParameterInfo, values As Dictionary(Of ArgumentReference), CSS As CSSFile) As Object
            With values.Keys.Where(Function(s) s.TextEquals(arg.Name)).FirstOrDefault
                If Not .StringEmpty Then
                    Return values(.ByRef).Value
                End If
            End With

            ' 在values参数列表之中查找不到，则可能是在CSS之中定义的样式，
            ' 查看CSS样式文件之中是否存在？
            Dim style As Selector = CSS("#" & arg.Name)

            If style Is Nothing Then

                ' 在CSS之中没有定义，则判断这个参数是否为可选参数，
                ' 如果不是可选参数， 则抛出错误
                If Not arg.IsOptional Then
                    Throw New ArgumentNullException(String.Format(RequiredArgvNotFound, arg.Name))
                Else
                    Return arg.DefaultValue
                End If
            Else

                ' 因为绘图的样式值都是使用CSS字符串来完成的，所以
                ' 在这里就直接调用CSS样式的ToString方法来得到
                ' 参数值了
                Return style.ToString
            End If
        End Function
    End Module
End Namespace
