#Region "Microsoft.VisualBasic::e8f41233d8c5bb3adba1d11725a47994, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Reflection\Reflection.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.ComponentModel
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' Some common used reflection operation extension at here.
''' </summary>
<PackageNamespace("Emit.Reflection",
                  Category:=APICategories.SoftwareTools,
                  Publisher:="xie.guigang@live.com")>
Public Module EmitReflection

    ''' <summary>
    ''' Run external [.NET] Program from RAM Memory
    ''' </summary>
    ''' <param name="app"></param>
    ''' <param name="CLI"></param>
    ''' <param name="cs">Going to running a c# program?</param>
    ''' <remarks>
    ''' http://www.codeproject.com/Tips/1108105/Run-external-NET-Program-from-RAM-Memory
    ''' 
    ''' Run external app directly from RAM. You can load the specific file into a ``Byte[]`` Array 
    ''' with a ``StreamReader()`` or even download it from WEB via a direct link provided. 
    ''' If you loaded the file from disk, you can delete it if you want after it has been loaded 
    ''' by a ``StreamReader()``.
    ''' </remarks>
    Public Sub RunApp(app As String, Optional CLI As String = "", Optional cs As Boolean = False)
        Dim bufs As Byte() = app.GetMapPath.ReadBinary ' Works on both local file or network file. 

        Try
            Dim assm As Assembly = Assembly.Load(bufs) ' or assm = Reflection.Assembly.Load(New WebClient().DownloadData("https://...."))
            Dim method As MethodInfo = assm.EntryPoint

            If (Not method Is Nothing) Then
                Dim o As Object = assm.CreateInstance(method.Name)

                If String.IsNullOrEmpty(CLI) Then
                    Dim null As Object() = If(cs, {Nothing}, Nothing)
                    Call method.Invoke(o, null)
                Else
                    ' if your app receives parameters
                    Call method.Invoke(o, New Object() {CommandLine.GetTokens(CLI)})
                End If
            Else
                Throw New NullReferenceException($"'{app}' No App Entry Point was found!")
            End If
        Catch ex As Exception
            ex = New Exception("CLI:=" & CLI, ex)
            ex = New Exception("app:=" & app, ex)
#If DEBUG Then
            Call ex.PrintException
#End If
            Throw ex
        End Try
    End Sub

#Region "IsNumericType"
    ''' <summary>
    ''' Determines whether the specified value is of numeric type.
    ''' </summary>
    ''' <param name="o">The object to check.</param>
    ''' <returns>
    ''' true if o is a numeric type; otherwise, false.
    ''' </returns>
    Public Function IsNumericType(o As Object) As Boolean
        Return (TypeOf o Is Byte OrElse
            TypeOf o Is SByte OrElse
            TypeOf o Is Short OrElse
            TypeOf o Is UShort OrElse
            TypeOf o Is Integer OrElse
            TypeOf o Is UInteger OrElse
            TypeOf o Is Long OrElse
            TypeOf o Is ULong OrElse
            TypeOf o Is Single OrElse
            TypeOf o Is Double OrElse
            TypeOf o Is Decimal)
    End Function
#End Region

    <Extension>
    Public Function GetDouble(field As FieldInfo, Optional obj As Object = Nothing) As Double
        Return CType(field.GetValue(obj), Double)
    End Function

    <Extension>
    Public Function GetInt(field As FieldInfo, Optional obj As Object = Nothing) As Integer
        Return CType(field.GetValue(obj), Integer)
    End Function

    ''' <summary>
    '''
    ''' </summary>
    ''' <param name="type"></param>
    ''' <param name="[nameOf]"></param>
    ''' <returns></returns>
    <Extension> Public Function API(type As Type, [nameOf] As String, Optional strict As Boolean = False) As String
#If NET_40 = 0 Then
        Dim methods = type.GetMethods(BindingFlags.Public Or BindingFlags.Static)
        Dim mBase As MethodInfo = (From m As MethodInfo In methods
                                   Where String.Equals([nameOf], m.Name)
                                   Select m).FirstOrDefault
        If mBase Is Nothing Then
NULL:       If Not strict Then
                Return [nameOf]
            Else
                Return ""
            End If
        Else
            Dim APIExport As ExportAPIAttribute = mBase.GetCustomAttribute(Of ExportAPIAttribute)
            If APIExport Is Nothing Then
                GoTo NULL
            Else
                Return APIExport.Name
            End If
        End If
