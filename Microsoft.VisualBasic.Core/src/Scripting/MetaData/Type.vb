#Region "Microsoft.VisualBasic::cd25328abf1e10de5b4ea7f2368e0011, Microsoft.VisualBasic.Core\src\Scripting\MetaData\Type.vb"

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

    '   Total Lines: 250
    '    Code Lines: 132
    ' Comment Lines: 80
    '   Blank Lines: 38
    '     File Size: 9.31 KB


    '     Class TypeInfo
    ' 
    '         Properties: assembly, fullName, isSystemKnownType, reference
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: [GetType], (+3 Overloads) LoadAssembly, ToString, TryHandleKnownType
    ' 
    '         Sub: doInfoParser
    ' 
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ApplicationServices.Development.NetCoreApp
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports any = Microsoft.VisualBasic.Scripting

Namespace Scripting.MetaData

    ''' <summary>
    ''' The clr type reference information.
    ''' </summary>
    ''' <remarks>(类型信息)</remarks>
    Public Class TypeInfo

        ''' <summary>
        ''' The assembly file which contains this type definition.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>(模块文件)</remarks>
        <XmlAttribute> Public Property assembly As String
        <XmlAttribute> Public Property reference As String

        ''' <summary>
        ''' <see cref="Type.FullName"/>.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>(类型源)</remarks>
        <XmlText> Public Property fullName As String

        ''' <summary>
        ''' Is this type object is a known system type?
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>(是否是已知的类型？)</remarks>
        Public ReadOnly Property isSystemKnownType As Boolean
            Get
                Return Not any.GetType(fullName) Is Nothing
            End Get
        End Property

        Sub New()
        End Sub

        ''' <summary>
        ''' Creates type reference from the definition.
        ''' </summary>
        ''' <param name="info"></param>
        Sub New(info As Type, Optional fullpath As Boolean = False)
            Call doInfoParser(info, assembly, fullName, reference, fullpath)
        End Sub

        Private Shared Sub doInfoParser(info As Type, ByRef assm As String, ByRef id As String, ByRef reference As String, fullpath As Boolean)
            If fullpath Then
                assm = info.Assembly.Location
            Else
                assm = info.Assembly.Location.FileName
            End If

            id = info.FullName
            reference = info.Assembly.FullName
        End Sub

        Public Overrides Function ToString() As String
            Return $"{assembly}!{fullName}"
        End Function

        ''' <summary>
        ''' Loads the assembly file which contains this type. If the <param name="directory"></param> is not a valid directory location, 
        ''' then using the location <see cref="App.HOME"/> as default.
        ''' </summary>
        ''' <returns></returns>
        Public Function LoadAssembly(Optional directory As DefaultString = Nothing) As Assembly
            Dim path As String = $"{directory Or App.HOME}/{Me.assembly}"
            Dim assm As Assembly = System.Reflection.Assembly.LoadFile(path)

            Return assm
        End Function

        ''' <summary>
        ''' Loads the assembly file which contains this type. 
        ''' </summary>
        ''' <param name="searchPath">
        ''' SetDllDirectory
        ''' </param>
        ''' <returns>
        ''' nothing for dll not found
        ''' </returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function LoadAssembly(searchPath As String()) As Assembly
            Return LoadAssembly(assembly, searchPath)
        End Function

        ''' <summary>
        ''' Loads the assembly file which contains this type. 
        ''' </summary>
        ''' <param name="searchPath">
        ''' SetDllDirectory
        ''' </param>
        ''' <param name="assembly">
        ''' should be the file name of the assembly dll module file, example as: ``library.dll``
        ''' </param>
        ''' <returns>
        ''' nothing for dll not found
        ''' </returns>
        Public Shared Function LoadAssembly(assembly As String, ParamArray searchPath As String()) As Assembly
            Dim path As Value(Of String) = ""
            Dim assm As Assembly = Nothing

            If assembly.FileLength > 0 Then
                Return System.Reflection.Assembly.LoadFile(assembly.GetFullPath)
            End If

            For Each filepath As String In {App.HOME}.JoinIterates(searchPath)
                If filepath.FileLength > 0 Then
                    assm = System.Reflection.Assembly.LoadFile(filepath)
                    Exit For
                ElseIf (path = $"{filepath}/{assembly}").FileExists Then
                    assm = System.Reflection.Assembly.LoadFile(path)
                    Exit For
                End If
            Next

            Return assm
        End Function

        Private Function TryHandleKnownType() As Type
            Dim type As Type = any.GetType(fullName)
            Dim assm As Assembly

            If Not type Is Nothing Then
                Return type
            End If

            Try
                ' 20200630 fix of the bugs of load the identical assembly file from different location
                ' due to the reason of context 'LoadNeither' to context 'Default'
                assm = System.Reflection.Assembly.Load(reference)
                type = assm.GetType(fullName)

                Return type
            Catch ex As Exception

            End Try

            Return Nothing
        End Function

        ''' <summary>
        ''' Get mapping type information.
        ''' </summary>
        ''' <param name="knownFirst">
        ''' 如果这个参数为真的话, 则会尝试直接从当前的应用程序域中查找类信息, 反之则会加载目标程序集进行类型信息查找
        ''' </param>
        ''' <param name="throwEx">
        ''' 如果这个参数设置为False的话，则出错的时候会返回空值
        ''' </param>
        ''' <param name="searchPath">
        ''' A list of the candidates directory path for search dll files
        ''' </param>
        ''' <param name="getException">
        ''' <see cref="DllNotFoundException"/>
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' the function of <see cref="deps.TryHandleNetCore5AssemblyBugs"/> has been called
        ''' automatically when the assembly is built for .netcore app
        ''' </remarks>
        Public Overloads Function [GetType](Optional knownFirst As Boolean = False,
                                            Optional throwEx As Boolean = True,
                                            Optional ByRef getException As Exception = Nothing,
                                            Optional searchPath$() = Nothing) As Type
            Dim type As Type = Nothing
            Dim assm As Assembly

            If knownFirst Then
                type = TryHandleKnownType()

                If Not type Is Nothing Then
                    Return type
                End If
            End If

            ' 错误一般出现在loadassembly阶段
            ' 主要是文件未找到
            Try
                assm = searchPath.DoCall(AddressOf LoadAssembly)

                If assm Is Nothing Then
                    getException = New DllNotFoundException(Me.assembly)

                    If throwEx Then
                        Throw getException
                    Else
                        Return Nothing
                    End If
                Else
