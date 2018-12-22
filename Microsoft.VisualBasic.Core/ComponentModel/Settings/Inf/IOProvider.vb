Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.Settings.Inf

    ''' <summary>
    ''' 在这个模块之中提供了.NET对象与``*.ini``配置文件之间的相互映射的序列化操作
    ''' </summary>
    Public Module IOProvider

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function EmptySection(x As Type, section As PropertyInfo) As String
            Return $"Property [{x.Name}\({section.PropertyType.Name}){section.Name}] for ini section is null."
        End Function

        ''' <summary>
        ''' 将目标对象写为``*.ini``文件
        ''' (目标对象之中的所有的简单属性都会被保存在一个对象名称的section中，)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="path"></param>
        ''' <returns></returns>
        <Extension>
        Public Function WriteProfile(Of T As Class)(x As T, path$) As Boolean
            Dim ini As New IniFile(path)
            Dim msg$

            ' 首先写入global的配置数据
            Call ClassMapper.ClassDumper(x:=x, type:=GetType(T), ini:=ini)

            For Each section As PropertyInfo In __getSections(Of T)()
                Dim obj As Object = section.GetValue(x, Nothing)
                Dim schema As Type = section.PropertyType

                If obj Is Nothing Then
                    msg = GetType(T).EmptySection(section)
                    obj = Activator.CreateInstance(schema)

                    Call msg.Warning
                    Call App.LogException(msg)
                End If

                Call ClassMapper.ClassDumper(
                    x:=obj,
                    type:=schema,
                    ini:=ini
                )
            Next

            Call $"Ini profile data was saved at location: {path.GetFullPath}".__INFO_ECHO

            Return True
        End Function

        ''' <summary>
        ''' 属性的类型需要定义<see cref="ClassName"/>，Section类型里面的属性还需要
        ''' 定义<see cref="DataFrameColumnAttribute"/>，否则将不会将对应的属性的值
        ''' 写入到ini文件之中。
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function WriteProfile(Of T As Class)(x As T) As Boolean
            Return x.WriteProfile(__getPath(Of T))
        End Function

        ''' <summary>
        ''' 查找出所有<see cref="ClassName"/>标记的属性
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        Private Function __getSections(Of T As Class)() As PropertyInfo()
            Dim properties As PropertyInfo() = GetType(T).GetProperties(PublicProperty)

            properties = LinqAPI.Exec(Of PropertyInfo) _
 _
                () <= From p As PropertyInfo
                      In properties
                      Let type As Type = p.PropertyType
                      Let attr As ClassName = type.GetAttribute(Of ClassName)
                      Where Not attr Is Nothing
                      Select p

            Return properties
        End Function

        ''' <summary>
        ''' 从指定的``*.ini``文件之中加载配置数据，如果配置文件不存在，则这个函数会返回空值
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="path"></param>
        ''' <returns></returns>
        <Extension>
        Public Function LoadProfile(Of T As Class)(path As String) As T
            Dim ini As New IniFile(path)

            If Not ini.FileExists Then
                Return Nothing
            End If

            Dim obj As Object = ClassMapper.ClassWriter(ini, GetType(T))

            For Each prop As PropertyInfo In __getSections(Of T)()
                Dim x As Object = ClassMapper.ClassWriter(ini, prop.PropertyType)
                Call prop.SetValue(obj, x, Nothing)
            Next

            Return DirectCast(obj, T)
        End Function

        Private Function __getPath(Of T As Class)() As String
            Dim path As IniMapIO = GetType(T).GetAttribute(Of IniMapIO)

            If path Is Nothing Then
                Throw New Exception("Could not found path mapping! @" & GetType(T).FullName)
            Else
                Return path.Path
            End If
        End Function

        Public Function LoadProfile(Of T As Class)(Optional ByRef fileExists As Boolean = False, Optional ByRef path$ = Nothing) As T
            path = __getPath(Of T)()
            fileExists = path.FileExists

            If Not fileExists Then
                ' 文件不存在，则直接写文件了
                Dim obj As T = Activator.CreateInstance(Of T)
                Call obj.WriteProfile
                Return obj
            Else
                Return path.LoadProfile(Of T)
            End If
        End Function
    End Module
End Namespace