#Else
        Throw New NotSupportedException
#End If
    End Function

    <ExportAPI("GET.Assembly.Details")>
    <Extension>
    Public Function GetAssemblyDetails(path As String) As SoftwareToolkits.ApplicationDetails
        Return New SoftwareToolkits.ApplicationDetails(Assembly.LoadFile(path))
    End Function

    <ExportAPI("GET.Assembly.Details")>
    <Extension>
    Public Function GetAssemblyDetails(def As Type) As SoftwareToolkits.ApplicationDetails
        Return New SoftwareToolkits.ApplicationDetails(def.Assembly)
    End Function

    <ExportAPI("GET.Assembly.Details")>
    <Extension>
    Public Function GetAssemblyDetails(assm As Assembly) As SoftwareToolkits.ApplicationDetails
        Return New SoftwareToolkits.ApplicationDetails(assm)
    End Function

    ''' <summary>
    ''' 得到集合类型的对象之中的元素类型
    ''' </summary>
    ''' <param name="type"></param>
    ''' <param name="strict"></param>
    ''' <returns></returns>
    <Extension> Public Function GetTypeElement(type As Type, strict As Boolean) As Type
        If type.IsInheritsFrom(GetType(Array)) Then
            Return type.GetElementType
        End If
        If type.IsInheritsFrom(GetType(List(Of ))) Then
            Return type.GetGenericArguments.First
        End If
        If type.IsInheritsFrom(GetType(Dictionary(Of ,))) Then
            Dim keyValue As Type() = type.GetGenericArguments
            Return GetType(KeyValuePair(Of ,)).MakeGenericType(keyValue)
        End If

        If strict Then
            Return Nothing
        Else
            Throw New NotImplementedException
        End If
    End Function

    ''' <summary>
    '''
    ''' </summary>
    ''' <param name="Product">.NET EXE/DLL assembly path</param>
    ''' <returns></returns>
    '''
    <ExportAPI("Get.Version")>
    Public Function GetVersion(Product As String) As Version
        Dim assm As System.Reflection.Assembly = System.Reflection.Assembly.LoadFile(Product)
        Return assm.GetVersion
    End Function

    <ExportAPI("Get.Description")>
    <Extension> Public Function Description(prop As PropertyInfo) As String
        Dim attrs As Object() = prop.GetCustomAttributes(GetType(DescriptionAttribute), inherit:=True)

        If attrs.IsNullOrEmpty Then
            Return ""
        Else
            Return DirectCast(attrs(Scan0), DescriptionAttribute).Description
        End If
    End Function

    ''' <summary>
    ''' Gets the <see cref="AssemblyFileVersionAttribute"/> value from the type defined assembly.
    ''' </summary>
    ''' <param name="type"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("Get.Version")>
    <Extension>
    Public Function ModuleVersion(type As Type) As String
        Return type.Assembly.GetVersion.ToString
    End Function

    ''' <summary>
    '''
    ''' </summary>
    ''' <param name="assm">.NET EXE/DLL assembly</param>
    ''' <returns></returns>
    '''
    <ExportAPI("Get.Version")>
    <Extension> Public Function GetVersion(assm As Assembly) As Version
#If NET_40 = 0 Then
        Dim attrs As IEnumerable(Of CustomAttributeData) = assm.CustomAttributes
        Dim vLQuery As CustomAttributeTypedArgument =
            LinqAPI.DefaultFirst(Of CustomAttributeTypedArgument) <=
                    From attr As CustomAttributeData
                    In attrs
                    Where attr.AttributeType.Equals(GetType(AssemblyFileVersionAttribute))
                    Select value = attr.ConstructorArguments(Scan0)

        If vLQuery.Value Is Nothing Then
            Return New Version("1.0.0.0")
        Else
            Return New Version(Scripting.ToString(vLQuery.Value))
        End If
#Else
        Throw New NotSupportedException
