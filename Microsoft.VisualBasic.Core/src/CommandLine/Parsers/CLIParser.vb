#Region "Microsoft.VisualBasic::f73f89db3ab68ea37d96d5728325ecbe, Microsoft.VisualBasic.Core\src\CommandLine\Parsers\CLIParser.vb"

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

    '   Total Lines: 155
    '    Code Lines: 104 (67.10%)
    ' Comment Lines: 29 (18.71%)
    '    - Xml Docs: 93.10%
    ' 
    '   Blank Lines: 22 (14.19%)
    '     File Size: 6.25 KB


    '     Module CLIParser
    ' 
    '         Function: checkKeyDuplicated, extract, (+2 Overloads) TryParse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text.Parser
Imports StringList = System.Collections.Generic.IEnumerable(Of String)

Namespace CommandLine.Parsers

    ''' <summary>
    ''' 命令行单词解析器
    ''' </summary>
    Public Module CLIParser

        ''' <summary>
        ''' split the key=value tuple insdie the commandline argument token string
        ''' </summary>
        ''' <param name="tokens">
        ''' a set of the commandline argument token list
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Private Iterator Function extract(tokens As IEnumerable(Of String)) As IEnumerable(Of String)
            For Each token As String In tokens
                If token.StartsWith("-") OrElse token.StartsWith("/") Then
                    If token.IndexOf("="c) > -1 Then
                        With token.GetTagValue("=", trim:=True)
                            Yield .Name
                            Yield .Value
                        End With
                    Else
                        Yield token
                    End If
                Else
                    Yield token
                End If
            Next
        End Function

        ''' <summary>
        ''' Try parsing the cli command string from the string value.
        ''' </summary>
        ''' <param name="args">The commandline arguments which is user inputs from the terminal.</param>
        ''' <param name="duplicatedAllows">Allow the duplicated command parameter argument name in the input, 
        ''' default is not allowed the duplication.(是否允许有重复名称的参数名出现，默认是不允许的)</param>
        ''' <returns></returns>
        ''' <remarks>(尝试着从文本行之中解析出命令行参数信息)</remarks>
        <ExportAPI("TryParse")>
        <Extension>
        Public Function TryParse(args As StringList,
                                 Optional duplicatedAllows As Boolean = False,
                                 Optional rawInput$ = Nothing) As CommandLine

#If UNIX Then
            ' 20210606 这个主要是针对docker环境的命令行传递的问题
            Dim tokens$() = POSIX.JoinTokens(args.SafeQuery).ToArray
#Else
            Dim tokens$() = args.SafeQuery.ToArray
#End If
            Dim singleValue$ = ""

            If tokens.Length = 0 Then
                Return New CommandLine
            Else
                tokens = tokens _
                    .fixWindowsNetworkDirectory _
                    .extract _
                    .ToArray
            End If

            Dim bools$() = tokens _
                .Skip(1) _
                .GetLogicalFlags(singleValue)
            Dim cli As New CommandLine With {
                .Name = tokens(Scan0),
                .Tokens = tokens,
                .BoolFlags = bools,
                .cliCommandArgvs = Join(tokens)
            }

            cli.SingleValue = singleValue
            cli.cliCommandArgvs = rawInput

            If cli.Parameters.Length = 1 AndAlso
                String.IsNullOrEmpty(cli.SingleValue) Then

                cli.SingleValue = cli.Parameters(0)
            End If

            If tokens.Length > 1 Then
                cli.arguments = tokens.Skip(1).ToArray.CreateParameterValues(False)

                Dim Dk As String() = checkKeyDuplicated(cli.arguments)

                If Not duplicatedAllows AndAlso Not Dk.IsNullOrEmpty Then
                    Dim Key$ = String.Join(", ", Dk)
                    Dim msg$ = String.Format(KeyDuplicated, Key, String.Join(" ", tokens.Skip(1).ToArray))

                    Throw New Exception(msg)
                End If
            End If

            Return cli
        End Function

        Const KeyDuplicated As String = "The command line switch key ""{0}"" Is already been added! Here Is your input data:  CMD {1}."

        Private Function checkKeyDuplicated(source As IEnumerable(Of NamedValue(Of String))) As String()
            Dim LQuery = (From param As NamedValue(Of String)
                          In source
                          Select param.Name.ToLower
                          Group By ToLower Into Group).ToArray

            Return LinqAPI.Exec(Of String) _
 _
                () <= From group
                      In LQuery
                      Where group.Group.Count > 1
                      Select group.ToLower
        End Function

        ''' <summary>
        ''' Try parsing the cli command string from the string value.
        ''' (尝试着从文本行之中解析出命令行参数信息，假若value里面有空格，则必须要将value添加双引号)
        ''' </summary>
        ''' <param name="CLI">The commandline arguments which is user inputs from the terminal.</param>
        ''' <param name="duplicateAllowed">Allow the duplicated command parameter argument name in the input, 
        ''' default is not allowed the duplication.(是否允许有重复名称的参数名出现，默认是不允许的)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("TryParse")>
        Public Function TryParse(<Parameter("CLI", "The CLI arguments that inputs from the console by user.")> CLI$,
                                 <Parameter("Duplicates.Allowed")>
                                 Optional duplicateAllowed As Boolean = False) As CommandLine

            If String.IsNullOrEmpty(CLI) Then
                Return New CommandLine
            Else
#Const DEBUG = False
#If DEBUG Then
                Call CLI.debug
#End If
            End If

            Dim args As CommandLine = CLITools _
                .GetTokens(CLI) _
                .TryParse(duplicateAllowed, rawInput:=CLI)

            Return args
        End Function
    End Module
End Namespace