#If NETCOREAPP Then
                    Call deps.TryHandleNetCore5AssemblyBugs(package:=assm, external_libloc:=searchPath)
#End If
                End If

                type = assm.GetType(Me.fullName)

                If type Is Nothing Then
                    getException = New MissingMemberException($"We could not found type '{Me.fullName}' in target assembly: {assm.FullName}!")
                Else
                    getException = Nothing
                End If
            Catch ex As Exception
                ex = New DllNotFoundException(ToString, ex)

                If throwEx Then
                    Throw ex
                Else
                    getException = ex
                End If
            Finally

            End Try

            Return type
        End Function

        ''' <summary>
        ''' 检查a是否是指向b的类型引用的
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator =(a As TypeInfo, b As Type) As Boolean
            Dim assm As String = Nothing
            Dim type As String = Nothing
            Dim reference As String = Nothing

            Call doInfoParser(b, assm, type, reference, fullpath:=False)

            Return String.Equals(a.assembly, assm, StringComparison.OrdinalIgnoreCase) AndAlso
                String.Equals(a.fullName, type, StringComparison.Ordinal) AndAlso
                String.Equals(a.reference, reference, StringComparison.Ordinal)
        End Operator

        Public Overloads Shared Operator <>(a As TypeInfo, b As Type) As Boolean
            Return Not a = b
        End Operator
    End Class
End Namespace