#End If
    End Function

    <ExportAPI("Is.Module")>
    <Extension> Public Function IsModule(typeDef As Type) As Boolean
        If typeDef.Name.IndexOf("$") > -1 OrElse typeDef.Name.IndexOf("`") > -1 Then
            Return False ' 匿名类型
        End If

        Return typeDef.IsClass
    End Function

    ''' <summary>
    ''' 出错会返回空集合
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="TProperty"></typeparam>
    ''' <param name="collection"></param>
    ''' <param name="Name">使用System.NameOf()操作符来获取</param>
    ''' <returns></returns>
    <Extension> Public Function [Get](Of T, TProperty)(collection As ICollection(Of T), Name As String, Optional TrimNull As Boolean = True) As TProperty()
        Dim Type As Type = GetType(T)
        Dim Properties = (From p In Type.GetProperties(BindingFlags.Public Or BindingFlags.Instance)
                          Where String.Equals(p.Name, Name)
                          Select p).ToArray
        If Properties.IsNullOrEmpty Then
            Return New TProperty() {}
        End If

        Dim [Property] As PropertyInfo = Properties.First
        Dim resultBuffer As TProperty()

        If TrimNull Then
            resultBuffer = (From obj As T In collection.AsParallel
                            Let value As Object = [Property].GetValue(obj, Nothing)
                            Where Not value Is Nothing
                            Select DirectCast(value, TProperty)).ToArray
        Else
            resultBuffer = (From obj As T In collection.AsParallel
                            Let value As Object = [Property].GetValue(obj, Nothing)
                            Select If(value Is Nothing, Nothing, DirectCast(value, TProperty))).ToArray
        End If

        Return resultBuffer
    End Function

    ''' <summary>
    ''' Is a inherits from b
    ''' </summary>
    ''' <param name="a">继承类型继承自基本类型，具备有基本类型的所有特性</param>
    ''' <param name="b">基本类型</param>
    ''' <param name="strict">
    ''' + 这个参数是为了解决比较来自不同的assembly文件之中的相同类型的比较，但是这个可能会在类型转换出现一些BUG
    ''' + 假若不严格要求的话，那么则两种类型相等的时候也会被算作为继承关系
    ''' + 假若是非严格判断，那么对于泛型而言，只要基本类型也相等也会被判断为成立的继承关系，这个是为了<see cref="Actives"/>操作设计的
    ''' 
    ''' </param>
    ''' <returns></returns>
    ''' <remarks>假若两个类型是来自于不同的assembly文件的话，即使这两个类型是相同的对象，也会无法判断出来</remarks>
    <ExportAPI("Is.InheritsFrom")>
    <Extension> Public Function IsInheritsFrom(a As Type, b As Type, Optional strict As Boolean = True) As Boolean
        Dim baseType As Type = a.BaseType

        If Not strict Then
            If a.Equals(b) Then
                Return True
            End If
            If a.IsGenericType AndAlso b.IsGenericType Then
                ' 2017-3-12
                ' GetType(Dictionary(Of String, Double)).IsInheritsFrom(GetType(Dictionary(Of ,)))

                If a.GetGenericTypeDefinition.Equals(b) Then
                    Return True
                End If
            End If
        End If

        Do While Not baseType Is Nothing
            If baseType.Equals(b) Then
                Return True
            ElseIf Not strict AndAlso (baseType.FullName = b.FullName) Then
                Return True
            Else
                baseType = baseType.BaseType
            End If
        Loop

        Return False
    End Function

    ''' <summary>
    ''' 如果有<see cref="system.ComponentModel.DescriptionAttribute"/>标记，则会返回该标记的字符串数据，假若没有则只会返回类型的名称
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    Public Function Description(Of T)() As String
        Dim typeRef As Type = GetType(T)
        Return typeRef.Description
    End Function

    ''' <summary>
    ''' 如果有<see cref="system.ComponentModel.DescriptionAttribute"/>标记，则会返回该标记的字符串数据，假若没有则只会返回类型的名称
    ''' </summary>
    ''' <returns></returns>
    '''
    <ExportAPI("Get.Description")>
    <Extension> Public Function Description(typeRef As Type) As String
        Dim CustomAttrs As Object() = typeRef.GetCustomAttributes(GetType(DescriptionAttribute), inherit:=False)

        If Not CustomAttrs.IsNullOrEmpty Then
            Return CType(CustomAttrs(Scan0), DescriptionAttribute).Description
        Else
            Return typeRef.Name
        End If
    End Function

    ''' <summary>
    ''' Gets all of the can read and write access property from a type define.
    ''' </summary>
    ''' <param name="type"></param>
    ''' <returns></returns>
