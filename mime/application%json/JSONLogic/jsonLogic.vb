#Region "Microsoft.VisualBasic::3db4a44b98a315280f653f594cf43011, mime\application%json\JSONLogic\jsonLogic.vb"

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

    '   Total Lines: 118
    '    Code Lines: 82 (69.49%)
    ' Comment Lines: 17 (14.41%)
    '    - Xml Docs: 82.35%
    ' 
    '   Blank Lines: 19 (16.10%)
    '     File Size: 4.76 KB


    '     Module jsonLogic
    ' 
    '         Function: (+2 Overloads) apply, GetValueType, MeasureType
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq.Expressions
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Namespace JSONLogic

    ''' <summary>
    ''' Build complex rules, serialize them as JSON, share them between front-end and back-end
    ''' </summary>
    ''' <remarks>
    ''' If you’re looking for a way to share logic between front-end and back-end code, and even
    ''' store it in a database, JsonLogic might be a fit for you.
    '''
    ''' JsonLogic isn't a full programming language. It’s a small, safe way to delegate one decision. 
    ''' You could store a rule in a database to decide later. You could send that rule from back-end
    ''' to front-end so the decision is made immediately from user input. Because the rule is data,
    ''' you can even build it dynamically from user actions or GUI input.
    '''
    ''' JsonLogic has no setters, no loops, no functions Or gotos. One rule leads To one decision, 
    ''' With no side effects And deterministic computation time.
    ''' 
    ''' > https://jsonlogic.com/
    ''' </remarks>
    Public Module jsonLogic

        Private Function GetValueType(val As JsonElement, ByRef input As Object) As Type
            If val Is Nothing Then
                input = Nothing
                Return GetType(Object)
            ElseIf TypeOf val Is JsonValue Then
                input = DirectCast(val, JsonValue).value
                Return DirectCast(val, JsonValue).UnderlyingType
            ElseIf TypeOf val Is JsonArray Then
                Dim types As New List(Of Type)
                Dim vals As New List(Of Object)
                Dim list As JsonArray = val

                For Each item As JsonElement In list
                    types.Add(GetValueType(item, input))
                    vals.Add(input)
                Next

                Dim type As Type = MeasureType(types)
                Dim array As Array = Array.CreateInstance(type, vals.Count)

                Array.ConstrainedCopy(vals.ToArray, Scan0, array, Scan0, array.Length)
                input = array

                Return type
            Else
                Throw New NotImplementedException
            End If
        End Function

        Private Function MeasureType(elements As IEnumerable(Of Type)) As Type
            Dim pullAll = elements.ToArray
            Dim first As Type = pullAll.First

            If pullAll.All(Function(t) t Is first) Then
                Return first
            Else
                Throw New NotImplementedException
            End If
        End Function

        Public Function apply(logic As JsonElement, Optional data As JsonElement = Nothing) As JsonElement
            Dim pars As New List(Of (String, Type))
            Dim input As New List(Of Object)

            If Not data Is Nothing Then
                If TypeOf data Is JsonObject Then
                    Dim list As JsonObject = data
                    Dim ref As Object = Nothing

                    For Each key As String In list.ObjectKeys
                        Call pars.Add((key, GetValueType(list(key), ref)))
                        Call input.Add(ref)
                    Next
                End If
            End If

            Dim env As New TreeBuilder(pars)
            Dim code As Expression = env.Parse(logic)
            Dim lambda As LambdaExpression = Expression.Lambda(code, env.Parameters.ToArray)
            Dim func_del = lambda.Compile
            Dim args As Object() = input.ToArray
            Dim output As Object = func_del.DynamicInvoke(args)

            If output Is Nothing Then
                Return Nothing
            ElseIf output.GetType.IsArray Then
                Dim array As New JsonArray

                For Each item As Object In DirectCast(output, Array)
                    array.Add(New JsonValue(item))
                Next

                Return array
            Else
                Return New JsonValue(output)
            End If
        End Function

        Public Function apply(logic As String, Optional data As String = Nothing) As Object
            Dim logic_json As JsonElement = JsonParser.Parse(logic)
            Dim data_json As JsonElement = JsonParser.Parse(data)
            Dim result_json As JsonElement = jsonLogic.apply(logic_json, data_json)

            If result_json Is Nothing Then
                Return Nothing
            ElseIf TypeOf result_json Is JsonValue Then
                Return DirectCast(result_json, JsonValue).value
            Else
                Throw New NotImplementedException(result_json.GetType.FullName)
            End If
        End Function

    End Module
End Namespace
