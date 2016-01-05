Imports System.Text
Imports System.CodeDom.Compiler

Namespace Framework.DynamicCode.VBC

    ''' <summary>
    ''' 编译整个LINQ语句的动态代码编译器
    ''' </summary>
    ''' <remarks></remarks>
    Public Class DynamicCompiler : Implements System.IDisposable

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks></remarks>
        Dim DotNETReferenceAssembliesDir As String
        Dim LINQStatement As LINQ.Statements.LINQStatement

        Public Const ModuleName As String = "ILINQProgram"
        Public Const SetObjectName As String = "SetObject"

        Dim ObjectModel As CodeDom.CodeNamespace

        Public ReadOnly Property CompiledCode As String
            Get
                Return GenerateCode(ObjectModel)
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="LINQStatement"></param>
        ''' <param name="SDK">.NET Framework Reference Assembly文件夹的位置</param>
        ''' <remarks></remarks>
        Sub New(LINQStatement As LINQ.Statements.LINQStatement, SDK As String)
            Me.LINQStatement = LINQStatement
            Me.DotNETReferenceAssembliesDir = SDK
        End Sub

        Public Function DeclareAssembly() As CodeDom.CodeCompileUnit
            Dim Assembly As CodeDom.CodeCompileUnit = New CodeDom.CodeCompileUnit
            Dim DynamicCodeNameSpace As CodeDom.CodeNamespace = New CodeDom.CodeNamespace("LINQDynamicCodeCompiled")
            Assembly.Namespaces.Add(DynamicCodeNameSpace)

            DynamicCodeNameSpace.Types.Add(DeclareType)
            DynamicCodeNameSpace.Imports.AddRange(New String() {}.ImportsNamespace)
#If DEBUG Then
            Console.WriteLine(GenerateCode(DynamicCodeNameSpace))
#End If
            ObjectModel = DynamicCodeNameSpace

            Return Assembly
        End Function

        Private Function DeclareType() As CodeDom.CodeTypeDeclaration
            Dim [Module] = DynamicCode.VBC.TokenCompiler.DeclareType(ModuleName, LINQStatement.Object, LINQStatement.ReadOnlyObjects)
            Call [Module].Members.Add(New DynamicCode.VBC.WhereConditionTestCompiler(LINQStatement).Compile)
            Call [Module].Members.Add(DeclareSetObject)
            Call [Module].Members.Add(New DynamicCode.VBC.SelectConstructCompiler(LINQStatement).Compile)

            Return [Module]
        End Function

        Private Function DeclareSetObject() As CodeDom.CodeMemberMethod
            Dim StatementCollection As CodeDom.CodeStatementCollection = New CodeDom.CodeStatementCollection
            Call StatementCollection.Add(New CodeDom.CodeAssignStatement(New CodeDom.CodeFieldReferenceExpression(New CodeDom.CodeThisReferenceExpression(), LINQStatement.Object.Name), New CodeDom.CodeArgumentReferenceExpression("p")))
            For Each ReadOnlyObject In LINQStatement.ReadOnlyObjects
                Call StatementCollection.Add(New DynamicCode.VBC.ReadOnlyObjectCompiler(ReadOnlyObject).Compile)
            Next

            Dim SetObject As CodeDom.CodeMemberMethod = DeclareFunction(SetObjectName, "System.Boolean", StatementCollection)
            SetObject.Parameters.Add(New CodeDom.CodeParameterDeclarationExpression(LINQStatement.Object.TypeId, "p"))
            SetObject.Attributes = CodeDom.MemberAttributes.Public
            Return SetObject
        End Function

        Public Function Compile(ReferenceAssemblys As String()) As System.Reflection.Assembly
            Dim Code = DeclareAssembly()
            Dim Assembly = CodeDOMExtension.Compile(Code, ReferenceAssemblys, DotNETReferenceAssembliesDir)
            Return Assembly
        End Function

        ''' <summary>
        ''' Declare a function with a specific function name and return type. please notice that in this newly 
        ''' declare function there is always a local variable name rval using for return the value.
        ''' (申明一个方法，返回指定类型的数据并且具有一个特定的函数名，请注意，在这个新申明的函数之中，
        ''' 固定含有一个rval的局部变量用于返回数据)
        ''' </summary>
        ''' <param name="Name">Function name.(函数名)</param>
        ''' <param name="Type">Function return value type.(该函数的返回值类型)</param>
        ''' <returns>A codeDOM object model of the target function.(一个函数的CodeDom对象模型)</returns>
        ''' <remarks></remarks>
        Public Shared Function DeclareFunction(Name As String, Type As String, Statements As CodeDom.CodeStatementCollection) As CodeDom.CodeMemberMethod
            Dim CodeMemberMethod As CodeDom.CodeMemberMethod = New CodeDom.CodeMemberMethod()
            CodeMemberMethod.Name = Name : CodeMemberMethod.ReturnType = New CodeDom.CodeTypeReference(Type) '创建一个名为“WhereTest”，返回值类型为Boolean的无参数的函数
            If String.Equals(Type, "System.Boolean", StringComparison.OrdinalIgnoreCase) Then
                CodeMemberMethod.Statements.Add(New CodeDom.CodeVariableDeclarationStatement(Type, "rval", New CodeDom.CodePrimitiveExpression(True)))   '创建一个用于返回值的局部变量，对于逻辑值，默认为真
            Else
                CodeMemberMethod.Statements.Add(New CodeDom.CodeVariableDeclarationStatement(Type, "rval"))   '创建一个用于返回值的局部变量
            End If

            If Not (Statements Is Nothing OrElse Statements.Count = 0) Then
                CodeMemberMethod.Statements.AddRange(Statements)
            End If
            CodeMemberMethod.Statements.Add(New CodeDom.CodeMethodReturnStatement(New CodeDom.CodeVariableReferenceExpression("rval")))  '引用返回值的局部变量

            Return CodeMemberMethod
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' 检测冗余的调用

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
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