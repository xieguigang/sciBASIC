Imports System.Text.RegularExpressions
Imports System.Text

Namespace Statements.Tokens

    ''' <summary>
    ''' Object declared using a LET expression.(使用Let语句所声明的只读对象)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ReadOnlyObject : Inherits Statements.Tokens.ObjectDeclaration

        Friend Expression As CodeDom.CodeExpression

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Statement">LINQ表达式</param>
        ''' <param name="Parser">Parser实例</param>
        ''' <param name="Declare">所解析出来的申明语句</param>
        ''' <remarks></remarks>
        Sub New(Statement As LINQ.Statements.LINQStatement, Parser As LINQ.Parser.Parser, [Declare] As String)
            MyBase.New(Statement)
            Dim Name As String = Regex.Match([Declare], ".+?\=").Value
            MyBase.Name = Name.Replace("=", "").Trim
            MyBase.TypeId = Mid([Declare], Len(Name) + 1).Trim
            Me.Expression = Parser.ParseExpression(MyBase.TypeId)
        End Sub

        Public Overrides Function ToFieldDeclaration() As CodeDom.CodeMemberField
            Dim CodeMemberField = New CodeDom.CodeMemberField("System.Object", Name)
            CodeMemberField.Attributes = CodeDom.MemberAttributes.Public
            Return CodeMemberField
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("Let {0} = {1}", Name, MyBase.TypeId)
        End Function

        Public Class Parser

            Public Shared Function GetStatement(s As String, Tokens As String(), LTrimb As Boolean) As String
                Dim str As String = String.Format(" {0} .+ {1} ", Tokens(0), Tokens(1))
                Dim nL As Integer = 2
                If LTrimb Then
                    str = LTrim(str)
                    nL = 1
                End If
                Dim sBuilder As StringBuilder = New StringBuilder(Regex.Match(s, str, RegexOptions.IgnoreCase).Value)

                If sBuilder.Length > 0 Then
                    Call sBuilder.Remove(0, nL + Len(Tokens(0)))
                    Dim n = Len(Tokens(1)) + 2
                    Call sBuilder.Remove(sBuilder.Length - n, n)

                    Return sBuilder.ToString
                Else
                    Return ""
                End If
            End Function

            ''' <summary>
            ''' 获取Let只读对象的申明语句
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Private Shared Function GetLetStatements(Statement As LINQ.Statements.LINQStatement) As String()
                Dim str As String = GetStatement(Statement._OriginalCommand, New String() {"let", "where"}, False)
                If String.IsNullOrEmpty(str) Then
                    str = GetStatement(Statement._OriginalCommand, New String() {"let", "select"}, False)
                    If String.IsNullOrEmpty(str) Then
                        Return New String() {}
                    End If
                End If

                Dim objs = Strings.Split(str, " let ", -1, CompareMethod.Text).ToList
                str = GetStatement(Statement._OriginalCommand, New String() {"where", "select"}, False)
                If Not String.IsNullOrEmpty(str) Then
                    Dim sBuilder As StringBuilder = New StringBuilder(Regex.Match(str, " let .+", RegexOptions.IgnoreCase).Value)
                    If sBuilder.Length = 0 Then
                        Return objs.ToArray
                    Else
                        sBuilder.Remove(0, 5)
                        Call objs.AddRange(Strings.Split(sBuilder.ToString, " let ", -1, CompareMethod.Text))
                    End If
                End If

                Return objs.ToArray
            End Function

            Public Shared Function GetReadOnlyObjects(Statement As LINQ.Statements.LINQStatement) As LINQ.Statements.Tokens.ObjectDeclaration()
                Dim LetStatements As String() = GetLetStatements(Statement)
                Dim Parser As LINQ.Parser.Parser = New LINQ.Parser.Parser
                Dim ReadOnlyObjectList As List(Of Statements.Tokens.ReadOnlyObject) = New List(Of Tokens.ReadOnlyObject)
                For Each obj In LetStatements
                    Dim ReadOnlyObject As Tokens.ReadOnlyObject = New Tokens.ReadOnlyObject(Statement, Parser, obj)
                    Call ReadOnlyObjectList.Add(ReadOnlyObject)
                Next

                Return ReadOnlyObjectList.ToArray
            End Function
        End Class
    End Class
End Namespace