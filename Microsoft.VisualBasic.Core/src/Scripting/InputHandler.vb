﻿#Region "Microsoft.VisualBasic::0201d27f5e39f79af459ccfee7416407, Microsoft.VisualBasic.Core\src\Scripting\InputHandler.vb"

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

    '   Total Lines: 447
    '    Code Lines: 266 (59.51%)
    ' Comment Lines: 134 (29.98%)
    '    - Xml Docs: 91.04%
    ' 
    '   Blank Lines: 47 (10.51%)
    '     File Size: 19.71 KB


    '     Module InputHandler
    ' 
    '         Properties: [String], CasterString, Types
    ' 
    '         Function: [DirectCast], (+3 Overloads) [GetType], (+2 Overloads) CastArray, Convertible, (+4 Overloads) CTypeDynamic
    '                   DefaultTextParser, GetString, IsPrimitive, ParseDateTime, StringParser
    '                   (+2 Overloads) ToString
    ' 
    '         Sub: CapabilityPromise
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Array
Imports System.Drawing
Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization
Imports CLI = Microsoft.VisualBasic.CommandLine.CommandLine

Namespace Scripting

    ''' <summary>
    ''' Handles the input text string from commandline or scripting, dealing with the 
    ''' conversion of this input string to .NET object in a more easy way.
    ''' (转换从终端或者脚本文件之中输入的字符串的类型的转换)
    ''' </summary>
    Public Module InputHandler

        ''' <summary>
        ''' Object为字符串类型，这个字典可以讲字符串转为目标类型
        ''' </summary>
        ''' <remarks></remarks>
        Public ReadOnly Property CasterString As New Dictionary(Of Type, LoadObject) From {
                                                                                           _
            {GetType(String), Function(s$) s},
            {GetType(Char), AddressOf Casting.CastChar},
            {GetType(Integer), AddressOf Casting.CastInteger},
            {GetType(UInteger), AddressOf UInteger.Parse},
            {GetType(ULong), AddressOf ULong.Parse},
            {GetType(UShort), AddressOf UShort.Parse},
            {GetType(Byte), AddressOf Byte.Parse},
            {GetType(SByte), AddressOf SByte.Parse},
            {GetType(Double), AddressOf Casting.ParseNumeric},
            {GetType(Long), AddressOf Casting.CastLong},
            {GetType(Boolean), AddressOf ParseBoolean},
            {GetType(Char()), AddressOf Casting.CastCharArray},
            {GetType(Date), AddressOf Casting.CastDate},
            {GetType(StringBuilder), AddressOf Casting.CastStringBuilder},
            {GetType(CLI), AddressOf Casting.CastCommandLine},
            {GetType(Image), AddressOf Casting.CastImage},
            {GetType(FileInfo), AddressOf Casting.CastFileInfo},
            {GetType(Color), AddressOf TranslateColor},
            {GetType(Font), AddressOf Casting.CastFont},
            {GetType(System.Net.IPEndPoint), AddressOf Casting.CastIPEndPoint},
            {GetType(LogFile), AddressOf Casting.CastLogFile},
            {GetType(Process), AddressOf Casting.CastProcess},
            {GetType(RegexOptions), AddressOf Casting.CastRegexOptions},
            {GetType(Single), AddressOf Casting.CastSingle},
            {GetType(Decimal), Function(x) CDec(x)},
            {GetType(Point), AddressOf PointParser},
            {GetType(PointF), AddressOf FloatPointParser},
            {GetType(Size), AddressOf SizeParser},
            {GetType(SizeF), AddressOf FloatSizeParser},
            {GetType(DoubleRange), AddressOf NumericRangeParser}
        }

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function StringParser(type As Type) As [Default](Of Func(Of String, Object))
            Return New Func(Of String, Object)(Function(s$) s.CTypeDynamic(type))
        End Function

        ''' <summary>
        ''' Parsing the dat value from the expression text, if any exception happend, a null date value will returned.
        ''' (空字符串会返回空的日期)
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        '''
        <ExportAPI("Date.Parse")>
        <Extension> Public Function ParseDateTime(s As String) As Date
            If String.IsNullOrEmpty(s) Then
                Return New Date
            Else
                Return DateTime.Parse(s)
            End If
        End Function

        ''' <summary>
        ''' Parse the target string value collection as a new array in the given element <paramref name="target"/> type
        ''' </summary>
        ''' <param name="expression">
        ''' A collection of object data in string value
        ''' </param>
        ''' <param name="target">
        ''' The element type
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function CTypeDynamic(expression As IEnumerable(Of String), target As Type) As Array
            If expression Is Nothing Then
                Return Nothing
            ElseIf target Is GetType(String) OrElse target Is GetType(String()) Then
                ' target is a string array
                Return expression.ToArray
            ElseIf target.IsArray Then
                target = target.GetElementType

                If target Is Nothing Then
                    ' object()
                    Return expression _
                        .Select(Function(str) CObj(str)) _
                        .ToArray
                End If
            End If

            Dim allStrs As String() = expression.ToArray
            Dim vec As Array = Array.CreateInstance(target, allStrs.Length)

            For i As Integer = 0 To vec.Length - 1
                vec(i) = CTypeDynamic(allStrs(i), target)
            Next

            Return vec
        End Function

        ''' <summary>
        ''' Converts a string expression which was input from the console or script file to the specified type.
        ''' (请注意，函数只是转换最基本的数据类型，转换错误会返回空值，空字符串也会返回空值)
        ''' </summary>
        ''' <param name="expression">The string expression to convert.</param>
        ''' <param name="target">The type to which to convert the object.</param>
        ''' <returns>An object whose type at run time is the requested target type.</returns>
        ''' <remarks>
        ''' If all failure, then will try <see cref="Conversion.CTypeDynamic"/>
        ''' </remarks>
        <Extension>
        Public Function CTypeDynamic(expression$, target As Type) As Object
            If target Is GetType(String) Then
                Return expression
            ElseIf expression.StringEmpty OrElse expression.ToLower = "null" Then
                Return Nothing
            End If

            If _CasterString.ContainsKey(target) Then
                Dim caster As LoadObject = _CasterString(target)
                Return caster(expression$)
            End If

            Static errCaster As New Index(Of String)

            If expression.Length < 100 Then
                ' 过长的字符串在内存之中累积下来可能会导致内存溢出
                ' 在这里对表达式做下长度限制
                If errCaster.IndexOf(expression & "|" & target.FullName) > -1 Then
                    ' This is a exists error
                    Return Nothing
                End If
            End If

            Try
                Return Conversion.CTypeDynamic(expression, target)
            Catch ex As Exception

                If expression.Length < 100 Then
                    errCaster.Add(expression & "|" & target.FullName)
                End If

                ex = New Exception($"{expression} => {target.FullName}", ex)
                Call App.LogException(ex, MethodBase.GetCurrentMethod.GetFullName)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Converts a string expression which was input from the console or script file to the specified type.
        ''' (请注意，函数只是转换最基本的数据类型，转换错误会返回空值)
        ''' </summary>
        ''' <param name="expr">The string expression to convert.</param>
        ''' <typeparam name="T">The type to which to convert the object.</typeparam>
        ''' <returns>An object whose type at run time is the requested target type.</returns>
        <Extension> Public Function CTypeDynamic(Of T)(expr$, Optional [default] As T = Nothing) As T
            Dim value As Object = CTypeDynamic(expr, GetType(T))

            If value Is Nothing Then
                Return [default]
            Else
                Return DirectCast(value, T)
            End If
        End Function

        ''' <summary>
        ''' 默认的字符串解析方法为<see cref="CTypeDynamic"/>脚本值动态转换函数
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function DefaultTextParser(Of T)() As [Default](Of IStringParser(Of T))
            Return New IStringParser(Of T)(AddressOf CTypeDynamic(Of T)).AsDefault
        End Function

        ''' <summary>
        ''' Does this type can be cast from the <see cref="String"/> type?
        ''' </summary>
        ''' <param name="targetType"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' (目标类型能否由字符串转换过来??)
        ''' </remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IsPrimitive(targetType As Type) As Boolean
            Return CasterString.ContainsKey(targetType)
        End Function

        Public Function IsNullablePrimitive(targetType As Type) As Boolean
            If targetType Is Nothing Then
                Return False
            End If
            If targetType.IsGenericType AndAlso targetType.GetGenericTypeDefinition = GetType(Nullable(Of )) Then
                Return IsPrimitive(targetType.GenericTypeArguments.First)
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Dynamics updates the capability of function <see cref="InputHandler.CTypeDynamic(String, Type)"/>, 
        ''' <see cref="InputHandler.CTypeDynamic(Of T)(String, T)"/> and 
        ''' <see cref="InputHandler.IsPrimitive(Type)"/>
        ''' </summary>
        ''' <param name="briefName"></param>
        ''' <param name="stringConvertType"></param>
        ''' <param name="cast"></param>
        Public Sub CapabilityPromise(briefName$, stringConvertType As Type, cast As LoadObject)
            With _CasterString
                If .ContainsKey(stringConvertType) Then
                    Call .Remove(stringConvertType)
                End If

                Call .Add(stringConvertType, cast)
            End With

            Dim key$ = briefName.ToLower

            If Types.ContainsKey(key) Then
                Call Types.Remove(key)
            End If
            Call Types.Add(key, stringConvertType)
        End Sub

        ''' <summary>
        ''' Enumerate all of the types that can be handled in this module. All of the key string is in lower case.(键值都是小写的)
        ''' </summary>
        Public ReadOnly Property Types As New SortedDictionary(Of String, Type) From {
                                                                                      _
                {"string", GetType(String)},
                {"char", GetType(Char)},
                {"integer", GetType(Integer)},
                {"int32", GetType(Integer)},
                {"int64", GetType(Long)},
                {"long", GetType(Long)},
                {"double", GetType(Double)},
                {"single", GetType(Single)},
                {"byte", GetType(Byte)},
                {"date", GetType(Date)},
                {"logfile", GetType(LogFile)},
                {"color", GetType(Color)},
                {"process", GetType(Process)},
                {"font", GetType(Font)},
                {"image", GetType(Image)},
                {"fileinfo", GetType(IO.FileInfo)},
                {"ipaddress", GetType(System.Net.IPAddress)},
                {"commandline", GetType(CLI)},
                {"stringbuilder", GetType(StringBuilder)},
                {"boolean", GetType(Boolean)},
                {"char()", GetType(Char())},
                {"string()", GetType(String())},
                {"integer()", GetType(Integer())},
                {"double()", GetType(Double())},
                {"bitmap", GetType(Bitmap)},
                {"object", GetType(Object)},
                {"regexoptions", GetType(RegexOptions)}
        }

        ''' <summary>
        ''' Get .NET clr <see cref="Type"/> definition info from its name.
        ''' (类型获取失败会返回空值，大小写不敏感)
        ''' </summary>
        ''' <param name="name">Case insensitive.(类型的名称简写)</param>
        ''' <param name="ObjectGeneric">是否出错的时候返回<see cref="Object"/>类型，默认返回Nothing</param>
        ''' <returns></returns>
        Public Function [GetType](name As String, Optional objectGeneric As Boolean = False) As Type
            Dim lowers = Strings.LCase(name)

            If Types.ContainsKey(lowers) Then
                Return Types(lowers)
            Else
                Dim type As Type = Type.GetType(name, False, True)

                If type Is Nothing AndAlso objectGeneric Then
                    Return GetType(Object)
                Else
                    Return type
                End If
            End If
        End Function

        ''' <summary>
        ''' Get .NET <see cref="Type"/> definition info from its name.
        ''' (类型获取失败会返回空值，大小写不敏感)
        ''' </summary>
        ''' <param name="name">Case insensitive.(类型的名称简写)</param>
        ''' <param name="ObjectGeneric">是否出错的时候返回<see cref="Object"/>类型，默认返回Nothing</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function [GetType](name As Value(Of String), Optional objectGeneric As Boolean = False) As Type
            Return Scripting.GetType(name.Value, objectGeneric)
        End Function

        Public Function [GetType](obj As Object, Optional ObjectGeneric As Boolean = False) As Type
            If obj Is Nothing Then
                If ObjectGeneric Then
                    Return GetType(Object)
                Else
                    Return Nothing
                End If
            Else
                Return obj.GetType
            End If
        End Function

        ''' <summary>
        ''' <see cref="System.Type"/> information for <see cref="System.String"/> type from GetType operator
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property [String] As Type = GetType(String)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ToString(Of T)() As [Default](Of IToString(Of T))
            Return New IToString(Of T)(AddressOf ToString)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetString(Of T)() As [Default](Of Func(Of T, String))
            Return New Func(Of T, String)(AddressOf ToString)
        End Function

        ''' <summary>
        ''' Does the <paramref name="inputtype"/> type can be cast to type <paramref name="DefType"/>.
        ''' (主要为了方便减少脚本编程模块的代码)
        ''' </summary>
        ''' <param name="inputType"></param>
        ''' <param name="DefType"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Convertible(inputType As Type, DefType As Type) As Boolean
            Return inputType.Equals([String]) AndAlso CasterString.ContainsKey(DefType)
        End Function

        ''' <summary>
        ''' <seealso cref="CStrSafe"/>, 出现错误的时候总是会返回空字符串的，
        ''' 
        ''' 注意：
        ''' 
        ''' 1. 对于一些基础的数据类型例如<see cref="Integer"/>,<see cref="Long"/>等则是以json序列化来构建字符串值，
        ''' 2. 对于<see cref="Byte"/>数组则是被编码为base64字符串
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="originToStringAsNothing">
        ''' Result of <see cref="Object.ToString"/> as nothing
        ''' </param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ToString(obj As Object, Optional null$ = "", Optional originToStringAsNothing As Boolean = False) As String
            If TypeOf obj Is String Then
                Return DirectCast(obj, String)
            Else
                Return CStrSafe(obj, null, originToStringAsNothing)
            End If
        End Function

        ''' <summary>
        ''' The parameter <paramref name="obj"/> should implements a <see cref="IEnumerable"/> interface on the type. and then DirectCast object to target type.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Public Function CastArray(Of T)(obj As Object) As T()
            Dim array = DirectCast(obj, IEnumerable)
            Dim data = (From val In array Select DirectCast(val, T)).ToArray
            Return data
        End Function

        Public Function CastArray(obj As Object, type As Type) As Object
            If Array.IndexOf(obj.GetType.GetInterfaces, GetType(IEnumerable)) = -1 Then
                Return obj
            End If

            Dim source As IEnumerable = DirectCast(obj, IEnumerable)
            Dim data = LinqAPI.Exec(Of Object) _
                                               _
                () <= From val As Object
                      In source
                      Let value = Conversion.CTypeDynamic(val, type)
                      Select value

            Return [DirectCast](data, type)
        End Function

        ''' <summary>
        ''' Cast the <see cref="Object"/> array to typed object array.
        ''' </summary>
        ''' <param name="array"></param>
        ''' <param name="type">数组里面的元素的类型</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' please note that, this function cast type by using direct cast, without 
        ''' any type conversion. type-mismatch error may be happends.
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function [DirectCast](array As IEnumerable, type As Type) As Array
            Return Runtime.Extensions.CreateArray(data:=array, type)
        End Function

        ''' <summary>
        ''' the given <paramref name="type"/> value should be the target 
        ''' array element type.
        ''' </summary>
        ''' <param name="array"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CTypeDynamic(array As IEnumerable, type As Type) As Array
            Dim pullAll As Array

            If array.GetType.IsArray Then
                pullAll = array

                If pullAll.GetType.GetElementType Is type Then
                    Return pullAll
                End If
            Else
                pullAll = (From x As Object
                           In array.AsQueryable
                           Select x).ToArray
            End If

            Dim vec As Array = System.Array.CreateInstance(type, pullAll.Length)

            For i As Integer = 0 To vec.Length - 1
                Call vec.SetValue(Conversion.CTypeDynamic(pullAll(i), type), i)
            Next

            Return vec
        End Function
    End Module
End Namespace
