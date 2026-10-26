Imports System.Globalization
Imports System.IO
Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Runtime.CompilerServices

Namespace Pickle

    ''' <summary>
    ''' 以引用相等（而非 <c>Equals</c>）比较对象的比较器，供 pickler 的 memo 表使用。
    ''' </summary>
    ''' <remarks>
    ''' 覆盖了 <c>Equals</c> 的类（例如值对象风格的类型）如果使用默认比较器，
    ''' 两个不同实例会被误判为同一个 memo 条目，从而导致反序列化后的共享引用错乱。
    ''' </remarks>
    Friend Class ReferenceComparer
        Implements IEqualityComparer(Of Object)

        Public Shared ReadOnly Instance As New ReferenceComparer()

        Public Shadows Function Equals(x As Object, y As Object) As Boolean Implements IEqualityComparer(Of Object).Equals
            Return Object.ReferenceEquals(x, y)
        End Function

        Public Shadows Function GetHashCode(obj As Object) As Integer Implements IEqualityComparer(Of Object).GetHashCode
            Return RuntimeHelpers.GetHashCode(obj)
        End Function
    End Class

    ''' <summary>
    ''' .NET CLR 对象的 pickle 编解码辅助：负责生成可解析的类型标识、按名字解析类型、
    ''' 以"字段快照"的方式采集与还原对象状态。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' pickle 格式本身是 Python 的序列化协议，没有"CLR 类型"的概念。为了让 .NET 对象可以
    ''' 通过 pickle 完成往返（保存后再读回仍然得到原始类型），本模块使用 <c>GLOBAL</c> 操作码
    ''' 携带一个 <c>clr:</c> 前缀的类型标识：
    ''' </para>
    ''' <code>
    ''' GLOBAL 'clr:MyAssembly'
    ''' My.Name.Space.MyClass
    ''' REDUCE / BUILD ...
    ''' </code>
    ''' <para>
    ''' 反序列化时按"程序集 + 类型全名"解析（失败后再扫描当前已加载的全部程序集，
    ''' 因此即使程序集版本升级或改名，只要类型全名不变仍可还原）。对于本库旧版本写出的
    ''' 文件（<c>module</c> 字段是命名空间而不是 <c>clr:</c> 前缀），也会在默认的 Python
    ''' 全局符号映射全部落空之后再做一次 CLR 解析尝试。
    ''' </para>
    ''' <para>
    ''' 对象状态以"字段名 → 字段值"的字典形式写入 <c>BUILD</c>：采集全部实例字段
    ''' （<c>Public</c> 与 <c>Private</c>，含基类继承字段，跳过 <c>&lt;NonSerialized&gt;</c>
    ''' 与委托类型字段），还原时回填字段；若状态里有未能匹配到字段的键，则尝试同名的
    ''' 可写属性。这样 VB 的自动属性（编译器生成的 <c>_Name</c> 字段）、只读字段、
    ''' 只带参数化构造函数的类型都能正确还原（通过 <see cref="RuntimeHelpers.GetUninitializedObject"/> 构造空实例）。
    ''' </para>
    ''' </remarks>
    Public Module ClrObjectCodec

        ''' <summary>
        ''' CLR 类型标识的模块名前缀。
        ''' </summary>
        Public Const ModulePrefix As String = "clr:"

        Private ReadOnly _types As New Dictionary(Of String, Type)(StringComparer.Ordinal)
        Private ReadOnly _layouts As New Dictionary(Of Type, FieldLayout)()
        Private ReadOnly _structBuilders As New Dictionary(Of Type, Func(Of Object(), Object))()

        ''' <summary>
        ''' 类型的字段布局：采集顺序与"字段名 → 字段"的映射。
        ''' </summary>
        Private Class FieldLayout
            Public ReadOnly Fields As FieldInfo()
            Public ReadOnly Map As Dictionary(Of String, FieldInfo)

            Public Sub New(fields As FieldInfo(), map As Dictionary(Of String, FieldInfo))
                Me.Fields = fields
                Me.Map = map
            End Sub
        End Class

        ''' <summary>
        ''' 生成类型标识的模块名部分，例如 <c>clr:Microsoft.VisualBasic.Data.BinaryData</c>。
        ''' </summary>
        ''' <param name="clrType">目标类型</param>
        Public Function GetModuleName(clrType As Type) As String
            If clrType Is Nothing Then
                Throw New ArgumentNullException(NameOf(clrType))
            End If

            Return ModulePrefix & clrType.Assembly.GetName().Name
        End Function

        ''' <summary>
        ''' 生成类型标识的类名部分：普通类型使用全名（嵌套类型带 <c>+</c>），
        ''' 构造泛型使用程序集限定名（否则无法还原泛型实参）。
        ''' </summary>
        ''' <param name="clrType">目标类型</param>
        Public Function GetClassName(clrType As Type) As String
            If clrType Is Nothing Then
                Throw New ArgumentNullException(NameOf(clrType))
            End If
            If clrType.IsGenericType AndAlso Not clrType.IsGenericTypeDefinition Then
                Return clrType.AssemblyQualifiedName
            Else
                Return If(clrType.FullName, clrType.Name)
            End If
        End Function

        ''' <summary>
        ''' 该类型是否可以由本模块以"字段快照"的方式保存与还原。
        ''' </summary>
        ''' <param name="clrType">待检查的类型</param>
        Public Function IsSupportedType(clrType As Type) As Boolean
            If clrType Is Nothing Then
                Return False
            End If
            If clrType.IsPrimitive OrElse clrType.IsEnum OrElse clrType.IsArray Then
                Return False
            End If
            If clrType.IsAbstract OrElse clrType.IsInterface OrElse clrType.ContainsGenericParameters Then
                ' 抽象类/接口无法实例化；开放泛型没有具体的类型实参
                Return False
            End If
            If clrType Is GetType(String) OrElse clrType Is GetType(Decimal) OrElse clrType Is GetType(DateTime) Then
                Return False
            End If
            If clrType Is GetType(TimeSpan) OrElse clrType Is GetType(DateTimeOffset) OrElse clrType Is GetType(Guid) Then
                Return False
            End If
            If clrType Is GetType(Uri) OrElse clrType Is GetType(IntPtr) OrElse clrType Is GetType(UIntPtr) Then
                Return False
            End If
            If GetType([Delegate]).IsAssignableFrom(clrType) OrElse GetType(MemberInfo).IsAssignableFrom(clrType) Then
                Return False
            End If

            Return True
        End Function

        ''' <summary>
        ''' 按 pickle 的全局符号重建 CLR 对象；返回 <c>Nothing</c> 表示该符号不由本模块处理。
        ''' </summary>
        ''' <param name="moduleName">pickle <c>GLOBAL</c> 的模块名</param>
        ''' <param name="className">pickle <c>GLOBAL</c> 的类名</param>
        ''' <param name="allowLegacyName">
        ''' 当模块名不含 <c>clr:</c> 前缀时是否仍然尝试解析（用于兼容旧版本写出的
        ''' "命名空间 + 类名" 形式）
        ''' </param>
        Public Function Construct(moduleName As String, className As String, Optional allowLegacyName As Boolean = False) As Object
            If String.IsNullOrEmpty(moduleName) OrElse String.IsNullOrEmpty(className) Then
                Return Nothing
            End If
            If Not moduleName.StartsWith(ModulePrefix, StringComparison.Ordinal) AndAlso Not allowLegacyName Then
                Return Nothing
            End If

            Dim clrType As Type = ResolveType(moduleName, className)

            If clrType Is Nothing Then
                Return Nothing
            End If
            If Not IsSupportedType(clrType) Then
                Return Nothing
            End If

            Try
                Return CreateInstance(clrType)
            Catch ex As Exception
                ' 无法实例化时退回默认处理（PythonObject 包装器）
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' 解析类型标识为 <see cref="Type"/>，解析失败返回 <c>Nothing</c>。
        ''' </summary>
        ''' <param name="moduleName">pickle <c>GLOBAL</c> 的模块名（可为 <c>clr:程序集名</c> 或命名空间）</param>
        ''' <param name="className">类型全名或程序集限定名</param>
        Public Function ResolveType(moduleName As String, className As String) As Type
            Dim [namespace] As String = If(moduleName, "")
            Dim key As String = [namespace] & "|" & className

            SyncLock _types
                Dim cached As Type = Nothing

                If _types.TryGetValue(key, cached) Then
                    Return cached
                End If
            End SyncLock

            Dim resolved As Type = ResolveTypeCore([namespace], className)

            If resolved IsNot Nothing Then
                SyncLock _types
                    _types(key) = resolved
                End SyncLock
            End If

            Return resolved
        End Function

        Private Function ResolveTypeCore(moduleName As String, className As String) As Type
            Dim assemblyName As String = Nothing

            If moduleName.StartsWith(ModulePrefix, StringComparison.Ordinal) Then
                assemblyName = moduleName.Substring(ModulePrefix.Length).Trim()
            End If

            ' 1) 形如 "Ns.Type, Assembly" 的程序集限定名
            If className.IndexOf(","c) >= 0 Then
                Dim qualified As Type = Type.GetType(className, throwOnError:=False)

                If qualified IsNot Nothing Then
                    Return qualified
                End If
            End If

            ' 2) 指定程序集 + 类型全名
            If Not String.IsNullOrEmpty(assemblyName) Then
                Dim assembly As Assembly = FindAssembly(assemblyName)

                If assembly IsNot Nothing Then
                    Dim fromAssembly As Type = assembly.GetType(className, throwOnError:=False, ignoreCase:=False)

                    If fromAssembly IsNot Nothing Then
                        Return fromAssembly
                    End If
                End If

                Dim scoped As Type = Type.GetType(className & ", " & assemblyName, throwOnError:=False)

                If scoped IsNot Nothing Then
                    Return scoped
                End If
            End If

            ' 3) 在所有已加载的程序集中按全名查找（兼容程序集改名/升级，以及旧文件的命名空间形式）
            Dim fullName As String = If(String.IsNullOrEmpty(assemblyName), moduleName & "." & className, className)
            Dim fallback As Type = FindTypeInLoadedAssemblies(fullName)

            If fallback IsNot Nothing Then
                Return fallback
            End If

            ' 4) 未限定程序集的 Type.GetType
            Return Type.GetType(fullName, throwOnError:=False)
        End Function

        Private Function FindAssembly(assemblyName As String) As Assembly
            For Each assembly As Assembly In AppDomain.CurrentDomain.GetAssemblies()
                Try
                    If String.Equals(assembly.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase) Then
                        Return assembly
                    End If
                Catch ex As Exception
                    ' 动态程序集等可能无法读取名称
                End Try
            Next

            Try
                Return Assembly.Load(New AssemblyName(assemblyName))
            Catch ex As Exception
                ' 加载失败时继续尝试按文件名加载
            End Try

            Try
                Dim file As String = System.IO.Path.Combine(AppContext.BaseDirectory, assemblyName & ".dll")

                If System.IO.File.Exists(file) Then
                    Return Assembly.LoadFrom(file)
                End If
            Catch ex As Exception
                ' 无法从输出目录加载
            End Try

            Return Nothing
        End Function

        Private Function FindTypeInLoadedAssemblies(fullName As String) As Type
            For Each assembly As Assembly In AppDomain.CurrentDomain.GetAssemblies()
                Try
                    Dim found As Type = assembly.GetType(fullName, throwOnError:=False, ignoreCase:=False)

                    If found IsNot Nothing Then
                        Return found
                    End If
                Catch ex As Exception
                    ' 跳过无法查询的程序集
                End Try
            Next

            Return Nothing
        End Function

        Private Function CreateInstance(clrType As Type) As Object
            If clrType.IsValueType Then
                Return Activator.CreateInstance(clrType)
            End If

            Dim [default] As ConstructorInfo = clrType.GetConstructor(
                BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic,
                binder:=Nothing,
                types:=Type.EmptyTypes,
                modifiers:=Nothing)

            If [default] IsNot Nothing Then
                Try
                    Return [default].Invoke(Array.Empty(Of Object)())
                Catch ex As Exception
                    ' 构造函数内部抛异常时退化为"未初始化对象"
                End Try
            End If

            Return RuntimeHelpers.GetUninitializedObject(clrType)
        End Function

        ''' <summary>
        ''' 判断某个由 pickle 栈给出的对象是否可以通过 <see cref="ApplyState"/> 回填状态。
        ''' </summary>
        ''' <param name="obj">目标对象</param>
        ''' <param name="state">pickle <c>BUILD</c> 的状态对象</param>
        Public Function CanApplyState(obj As Object, state As Object) As Boolean
            If obj Is Nothing OrElse state Is Nothing Then
                Return False
            End If
            If Not TypeOf state Is IDictionary Then
                Return False
            End If
            If TypeOf obj Is PythonObject OrElse TypeOf obj Is IDictionary OrElse TypeOf obj Is String Then
                Return False
            End If

            Return True
        End Function

        ''' <summary>
        ''' 把 <c>BUILD</c> 携带的状态字典回填到 CLR 对象的字段（或可写属性）上。
        ''' </summary>
        ''' <param name="obj">目标对象（装箱的结构体同样可以就地回填）</param>
        ''' <param name="state">"字段名 → 值" 状态字典</param>
        ''' <returns>是否至少成功回填了一个成员</returns>
        Public Function ApplyState(ByRef obj As Object, state As Object) As Boolean
            If Not CanApplyState(obj, state) Then
                Return False
            End If

            Dim clrType As Type = obj.GetType()

            If clrType.IsValueType Then
                ' 装箱的结构体无法用 FieldInfo.SetValue 就地修改（反射会作用于副本），
                ' 必须用表达式树生成一个"带值的新实例"再替换掉栈上的装箱对象
                Return ApplyValueTypeState(obj, clrType, state)
            End If

            Dim layout As FieldLayout = GetLayout(clrType)
            Dim applied As Boolean = False

            For Each entry As DictionaryEntry In DirectCast(state, IDictionary)
                Dim key As String = TryCast(entry.Key, String)

                If key Is Nothing Then
                    Continue For
                End If

                Dim field As FieldInfo = Nothing

                If layout.Map.TryGetValue(key, field) Then
                    If SetField(field, obj, entry.Value) Then
                        applied = True
                    End If
                Else
                    Dim [property] As PropertyInfo = FindWritableProperty(clrType, key)

                    If [property] IsNot Nothing Then
                        Dim value As Object = CoerceValue(entry.Value, [property].PropertyType)

                        If value IsNot Nothing OrElse Not [property].PropertyType.IsValueType Then
                            Try
                                Call [property].SetValue(obj, value)
                                applied = True
                            Catch ex As Exception
                                ' 属性 setter 抛出异常时忽略该项
                            End Try
                        End If
                    End If
                End If
            Next

            Return applied
        End Function

        Private Function SetField(field As FieldInfo, obj As Object, rawValue As Object) As Boolean
            Dim value As Object = CoerceValue(rawValue, field.FieldType)

            If value Is Nothing AndAlso field.FieldType.IsValueType Then
                ' 值类型字段不能用 Nothing 赋值，保留默认值
                Return False
            End If

            Try
                Call field.SetValue(obj, value)
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        ''' <summary>
        ''' 用"带值的新实例"替换装箱的结构体。
        ''' </summary>
        ''' <remarks>
        ''' 反射的 <c>FieldInfo.SetValue</c> 对装箱的值类型会作用于副本（调用成功但原装箱对象不受影响），
        ''' 因此这里用表达式树编译出 <c>Object() → Object</c> 的构造器：在方法内部声明一个结构体局部变量、
        ''' 逐字段赋值后再装箱返回，从而得到正确的新值。
        ''' </remarks>
        Private Function ApplyValueTypeState(ByRef obj As Object, clrType As Type, state As Object) As Boolean
            Dim entries As IDictionary = DirectCast(state, IDictionary)
            Dim layout As FieldLayout = GetLayout(clrType)

            If layout.Fields.Length = 0 Then
                Return False
            End If

            Dim values As Object() = New Object(layout.Fields.Length - 1) {}
            Dim applied As Boolean = False

            For i As Integer = 0 To layout.Fields.Length - 1
                Dim field As FieldInfo = layout.Fields(i)

                If entries.Contains(field.Name) Then
                    values(i) = CoerceValue(entries(field.Name), field.FieldType)

                    If values(i) IsNot Nothing Then
                        applied = True
                    End If
                End If
            Next

            If Not applied Then
                Return False
            End If

            Dim builder As Func(Of Object(), Object) = GetStructBuilder(clrType)

            If builder Is Nothing Then
                Return False
            End If

            obj = builder(values)

            Return True
        End Function

        Private Function GetStructBuilder(clrType As Type) As Func(Of Object(), Object)
            SyncLock _structBuilders
                Dim cached As Func(Of Object(), Object) = Nothing

                If _structBuilders.TryGetValue(clrType, cached) Then
                    Return cached
                End If
            End SyncLock

            Dim builder As Func(Of Object(), Object)

            Try
                builder = CompileStructBuilder(clrType)
            Catch ex As Exception
                builder = Nothing
            End Try

            SyncLock _structBuilders
                _structBuilders(clrType) = builder
            End SyncLock

            Return builder
        End Function

        Private Function CompileStructBuilder(clrType As Type) As Func(Of Object(), Object)
            Dim layout As FieldLayout = GetLayout(clrType)
            Dim valuesParam As ParameterExpression = Expression.Parameter(GetType(Object()), "values")
            Dim local As ParameterExpression = Expression.Variable(clrType, "value")
            ' default(T) 即结构体的零值（VB 中无法写 Expression.New(...)：New 是关键字）
            Dim body As New List(Of Expression) From {Expression.Assign(local, Expression.Default(clrType))}

            For i As Integer = 0 To layout.Fields.Length - 1
                Dim field As FieldInfo = layout.Fields(i)

                If field.IsInitOnly OrElse field.IsLiteral Then
                    ' 只读字段无法在表达式树中赋值，保持默认值
                    Continue For
                End If

                Dim item As Expression = Expression.ArrayIndex(valuesParam, Expression.Constant(i))
                Dim hasValue As Expression = Expression.NotEqual(item, Expression.Constant(Nothing))
                Dim value As Expression = Expression.Condition(
                    hasValue,
                    Expression.Convert(item, field.FieldType),
                    Expression.Default(field.FieldType))

                Call body.Add(Expression.Assign(Expression.Field(local, field), value))
            Next

            Call body.Add(Expression.Convert(local, GetType(Object)))

            Dim block As Expression = Expression.Block(New ParameterExpression() {local}, body)

            Return Expression.Lambda(Of Func(Of Object(), Object))(block, valuesParam).Compile()
        End Function

        Private Function FindWritableProperty(clrType As Type, name As String) As PropertyInfo
            Dim [property] As PropertyInfo = clrType.GetProperty(
                name,
                BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic)

            If [property] Is Nothing Then
                Return Nothing
            End If
            If Not [property].CanWrite Then
                Return Nothing
            End If
            If [property].GetIndexParameters().Length > 0 Then
                Return Nothing
            End If

            Return [property]
        End Function

        ''' <summary>
        ''' 采集类型实例上需要序列化的字段（含基类字段，跳过 <c>&lt;NonSerialized&gt;</c> 与委托）。
        ''' </summary>
        ''' <param name="clrType">目标类型</param>
        Public Function GetSerializableFields(clrType As Type) As FieldInfo()
            Return GetLayout(clrType).Fields
        End Function

        Private Function GetLayout(clrType As Type) As FieldLayout
            SyncLock _layouts
                Dim cached As FieldLayout = Nothing

                If _layouts.TryGetValue(clrType, cached) Then
                    Return cached
                End If
            End SyncLock

            Dim fields As New List(Of FieldInfo)()
            Dim map As New Dictionary(Of String, FieldInfo)(StringComparer.Ordinal)
            Dim current As Type = clrType

            While current IsNot Nothing AndAlso current IsNot GetType(Object) AndAlso current IsNot GetType(ValueType)
                Dim declared As FieldInfo() = current.GetFields(
                    BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.DeclaredOnly)

                For Each field As FieldInfo In declared
                    If field.IsNotSerialized Then
                        Continue For
                    End If
                    If GetType([Delegate]).IsAssignableFrom(field.FieldType) Then
                        Continue For
                    End If
                    If map.ContainsKey(field.Name) Then
                        ' 派生类与基类同名字段（shadowing）时以派生类为准
                        Continue For
                    End If

                    map(field.Name) = field
                    Call fields.Add(field)
                Next

                current = current.BaseType
            End While

            Dim layout As New FieldLayout(fields.ToArray(), map)

            SyncLock _layouts
                _layouts(clrType) = layout
            End SyncLock

            Return layout
        End Function

        ''' <summary>
        ''' 把 pickle 反序列化出来的宽松值（<c>Long</c>/<c>Double</c>/<c>List(Of Object)</c>/<c>Dictionary(Of Object, Object)</c> 等）
        ''' 转换成目标成员声明的类型。
        ''' </summary>
        ''' <param name="rawValue">来自 pickle 的值</param>
        ''' <param name="targetType">目标成员的声明类型</param>
        Public Function CoerceValue(rawValue As Object, targetType As Type) As Object
            If targetType Is Nothing OrElse targetType Is GetType(Object) Then
                Return rawValue
            End If

            Dim underlying As Type = Nullable.GetUnderlyingType(targetType)

            If underlying IsNot Nothing Then
                If rawValue Is Nothing Then
                    Return Nothing
                End If

                targetType = underlying
            End If

            If rawValue Is Nothing Then
                Return Nothing
            End If
            If targetType.IsInstanceOfType(rawValue) Then
                Return rawValue
            End If

            ' ----- 枚举 -----
            If targetType.IsEnum Then
                Try
                    If TypeOf rawValue Is String Then
                        Return [Enum].Parse(targetType, CStr(rawValue), ignoreCase:=True)
                    Else
                        Dim numeric As Object = Convert.ChangeType(rawValue, [Enum].GetUnderlyingType(targetType), CultureInfo.InvariantCulture)

                        Return [Enum].ToObject(targetType, numeric)
                    End If
                Catch ex As Exception
                    Return rawValue
                End Try
            End If

            ' ----- 字符串补回特定类型（pickler 把它们写成了字符串）-----
            Dim text As String = TryCast(rawValue, String)

            If text IsNot Nothing Then
                Select Case True
                    Case targetType Is GetType(Guid)
                        Dim guid As Guid

                        If Guid.TryParse(text, guid) Then
                            Return guid
                        End If
                    Case targetType Is GetType(Uri)
                        Return New Uri(text, UriKind.RelativeOrAbsolute)
                    Case targetType Is GetType(DateTime)
                        Dim dt As DateTime

                        If DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, dt) Then
                            Return dt
                        End If
                    Case targetType Is GetType(DateTimeOffset)
                        Dim dto As DateTimeOffset

                        If DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, dto) Then
                            Return dto
                        End If
                    Case targetType Is GetType(TimeSpan)
                        Dim ts As TimeSpan

                        If TimeSpan.TryParse(text, CultureInfo.InvariantCulture, ts) Then
                            Return ts
                        End If
                    Case targetType Is GetType(DateOnly)
                        Dim day As DateOnly

                        If DateOnly.TryParse(text, CultureInfo.InvariantCulture, day) Then
                            Return day
                        End If
                    Case targetType Is GetType(TimeOnly)
                        Dim time As TimeOnly

                        If TimeOnly.TryParse(text, CultureInfo.InvariantCulture, time) Then
                            Return time
                        End If
                    Case targetType Is GetType(Type)
                        Return Type.GetType(text, throwOnError:=False)
                    Case targetType Is GetType(Char)
                        If text.Length > 0 Then
                            Return text(0)
                        End If
                End Select
            End If

            ' ----- 数值 / 布尔 / 字符串 -----
            If targetType.IsPrimitive OrElse targetType Is GetType(Decimal) OrElse targetType Is GetType(String) Then
                Try
                    Return Convert.ChangeType(rawValue, targetType, CultureInfo.InvariantCulture)
                Catch ex As Exception
                    Return rawValue
                End Try
            End If

            ' ----- IntPtr / UIntPtr -----
            If targetType Is GetType(IntPtr) Then
                Return New IntPtr(Convert.ToInt64(rawValue, CultureInfo.InvariantCulture))
            End If
            If targetType Is GetType(UIntPtr) Then
                Return New UIntPtr(Convert.ToUInt64(rawValue, CultureInfo.InvariantCulture))
            End If

            ' ----- 数组 -----
            If targetType.IsArray Then
                Dim elementType As Type = targetType.GetElementType()
                Dim items As List(Of Object) = AsObjectList(rawValue)

                If items Is Nothing Then
                    Return Nothing
                End If

                Dim array As Array = Array.CreateInstance(elementType, items.Count)

                For i As Integer = 0 To items.Count - 1
                    Call array.SetValue(CoerceValue(items(i), elementType), i)
                Next

                Return array
            End If

            ' ----- 泛型集合 -----
            If targetType.IsGenericType Then
                Dim definition As String = targetType.GetGenericTypeDefinition().FullName
                Dim arguments As Type() = targetType.GetGenericArguments()

                Select Case definition
                    Case "System.Collections.Generic.List`1"
                        Return BuildGenericList(targetType, arguments(0), rawValue)
                    Case "System.Collections.Generic.IList`1"
                        Return BuildGenericList(targetType, arguments(0), rawValue)
                    Case "System.Collections.Generic.ICollection`1"
                        Return BuildGenericList(targetType, arguments(0), rawValue)
                    Case "System.Collections.Generic.IEnumerable`1"
                        Return BuildGenericList(targetType, arguments(0), rawValue)
                    Case "System.Collections.Generic.IReadOnlyList`1"
                        Return BuildGenericList(targetType, arguments(0), rawValue)
                    Case "System.Collections.Generic.IReadOnlyCollection`1"
                        Return BuildGenericList(targetType, arguments(0), rawValue)
                    Case "System.Collections.Generic.HashSet`1"
                        Return BuildGenericList(targetType, arguments(0), rawValue)
                    Case "System.Collections.Generic.ISet`1"
                        Return BuildGenericList(targetType, arguments(0), rawValue)
                    Case "System.Collections.ObjectModel.Collection`1"
                        Return BuildGenericList(targetType, arguments(0), rawValue)
                    Case "System.Collections.ObjectModel.ObservableCollection`1"
                        Return BuildGenericList(targetType, arguments(0), rawValue)

                    Case "System.Collections.Generic.Dictionary`2"
                        Return BuildGenericDictionary(targetType, arguments(0), arguments(1), rawValue)
                    Case "System.Collections.Generic.IDictionary`2"
                        Return BuildGenericDictionary(targetType, arguments(0), arguments(1), rawValue)
                    Case "System.Collections.Generic.IReadOnlyDictionary`2"
                        Return BuildGenericDictionary(targetType, arguments(0), arguments(1), rawValue)
                End Select
            End If

            ' ----- 非泛型集合接口 -----
            If targetType Is GetType(ArrayList) Then
                Dim items As List(Of Object) = AsObjectList(rawValue)

                If items IsNot Nothing Then
                    Return New ArrayList(items.ToArray())
                End If
            End If

            If targetType.IsAssignableFrom(rawValue.GetType()) Then
                Return rawValue
            End If

            ' 无法确定的成员：原样返回，由反射 setter 决定是否抛异常
            Return rawValue
        End Function

        Private Function BuildGenericList(targetType As Type, elementType As Type, rawValue As Object) As Object
            Dim items As List(Of Object) = AsObjectList(rawValue)

            If items Is Nothing Then
                Return Nothing
            End If

            Dim concrete As Type = targetType

            If targetType.IsInterface Then
                concrete = GetType(List(Of )).MakeGenericType(elementType)
            End If

            Dim instance As Object

            Try
                instance = Activator.CreateInstance(concrete)
            Catch ex As Exception
                Return Nothing
            End Try

            Dim add As MethodInfo = concrete.GetMethod("Add", New Type() {elementType})

            If add Is Nothing Then
                Return instance
            End If

            For Each item As Object In items
                Try
                    Call add.Invoke(instance, New Object() {CoerceValue(item, elementType)})
                Catch ex As Exception
                    ' 跳过无法转换的元素
                End Try
            Next

            Return instance
        End Function

        Private Function BuildGenericDictionary(targetType As Type, keyType As Type, valueType As Type, rawValue As Object) As Object
            Dim entries As IDictionary = TryCast(rawValue, IDictionary)

            If entries Is Nothing Then
                Return Nothing
            End If

            Dim concrete As Type = targetType

            If targetType.IsInterface Then
                concrete = GetType(Dictionary(Of ,)).MakeGenericType(keyType, valueType)
            End If

            Dim instance As Object

            Try
                instance = Activator.CreateInstance(concrete)
            Catch ex As Exception
                Return Nothing
            End Try

            Dim add As MethodInfo = concrete.GetMethod("Add", New Type() {keyType, valueType})

            If add Is Nothing Then
                Return instance
            End If

            For Each entry As DictionaryEntry In entries
                Try
                    Call add.Invoke(instance, New Object() {
                        CoerceValue(entry.Key, keyType),
                        CoerceValue(entry.Value, valueType)
                    })
                Catch ex As Exception
                    ' 跳过无法转换的键值对
                End Try
            Next

            Return instance
        End Function

        Private Function AsObjectList(rawValue As Object) As List(Of Object)
            Dim [tuple] As PythonTuple = TryCast(rawValue, PythonTuple)

            If [tuple] IsNot Nothing Then
                Return New List(Of Object)([tuple].Items)
            End If

            Dim list As IList = TryCast(rawValue, IList)

            If list IsNot Nothing Then
                Dim buffer As New List(Of Object)(list.Count)

                For Each item As Object In list
                    Call buffer.Add(item)
                Next

                Return buffer
            End If

            Dim [set] As PythonSet = TryCast(rawValue, PythonSet)

            If [set] IsNot Nothing Then
                Return New List(Of Object)([set])
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' 把对象状态字典转换为便于阅读的文本（调试用）。
        ''' </summary>
        ''' <param name="state">状态字典</param>
        Public Function DescribeState(state As Object) As String
            Dim entries As IDictionary = TryCast(state, IDictionary)

            If entries Is Nothing Then
                Return If(state Is Nothing, "None", state.ToString())
            End If

            Dim parts As New List(Of String)()

            For Each entry As DictionaryEntry In entries
                Call parts.Add($"{entry.Key}={If(entry.Value Is Nothing, "None", entry.Value.ToString())}")
            Next

            Return "{" & String.Join(", ", parts) & "}"
        End Function

    End Module

End Namespace