#If FRAMEWORD_CORE Then
    <ExportAPI("Get.Properties")>
    <Extension> Public Function GetReadWriteProperties(type As System.Type) As System.Reflection.PropertyInfo()
#Else
    <Extension> Public Function GetReadWriteProperties(type As System.Type) As System.Reflection.PropertyInfo()
#End If
        Dim LQuery = (From p In type.GetProperties Where p.CanRead AndAlso p.CanWrite Select p).ToArray
        Return LQuery
    End Function

    ''' <summary>
    ''' 只对属性有效，出错会返回空值
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <param name="Name"></param>
    ''' <returns></returns>
    '''
    <ExportAPI("GetValue")>
    <Extension> Public Function GetValue(Type As Type, obj As Object, Name As String) As Object
        Try
            Return __getValue(Type, obj, Name)
        Catch ex As Exception
            Return App.LogException(ex, $"{GetType(Extensions).FullName}::{NameOf(GetValue)}")
        End Try
    End Function

    Private Function __getValue(Type As Type, obj As Object, Name As String) As Object
        Dim [property] = Type.GetProperty(Name, BindingFlags.Public Or BindingFlags.Instance)
        If [property] Is Nothing Then
            Return Nothing
        End If
        Dim value = [property].GetValue(obj, Nothing)
        Return value
    End Function

    ''' <summary>
    ''' 只对属性有效，出错会返回空值
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <param name="Name"></param>
    ''' <returns></returns>
    <Extension> Public Function GetValue(Of T)(Type As Type, obj As Object, Name As String) As T
        Dim value = Type.GetValue(obj, Name)
        If value Is Nothing Then
            Return Nothing
        End If
        Dim cast As T = DirectCast(value, T)
        Return cast
    End Function

#If NET_40 = 0 Then

    ''' <summary>
    ''' Try convert the type specific collection data type into a generic enumerable collection data type.(尝试将目标集合类型转换为通用的枚举集合类型)
    ''' </summary>
    ''' <param name="Type">The type specific collection data type.(特定类型的集合对象类型，当然也可以是泛型类型)</param>
    ''' <returns>If the target data type is not a collection data type then the original data type will be returns and the function displays a warning message.</returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Collection2GenericIEnumerable", Info:="Try convert the type specific collection data type into a generic enumerable collection data type.")>
    <Extension> Public Function Collection2GenericIEnumerable(
                                                        Type As Type,
                                                        Optional DebuggerMessage As Boolean = True) As Type

        If Array.IndexOf(Type.GetInterfaces, GetType(IEnumerable)) = -1 Then
EXIT_:      If DebuggerMessage Then Call $"[WARN] Target type ""{Type.FullName}"" is not a collection type!".__DEBUG_ECHO
            Return Type
        End If

        Dim GenericType As Type = GetType(Generic.IEnumerable(Of )) 'Type.GetType("System.Collections.Generic.IEnumerable")
        Dim ElementType As Type = Type.GetElementType

        If ElementType Is Nothing Then
            Dim Generics = Type.GenericTypeArguments

            If Generics.IsNullOrEmpty Then
                GoTo EXIT_
            Else
                ElementType = Generics(Scan0)
            End If
        End If

        GenericType = GenericType.MakeGenericType({ElementType})

        Return GenericType
    End Function
