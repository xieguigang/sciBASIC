#Region "Microsoft.VisualBasic::0d2af3ff00c1645f96925880206700d1, Data_science\Mathematica\Math\Math\Scripting\Helpers\Function.vb"

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

'     Class [Function]
' 
'         Properties: FunctionList
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: Evaluate, RND
' 
'         Sub: (+3 Overloads) Add
' 
' 
' /********************************************************************************/

#End Region

Imports stdNum = System.Math

Namespace Scripting.Helpers

    ''' <summary>
    ''' Mathematics function calculation engine
    ''' (数学函数计算引擎) 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class [Function] : Inherits MemoryCollection(Of Func(Of Double(), Double))

        ''' <summary>
        ''' The mathematics calculation delegates collection with its specific name.
        ''' (具有特定名称的数学计算委托方法的集合) 
        ''' </summary>
        ''' <remarks></remarks>
        Protected Shared ReadOnly SystemPrefixFunctions As New Dictionary(Of String, Func(Of Double(), Double)) From {
 _
                {"abs", Function(args) stdNum.Abs(args(Scan0))},
                {"acos", Function(args) stdNum.Acos(args(Scan0))},
                {"asin", Function(args) stdNum.Asin(args(Scan0))},
                {"atan", Function(args) stdNum.Atan(args(Scan0))},
                {"atan2", Function(args) stdNum.Atan2(args(Scan0), args(1))},
                {"bigmul", Function(args) stdNum.BigMul(args(Scan0), args(1))},
                {"ceiling", Function(args) stdNum.Ceiling(args(Scan0))},
                {"cos", Function(args) stdNum.Cos(args(Scan0))},
                {"cosh", Function(args) stdNum.Cosh(args(Scan0))},
                {"exp", Function(args) stdNum.Exp(args(Scan0))},
                {"floor", Function(args) stdNum.Floor(args(Scan0))},
                {"ieeeremainder", Function(args) stdNum.IEEERemainder(args(Scan0), args(1))},
                {"log", Function(args) stdNum.Log(args(Scan0), newBase:=args(1))},
                {"ln", Function(args) stdNum.Log(args(Scan0))},
                {"log10", Function(args) stdNum.Log10(args(Scan0))},
                {"max", Function(args) stdNum.Max(args(Scan0), args(1))},
                {"min", Function(args) stdNum.Min(args(Scan0), args(1))},
                {"pow", Function(args) stdNum.Pow(args(Scan0), args(1))},
                {"round", Function(args) stdNum.Round(args(Scan0))},
                {"sign", Function(args) stdNum.Sign(args(Scan0))},
                {"sin", Function(args) stdNum.Sin(args(Scan0))},
                {"sinh", Function(args) stdNum.Sinh(args(Scan0))},
                {"sqrt", Function(args) stdNum.Sqrt(args(Scan0))},
                {"tan", Function(args) stdNum.Tan(args(Scan0))},
                {"tanh", Function(args) stdNum.Tanh(args(Scan0))},
                {"truncate", Function(args) stdNum.Truncate(args(Scan0))},
                {"rnd", Function(args) RND(args(Scan0), args(1))},
                {"int", Function(args) CType(args(Scan0), Integer)},
                {String.Empty, Function(args) args(Scan0)} ' If no function name, then return the paramenter a directly. 
        }

        REM (经过以函数名长度降序排序的在VisualBasic中可用的函数名称的字符串集合) 
        Public ReadOnly Property FunctionList As String()
            Get
                Return MyBase.Objects
            End Get
        End Property

        Sub New(engine As Expression)
            Call MyBase.New(engine)

            For Each item As KeyValuePair(Of String, Func(Of Double(), Double)) In SystemPrefixFunctions
                Call MyBase.Add(item.Key, item.Value, False, False)
            Next

            ' A string list of available function name in visualbasic, 
            ' it was sort by the length of the each function name.
            Call __buildCache()
        End Sub

        ''' <summary>
        ''' 大小写不敏感
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Function Evaluate(name As String, args As Double()) As Double
            Return objTable(name.ToLower)(args)
        End Function

        Public Overloads Sub Add(name As String, handle As Func(Of Double(), Double))
            Call MyBase.Add(name, handle, True, False)
        End Sub

        ''' <summary>
        ''' Parsing the use function definition from the user input value on the console 
        ''' and then add it to the function dictionary.
        ''' (从终端上面输入的用户函数的申明语句中解析出表达式，然后将其加入到用户字典中)
        ''' </summary>
        ''' <param name="statement">[function name](args) expression</param>
        ''' <remarks>function [function name] expression</remarks>
        Public Overloads Sub Add(statement As String)
            Dim model As Func = FuncParser.TryParse(statement)
            Dim handle As Func(Of Double(), Double) = model.GetExpression(__engine)

            Call Add(model.Name.ToLower, handle)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name">函数名</param>
        ''' <param name="expr">函数申明</param>
        Public Overloads Sub Add(name As String, expr As String)

        End Sub

        ''' <summary>
        ''' This function return a random number, you can specific the boundary of the random number in the parameters. 
        ''' </summary>
        ''' <param name="UpBound">
        ''' If this parameter is empty or value is zero, then return the randome number between 0 and 1.
        ''' (如果这个参数为空或者其值为0，那么函数就会返回0和1之间的随机数)
        ''' </param>
        ''' <param name="LowBound"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function RND(LowBound As Double, UpBound As Double) As Double
            Dim rand As New Random(1000)
            If UpBound = 0R OrElse UpBound < LowBound Then
                Return rand.NextDouble
            Else
                Return LowBound + rand.NextDouble * (UpBound - LowBound)
            End If
        End Function
    End Class
End Namespace
