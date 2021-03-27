#Region "Microsoft.VisualBasic::d7cbb4b586290172266459ddb63244f0, Data\BinaryData\BinaryData\XDR\EmitContexts\SwitchModel.vb"

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

    '     Class SwitchModel
    ' 
    '         Properties: Branches, SwitchField
    ' 
    '         Function: BuildReadBranch, BuildReader, BuildWriteBranch, BuildWriter, Create
    '                   ThrowUnexpectedValue
    ' 
    '         Sub: AppendField
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Linq
Imports System.Reflection
Imports System.Collections.Generic
Imports System.Linq.Expressions

Namespace Xdr.EmitContexts
    Public Class SwitchModel
        Private _SwitchField As Xdr.EmitContexts.FieldDesc, _Branches As System.Collections.Generic.Dictionary(Of Object, Xdr.EmitContexts.FieldDesc)

        Public Property SwitchField As FieldDesc
            Get
                Return _SwitchField
            End Get
            Private Set(value As FieldDesc)
                _SwitchField = value
            End Set
        End Property

        Public Property Branches As Dictionary(Of Object, FieldDesc)
            Get
                Return _Branches
            End Get
            Private Set(value As Dictionary(Of Object, FieldDesc))
                _Branches = value
            End Set
        End Property

        Public Function BuildWriter(targetType As Type) As [Delegate]
            Dim pWriter = Expression.Parameter(GetType(Writer))
            Dim pItem = Expression.Parameter(targetType)
            Dim variables As List(Of ParameterExpression) = New List(Of ParameterExpression)()
            Dim body As List(Of Expression) = New List(Of Expression)()
            Dim [exit] As LabelTarget = Expression.Label()
            Dim cases As List(Of SwitchCase) = New List(Of SwitchCase)()

            For Each branch In Branches
                cases.Add(BuildWriteBranch(branch.Key, branch.Value, pWriter, pItem, [exit]))
            Next

            body.Add(Expression.Switch(Expression.PropertyOrField(pItem, SwitchField.MInfo.Name), Expression.Block(ThrowUnexpectedValue(Expression.PropertyOrField(pItem, SwitchField.MInfo.Name))), cases.ToArray()))
            body.Add(Expression.Label([exit]))
            Dim block = Expression.Block(variables, body)
            Return Expression.Lambda(CType(GetType(WriteOneDelegate(Of)).MakeGenericType(targetType), Type), CType(block, Expression), CType(pWriter, ParameterExpression), CType(pItem, ParameterExpression)).Compile()
        End Function

        Private Function BuildWriteBranch(key As Object, fieldDesc As FieldDesc, pWriter As Expression, pItem As Expression, [exit] As LabelTarget) As SwitchCase
            Dim body As List(Of Expression) = New List(Of Expression)()
            body.Add(SwitchField.BuildWriteOne(pWriter, key))
            If fieldDesc IsNot Nothing Then body.Add(fieldDesc.BuildWrite(pWriter, pItem))
            body.Add(Expression.Return([exit]))
            Return Expression.SwitchCase(Expression.Block(body), Expression.Constant(key))
        End Function

        Public Function BuildReader(targetType As Type) As [Delegate]
            Dim pReader = Expression.Parameter(GetType(Reader))
            Dim variables As List(Of ParameterExpression) = New List(Of ParameterExpression)()
            Dim body As List(Of Expression) = New List(Of Expression)()
            Dim resultVar = Expression.Variable(targetType, "result")
            variables.Add(resultVar)
            Dim assign = Expression.Assign(resultVar, Expression.[New](targetType))
            body.Add(assign)
            body.Add(SwitchField.BuildAssign(SwitchField.BuildReadOne(pReader), resultVar))
            Dim [exit] As LabelTarget = Expression.Label()
            Dim cases As List(Of SwitchCase) = New List(Of SwitchCase)()

            For Each branch In Branches
                cases.Add(BuildReadBranch(branch.Key, branch.Value, resultVar, pReader, [exit]))
            Next

            body.Add(Expression.Switch(Expression.PropertyOrField(resultVar, SwitchField.MInfo.Name), Expression.Block(ThrowUnexpectedValue(Expression.PropertyOrField(resultVar, SwitchField.MInfo.Name))), cases.ToArray()))
            body.Add(Expression.Label([exit]))
            body.Add(resultVar)
            Dim block = Expression.Block(variables, body)
            Return Expression.Lambda(CType(GetType(ReadOneDelegate(Of)).MakeGenericType(targetType), Type), CType(block, Expression), pReader).Compile()
        End Function

        Private Shared Function ThrowUnexpectedValue(value As MemberExpression) As Expression
            'throw new FormatException(string.Format("unexpected value: {0}", result.Type));

            Dim strExpr = Expression.Call(GetType(String).GetMethod("Format", New Type() {GetType(String), GetType(Object)}), Expression.Constant("unexpected value: {0}"), Expression.Call(value, GetType(Object).GetMethod("ToString")))
            Return Expression.Throw(Expression.[New](GetType(FormatException).GetConstructor(New Type() {GetType(String)}), strExpr))
        End Function

        Private Shared Function BuildReadBranch(key As Object, fieldDesc As FieldDesc, resultVar As ParameterExpression, pReader As Expression, [exit] As LabelTarget) As SwitchCase
            Dim body As List(Of Expression) = New List(Of Expression)()
            If fieldDesc IsNot Nothing Then body.Add(fieldDesc.BuildAssign(fieldDesc.BuildRead(pReader), resultVar))
            body.Add(Expression.Break([exit]))
            Return Expression.SwitchCase(Expression.Block(body), Expression.Constant(key))
        End Function

        Public Shared Function Create(t As Type) As SwitchModel
            Dim model As SwitchModel = New SwitchModel()
            model.Branches = New Dictionary(Of Object, FieldDesc)()

            For Each f In t.GetFields().Where(Function(fi) fi.IsPublic AndAlso Not fi.IsStatic)
                AppendField(model, f)
            Next

            For Each p In t.GetProperties().Where(Function(pi) pi.CanWrite AndAlso pi.CanRead)
                AppendField(model, p)
            Next

            If model.SwitchField Is Nothing AndAlso model.Branches.Count = 0 Then Return Nothing
            If model.SwitchField Is Nothing Then Throw New InvalidOperationException("switch attribute not found")
            If model.Branches.Count <= 1 Then Throw New InvalidOperationException("requires more than two case attributes")
            If model.Branches.Values.All(Function(f) f Is Nothing) Then Throw New InvalidOperationException("required no void case attribute")
            Return model
        End Function

        Private Shared Sub AppendField(model As SwitchModel, mi As MemberInfo)
            If mi.GetAttr(Of SwitchAttribute)() IsNot Nothing Then ' switch field
                If model.SwitchField IsNot Nothing Then Throw New InvalidOperationException("duplicate switch attribute")
                model.SwitchField = New FieldDesc(mi)

                For Each cAttr In mi.GetAttrs(Of CaseAttribute)()
                    If model.Branches.ContainsKey(cAttr.Value) Then Throw New InvalidOperationException("duplicate case value " & cAttr.Value.ToString())
                    model.Branches.Add(cAttr.Value, Nothing)
                Next ' case field
            Else

                For Each cAttr In mi.GetAttrs(Of CaseAttribute)()
                    If model.Branches.ContainsKey(cAttr.Value) Then Throw New InvalidOperationException("duplicate case value " & cAttr.Value.ToString())
                    model.Branches.Add(cAttr.Value, New FieldDesc(mi))
                Next
            End If
        End Sub
    End Class
End Namespace

