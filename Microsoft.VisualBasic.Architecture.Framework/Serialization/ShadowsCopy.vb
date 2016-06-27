Imports System.Reflection
Imports System.Runtime.CompilerServices

#If NET_40 = 0 Then

Namespace Serialization

    Public Module ShadowsCopy

        ''' <summary>
        ''' 将目标对象之中的属性按值复制
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        ''' <remarks>对外函数接口，为了防止无限递归的出现</remarks>
        <Extension> Public Function ShadowsCopy(Of T As Class)(obj As T) As T
            Dim CopiedToTarget As T = DirectCast(__shadowsCopy(GetType(T), obj), T)
            Return CopiedToTarget
        End Function

        ''' <summary>
        ''' 将目标对象之中的属性按值复制
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        ''' <remarks>对外函数接口，为了防止无限递归的出现</remarks>
        <Extension> Public Function ShadowsCopy(obj As Object) As Object
            Dim CopiedToTarget As Object = __shadowsCopy(obj.GetType, obj)
            Return CopiedToTarget
        End Function

        ''' <summary>
        ''' 递归使用的，基本数据类型直接复制，引用类型则首先创建一个新的对象，在对该对象进行递归复制，假若目标对象没有可用的无参数的构造函数，则直接赋值
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function __shadowsCopy(typeinfo As Type, obj As Object) As Object
            If ComponentModel.DataSourceModel.DataFramework.PrimitiveFromString.ContainsKey(typeinfo) Then
                Return obj    '首先查看是否为基本类型，是的话则直接返回值，否在使用反射进行递归复制
            End If

            Dim OptParameter As System.Reflection.ParameterInfo = Nothing
            Dim NonParamCtor = (From ctor As System.Reflection.ConstructorInfo
                                In typeinfo.GetConstructors
                                Where ctor.GetParameters.IsNullOrEmpty
                                Select ctor).FirstOrDefault

            If NonParamCtor Is Nothing Then
                NonParamCtor = (From ctor As System.Reflection.ConstructorInfo
                                In typeinfo.GetConstructors
                                Let p = ctor.GetParameters
                                Where p.Length = 1 AndAlso p.First.IsOptional
                                Select ctor).FirstOrDefault

                If NonParamCtor Is Nothing Then
                    Return obj '目标类型没有无参数的构造函数，则直接返回目标对象
                Else
                    OptParameter = NonParamCtor.GetParameters.First     '有一个可选的默认参数，则直接使用默认值进行构造
                End If
            End If

            Dim Target As Object = If(OptParameter Is Nothing, Activator.CreateInstance(typeinfo), Activator.CreateInstance(typeinfo, OptParameter.DefaultValue))

            For Each [Property] In (From p In typeinfo.GetProperties Where p.CanRead AndAlso p.CanWrite Select p).ToArray
                Dim CopiedValue = __shadowsCopy([Property].PropertyType, [Property].GetValue(obj))
                Call [Property].SetValue(Target, CopiedValue)
            Next

            Return Target
        End Function

        ''' <summary>
        ''' 请使用这个函数来对CSV序列化的对象进行浅拷贝。将<paramref name="source"/>之中的第一层的属性值拷贝到<paramref name="target"/>对应的属性值之中，然后返回<paramref name="target"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="Target"></param>
        ''' <returns></returns>
        Public Function ShadowCopy(Of T As Class)(source As T, ByRef Target As T) As T
            Dim opr As New ShadowsCopyOpr(Of T)
            Return opr.ShadowCopy(source, Target)
        End Function

        ''' <summary>
        ''' 将第一层的属性值从基本类复制给继承类
        ''' </summary>
        ''' <typeparam name="Tbase"></typeparam>
        ''' <typeparam name="TInherits"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        Public Function ShadowCopy(Of Tbase As Class, TInherits As Tbase)(source As Tbase) As TInherits
            Dim opr As New ShadowsCopyOpr(Of Tbase, TInherits)
            Dim value As TInherits = opr.ShadowCopy(source)
            Return value
        End Function
    End Module

    ''' <summary>
    ''' 批量拷贝需要使用这个模块来执行
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class ShadowsCopyOpr(Of T As Class)

        ReadOnly prop As PropertyInfo()

        Sub New()
            Me.prop = (From [Property] As PropertyInfo
                       In GetType(T).GetProperties(System.Reflection.BindingFlags.Public Or System.Reflection.BindingFlags.Instance)
                       Where [Property].CanRead AndAlso [Property].CanWrite
                       Select [Property]).ToArray
        End Sub

        Public Function ShadowCopy(source As T) As T
            Dim copyTo As T = Activator.CreateInstance(Of T)
            Return ShadowCopy(source, copyTo)
        End Function

        Public Function ShadowCopy(source As T, copyTo As T) As T
            For Each [property] As PropertyInfo In prop
                Dim value As Object = [property].GetValue(source)
                Call [property].SetValue(obj:=copyTo, value:=value)
            Next

            Return copyTo
        End Function

        Public Overrides Function ToString() As String
            Return $"[{NameOf(ShadowsCopy)}] -->> {GetType(T).FullName}"
        End Function
    End Class

    ''' <summary>
    ''' 批量拷贝需要使用这个模块来执行
    ''' </summary>
    ''' <typeparam name="Tbase"></typeparam>
    ''' <typeparam name="TInherits"></typeparam>
    Public Class ShadowsCopyOpr(Of Tbase As Class, TInherits As Tbase)

        ReadOnly baseProp As PropertyInfo()
        ReadOnly inheritsHash As Dictionary(Of String, PropertyInfo)

        Sub New()
            Dim baseProp = GetType(Tbase).GetProperties(BindingFlags.Public Or BindingFlags.Instance)
            Dim inheritsProp = GetType(TInherits).GetProperties(BindingFlags.Public Or BindingFlags.Instance)
            Dim inheritsHash = (From prop As PropertyInfo In inheritsProp Where prop.CanWrite Select prop).ToDictionary(Function(x) x.Name)

            Me.baseProp = (From prop As PropertyInfo In baseProp Where prop.CanRead Select prop).ToArray
            Me.inheritsHash = inheritsHash
        End Sub

        Public Function ShadowCopy(source As Tbase) As TInherits
            Dim copyTo As TInherits = Activator.CreateInstance(GetType(TInherits))  ' 需要Object类型进行复制

            For Each prop As PropertyInfo In baseProp
                Dim value As Object = prop.GetValue(source)
                Call prop.SetValue(obj:=copyTo, value:=value)
            Next

            Return DirectCast(copyTo, TInherits)
        End Function

        Public Overrides Function ToString() As String
            Return $"{GetType(Tbase).FullName} -->> {GetType(TInherits).FullName}"
        End Function
    End Class
End Namespace
#End If