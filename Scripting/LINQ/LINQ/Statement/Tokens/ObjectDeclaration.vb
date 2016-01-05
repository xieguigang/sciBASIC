Namespace Statements.Tokens

    Public Class ObjectDeclaration : Inherits LINQ.Statements.Tokens.Token

        ''' <summary>
        ''' 变量的名称
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Name As String
        ''' <summary>
        ''' 变量的类型标识符
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property TypeId As String

        Public Property RegistryType As LINQ.Framework.TypeRegistry.RegistryItem

        Friend SetObject As System.Reflection.MethodInfo

        Sub New(Statement As LINQ.Statements.LINQStatement)
            Me.Statement = Statement
            Me.TryParse()
            Me.RegistryType = Statement.TypeRegistry.Find(TypeId)
            If RegistryType Is Nothing Then
                Throw New LINQ.Framework.LQueryFramework.TypeMissingExzception("Could not found any information about the type {0}.", TypeId)
            Else
                Dim ILINQCollection As System.Type = Tokens.ObjectCollection.LoadExternalModule(RegistryType)
                Statement.Collection.ILINQCollection = Activator.CreateInstance(ILINQCollection)
                Me.TypeId = Statement.Collection.ILINQCollection.GetEntityType.FullName
            End If
        End Sub

        Private Sub TryParse()
            Dim str = LINQ.Statements.Tokens.ReadOnlyObject.Parser.GetStatement(Statement._OriginalCommand, New String() {"from", "in"}, True)
            Dim Tokens As String() = str.Split
            Name = Tokens.First
            TypeId = Tokens.Last
            Me._OriginalCommand = str
        End Sub

        Public Overridable Function ToFieldDeclaration() As CodeDom.CodeMemberField
            Dim CodeMemberField = New CodeDom.CodeMemberField(TypeId, Name)
            CodeMemberField.Attributes = CodeDom.MemberAttributes.Public

            Return CodeMemberField
        End Function

        Public Sub Initialize()
            Me.SetObject = LINQ.Framework.DynamicCode.DynamicInvoke.GetMethod(Statement.ILINQProgram, Framework.DynamicCode.VBC.DynamicCompiler.SetObjectName)
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("Dim {0} As {1}", Name, TypeId)
        End Function
    End Class
End Namespace