#End If

    ''' <summary>
    ''' Get the method reflection entry point for a anonymous lambda expression.(当函数返回Nothing的时候说明目标对象不是一个函数指针)
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Delegate.GET_Invoke", Info:="Get the method reflection entry point for a anonymous lambda expression.")>
    Public Function GetDelegateInvokeEntryPoint(obj As Object) As System.Reflection.MethodInfo
        Dim TypeInfo As System.Type = obj.GetType
        Dim InvokeEntryPoint = (From MethodInfo As System.Reflection.MethodInfo
                                In TypeInfo.GetMethods
                                Where String.Equals(MethodInfo.Name, "Invoke")
                                Select MethodInfo).FirstOrDefault
        Return InvokeEntryPoint
    End Function

    ''' <summary>
    ''' Get the scripting namespace value from <see cref="[Namespace]"/>
    ''' </summary>
    ''' <param name="__nsType"></param>
    ''' <returns></returns>
    '''
    <ExportAPI("Get.APINamespace")>
    <Extension> Public Function NamespaceEntry(__nsType As Type) As [Namespace]
        Dim attr As Object() = Nothing
        Try
            attr = __nsType.GetCustomAttributes(GetType([Namespace]), True)
        Catch ex As Exception
            Call App.LogException(New Exception(__nsType.FullName, ex))
        End Try
        If attr.IsNullOrEmpty Then
            Return New [Namespace](__nsType.Name, __nsType.FullName, True)
        Else
            Return DirectCast(attr(Scan0), [Namespace])
        End If
    End Function

    ''' <summary>
    ''' Gets the full name of a method reflection meta data.
    ''' </summary>
    ''' <param name="method"></param>
    ''' <param name="IncludeAssembly"></param>
    ''' <returns></returns>
    <ExportAPI("Get.FullName")>
    <Extension> Public Function GetFullName(method As MethodBase, Optional IncludeAssembly As Boolean = False) As String
        Dim Name As String = $"{method.DeclaringType.FullName}::{method.ToString}"
        If Not IncludeAssembly Then
            Return Name
        Else
            Return $"{method.DeclaringType.Module.Assembly.Location.ToFileURL}!{Name}"
        End If
    End Function

    <ExportAPI("Get.FullName")>
    <Extension> Public Function FullName(Method As System.Reflection.MethodInfo, Optional IncludeAssembly As Boolean = False) As String
        Return GetFullName(Method, IncludeAssembly)
    End Function

    ''' <summary>
    ''' Get the specific type of custom attribute from a property.
    ''' If the target custom attribute is not declared on the target, then this function returns nothing.
    ''' (从一个属性对象中获取特定的自定义属性对象)
    ''' </summary>
    ''' <typeparam name="T">The type of the custom attribute.(自定义属性的类型)</typeparam>
    ''' <param name="Property">Target property object.(目标属性对象)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function GetAttribute(Of T As Attribute)([Property] As MemberInfo) As T
        Dim attrType As Type = GetType(T)
        Dim attrs As Object() = [Property].GetCustomAttributes(attrType, True)

        If Not attrs Is Nothing AndAlso attrs.Length = 1 Then
            Dim CustomAttr As T = CType(attrs(Scan0), T)

            If Not CustomAttr Is Nothing Then
                Return CustomAttr
            End If
        Else
            attrs = [Property].GetCustomAttributes(attrType, False)
            If Not attrs.IsNullOrEmpty Then
                Return DirectCast(attrs(Scan0), T)
            End If
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' Get the specific type of custom attribute from a property.
    ''' (从一个属性对象中获取特定的自定义属性对象，找不到的话，就会返回空值)
    ''' </summary>
    ''' <typeparam name="T">The type of the custom attribute.(自定义属性的类型)</typeparam>
    ''' <param name="Property">Target property object.(目标属性对象)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function GetAttribute(Of T As Attribute)([Property] As PropertyInfo) As T
        Dim Attributes As Object() = [Property].GetCustomAttributes(GetType(T), True)

        If Not Attributes Is Nothing AndAlso Attributes.Length = 1 Then
            Dim CustomAttr As T = CType(Attributes(0), T)

            If Not CustomAttr Is Nothing Then
                Return CustomAttr
            End If
        End If
        Return Nothing
    End Function

#If NET_40 = 0 Then

    ''' <summary>
    '''
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="args">构造函数里面的参数信息</param>
    ''' <returns></returns>
    Public Function CreateObject(Of T)(args As Object(),
                                       Optional throwEx As Boolean = True,
                                       <CallerMemberName> Optional caller As String = "") As T
        Try
            Dim obj As Object =
                Activator.CreateInstance(GetType(T), args)
            Return DirectCast(obj, T)
        Catch ex As Exception
            Dim params As String() = args.ToArray(Function(x) x.GetType.FullName & " ==> " & GetObjectJson(x, x.GetType))
            ex = New Exception(String.Join(vbCrLf, params), ex)
            ex = New Exception("@" & caller, ex)

            Call App.LogException(ex)

            If throwEx Then
                Throw ex
            Else
                Return Nothing
            End If
        End Try
    End Function
#End If
End Module
