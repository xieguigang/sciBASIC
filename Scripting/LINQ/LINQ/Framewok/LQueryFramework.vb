Namespace Framework

    Public Class LQueryFramework : Implements System.IDisposable

        ''' <summary>
        ''' LINQ查询框架的默认注册表文件的文件名
        ''' </summary>
        ''' <remarks></remarks>
        Public Const DefaultFile As String = ".\LINQ.Framework.TypeDef.xml"

        Public Property TypeRegistry As TypeRegistry
        Public Property Runtime As LINQ.Script.I_DynamicsRuntime

        ''' <summary>
        ''' 本模块的完整的文件路径
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared ReadOnly Property ReferenceAssembly As String
            Get
                Dim PathValue As String = String.Format("{0}/linq.exe", System.Windows.Forms.Application.StartupPath)
                Return IO.Path.GetFullPath(PathValue)
            End Get
        End Property

        Sub New(Optional TypeDef As String = DefaultFile)
            TypeRegistry = LINQ.Framework.TypeRegistry.Load(TypeDef)
        End Sub

        ''' <summary>
        ''' 加载外部模块，并查询出目标类型的ILINQCollection接口类型信息
        ''' </summary>
        ''' <param name="TypeId">目标对象的类型标识符，即RegistryItem对象中的Name属性</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadExternalModule(TypeId As String) As System.Type
            Dim RegistryItem = TypeRegistry.Find(TypeId)

            If RegistryItem Is Nothing Then
                Throw New TypeMissingExzception("There is a type missing error in this linq statement, where could not found any type registry information for type id ""{0}""", TypeId)
            End If

            Dim Assembly As System.Reflection.Assembly = System.Reflection.Assembly.LoadFrom(RegistryItem.AssemblyFullPath)
            Dim ILINQCollection As System.Type = Assembly.GetType(RegistryItem.TypeId, False, False)

            If ILINQCollection Is Nothing Then
                Throw New TypeMissingExzception("There is a type missing error while trying to load the {0} ILINQ interface type definition.", TypeId)
            Else
                Return ILINQCollection
            End If
        End Function

        ''' <summary>
        ''' 查找出目标模块之中的含有指定的自定义属性的所有类型
        ''' </summary>
        ''' <param name="Assembly"></param>
        ''' <param name="FindEntry">目标自定义属性的类型</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function LoadAssembly(Assembly As System.Reflection.Assembly, FindEntry As System.Type) As System.Reflection.TypeInfo()
            Dim LQuery = From [Module] As System.Reflection.TypeInfo
                         In Assembly.DefinedTypes
                         Let attrs = [Module].GetCustomAttributes(FindEntry, inherit:=False)
                         Where Not attrs Is Nothing AndAlso attrs.Count = 1
                         Select [Module] '
            Return LQuery.ToArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="PropertyName"></param>
        ''' <param name="Target"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetValue(PropertyName As String, Target As Object) As Object
            Dim Type As System.Type = Target.GetType
            Dim PropertyInfoCollection = Type.GetProperties
            Dim LQuery = From [Property] As System.Reflection.PropertyInfo In PropertyInfoCollection Where String.Equals([Property].Name, PropertyName, StringComparison.OrdinalIgnoreCase) Select [Property] '

            Dim Result = LQuery.ToArray
            If Result.Count > 0 Then
                Dim PropertyInfo As System.Reflection.PropertyInfo = Result.First
                Return PropertyInfo.GetValue(Target)
            Else
                Throw New DataException("No such a property named '" & PropertyName & "' in the target type information.")
            End If
        End Function

        ''' <summary>
        ''' Exception for [We could not found any registered type information from the type registry.]
        ''' </summary>
        ''' <remarks></remarks>
        Public Class TypeMissingExzception : Inherits Exception

            Dim _Msg As String

            Public Overrides ReadOnly Property Message As String
                Get
                    Return _Msg
                End Get
            End Property

            Sub New(Msg As String, ParamArray arg As String())
                _Msg = String.Format(Msg, arg)
            End Sub

            Public Overrides Function ToString() As String
                Return Message
            End Function
        End Class

        ''' <summary>
        ''' Execute a compiled LINQ statement object model to query a object-orientale database.
        ''' </summary>
        ''' <param name="Statement"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Dim List As List(Of Object) = New List(Of Object)
        ''' 
        ''' For Each [Object] In LINQ.GetCollection(Statement)
        '''    Call SetObject([Object])
        '''    If True = Test() Then
        '''        List.Add(SelectConstruct())
        '''    End If
        ''' Next
        ''' Return List.ToArray
        ''' </remarks>
        Public Function EXEC(Statement As LINQ.Statements.LINQStatement) As Object()
            Using ObjectModel As LINQ.Framework.ObjectModel.LINQ = CreateObjectModel(Statement)
                Return ObjectModel.EXEC
            End Using
        End Function

        ''' <summary>
        ''' 创建一个LINQ查询表达式的对象句柄
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Function CreateObjectModel(Statement As LINQ.Statements.LINQStatement) As LINQ.Framework.ObjectModel.LINQ
            If Statement.Collection.IsParallel Then
                Return New LINQ.Framework.ObjectModel.ParallelLINQ(Statement:=Statement, FrameworkRuntime:=Me.Runtime)
            Else
                Return New LINQ.Framework.ObjectModel.LINQ(Statement:=Statement, Runtime:=Me.Runtime)
            End If
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' 检测冗余的调用

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    Call TypeRegistry.Dispose()
                    ' TODO:  释放托管状态(托管对象)。
                End If

                ' TODO:  释放非托管资源(非托管对象)并重写下面的 Finalize()。
                ' TODO:  将大型字段设置为 null。
            End If
            Me.disposedValue = True
        End Sub

        ' TODO:  仅当上面的 Dispose(ByVal disposing As Boolean)具有释放非托管资源的代码时重写 Finalize()。
        'Protected Overrides Sub Finalize()
        '    ' 不要更改此代码。    请将清理代码放入上面的 Dispose(ByVal disposing As Boolean)中。
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' Visual Basic 添加此代码是为了正确实现可处置模式。
        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。    请将清理代码放入上面的 Dispose (disposing As Boolean)中。
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace