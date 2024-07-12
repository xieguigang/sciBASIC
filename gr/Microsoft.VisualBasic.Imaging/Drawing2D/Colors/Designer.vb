#Region "Microsoft.VisualBasic::1d5d95845dc54d7f83e5d76416d2383a, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Designer.vb"

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

    '   Total Lines: 598
    '    Code Lines: 303 (50.67%)
    ' Comment Lines: 233 (38.96%)
    '    - Xml Docs: 87.55%
    ' 
    '   Blank Lines: 62 (10.37%)
    '     File Size: 33.01 KB


    '     Module Designer
    ' 
    '         Properties: AvailableInterpolates, Category31, ColorBrewer, ConsoleColors, MaterialPalette
    '                     Typhoon
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Colors, ConsoleColor, CubicSpline, FromConsoleColor, FromNames
    '                   FromSchema, GetBrushes, (+2 Overloads) GetColors, getColorsInternal, internalFills
    '                   IsColorNameList, ParseAvailableInterpolates, ParseColorBrewer, rangeConstraint, SplitColorList
    ' 
    '         Sub: Register
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors.OfficeAccent
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Interpolation
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports r = System.Text.RegularExpressions.Regex

Namespace Drawing2D.Colors

    ''' <summary>
    ''' Generate color sequence
    ''' </summary>
    Public Module Designer

        '''<summary>
        ''' {  
        '''  &quot;Color [PapayaWhip]&quot;: [
        '''    {
        '''      &quot;knownColor&quot;: 93,
        '''      &quot;name&quot;: null,
        '''      &quot;state&quot;: 1,
        '''      &quot;value&quot;: 0
        '''    },
        '''    {
        '''      &quot;knownColor&quot;: 119,
        '''      &quot;name&quot;: null,
        '''      &quot;state&quot;: 1,
        '''      &quot;value&quot;: 0
        '''    },
        '''    {
        '''      &quot;knownColor&quot;: 30,
        '''      &quot;name&quot;: null,
        '''      &quot;state&quot;: 1,
        '''      &quot;value&quot;: 0
        '''    },
        '''    {
        '''      &quot;knownColor&quot;: 165,
        '''      &quot;name&quot;: null,
        '''      &quot;state&quot;: 1,
        '''      &quot;value&quot;: 0
        '''    },
        '''    {
        '''      &quot;knownColor&quot;: 81,
        '''      &quot;name&quot;: null,
        '''      &quot;state&quot;: 1,
        '''      &quot;value&quot;: [rest of string was truncated]&quot;;.
        '''</summary>
        Public ReadOnly Property AvailableInterpolates As IReadOnlyDictionary(Of Color, Color())
        Public ReadOnly Property ColorBrewer As Dictionary(Of String, ColorBrewer)

        ''' <summary>
        ''' <see cref="Designer.GetColors(String)"/> schema name for color profile: <see cref="CustomDesigns.ClusterColour"/>.
        ''' </summary>
        Friend Const Clusters$ = NameOf(Clusters)

        ReadOnly BlackGreenRed As Color() = {Color.Black, Color.Green, Color.Red}

        ''' <summary>
        ''' 16 console colors
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ConsoleColors As Color() = Enums(Of ConsoleColor) _
            .Select(Function(c) c.ToString) _
            .Select(Function(exp As String)
                        Return exp.FromConsoleColor
                    End Function) _
            .ToArray

        <Extension>
        Private Function FromConsoleColor(exp As String) As Color
            ' 2019-03-14 有些console的颜色是不存在的,所以解析会得到黑色
            Dim color As Color = exp.TranslateColor(False)

            If Not color.IsEmpty Then
                Return color
            Else
                ' 使用相近的颜色进行替代
                If InStr(exp, "Dark") > 0 Then
                    exp = exp.Replace("Dark", "")
                    color = exp.TranslateColor.Darken
                ElseIf InStr(exp, "Light") > 0 Then
                    exp = exp.Replace("Light", "")
                    color = exp.TranslateColor.Lighten
                Else
                    Throw New NotImplementedException(exp)
                End If

                Return color
            End If
        End Function

        ''' <summary>
        ''' 将<see cref="System.ConsoleColor"/>枚举值转换为gdi+颜色对象
        ''' </summary>
        ''' <param name="color"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ConsoleColor(color As ConsoleColor) As Color
            Return color.ToString.FromConsoleColor
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks>
        ''' ###### 2017-9-7
        ''' 
        ''' mono的Json反序列化任然存在问题
        ''' 
        ''' [ERROR 2017/9/7 10:03:20] &lt;Print>::System.Exception: Print ---> System.Reflection.TargetInvocationException: Exception has been thrown by the target of an invocation. ---> System.TypeInitializationException: The type initializer for 'Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Designer' threw an exception. ---> System.Reflection.TargetInvocationException: Exception has been thrown by the target of an invocation. ---> System.ArgumentNullException: Value cannot be null.
        ''' Parameter name :  key
        '''   at System.Collections.Generic.Dictionary`2[TKey,TValue].TryInsert (TKey key, TValue value, System.Collections.Generic.InsertionBehavior behavior) [0x00008] In &lt;902ab9e386384bec9c07fa19aa938869>:0
        '''   at System.Collections.Generic.Dictionary`2[TKey,TValue].Add (TKey key, TValue value) [0x00000] In &lt;902ab9e386384bec9c07fa19aa938869>:0
        '''   at (wrapper managed-to-native) System.Reflection.MonoMethod:InternalInvoke (System.Reflection.MonoMethod,object,object[],System.Exception&amp;)
        '''   at System.Reflection.MonoMethod.Invoke (System.Object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) [0x00032] In &lt;902ab9e386384bec9c07fa19aa938869>:0
        '''    --- End of inner exception stack trace ---
        '''   at System.Reflection.MonoMethod.Invoke (System.Object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) [0x00048] In &lt;902ab9e386384bec9c07fa19aa938869>:0
        '''   at System.Reflection.MethodBase.Invoke (System.Object obj, System.Object[] parameters) [0x00000] In &lt;902ab9e386384bec9c07fa19aa938869>:0
        '''   at System.Runtime.Serialization.Json.JsonFormatReaderInterpreter.ReadSimpleDictionary (System.Runtime.Serialization.CollectionDataContract collectionContract, System.Type keyValueType) [0x0014a] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.Json.JsonFormatReaderInterpreter.ReadCollection (System.Runtime.Serialization.CollectionDataContract collectionContract) [0x000fe] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.Json.JsonFormatReaderInterpreter.ReadCollectionFromJson (System.Runtime.Serialization.XmlReaderDelegator xmlReader, System.Runtime.Serialization.Json.XmlObjectSerializerReadContextComplexJson context, System.Xml.XmlDictionaryString emptyDictionaryString, System.Xml.XmlDictionaryString itemName, System.Runtime.Serialization.CollectionDataContract collectionContract) [0x00025] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.Json.JsonFormatReaderGenerator+CriticalHelper+&lt;>c__DisplayClass1_0.&lt;GenerateCollectionReader>b__0 (System.Runtime.Serialization.XmlReaderDelegator xr, System.Runtime.Serialization.Json.XmlObjectSerializerReadContextComplexJson ctx, System.Xml.XmlDictionaryString emptyDS, System.Xml.XmlDictionaryString inm, System.Runtime.Serialization.CollectionDataContract cc) [0x0000c] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.Json.JsonCollectionDataContract.ReadJsonValueCore (System.Runtime.Serialization.XmlReaderDelegator jsonReader, System.Runtime.Serialization.Json.XmlObjectSerializerReadContextComplexJson context) [0x0004f] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.Json.JsonDataContract.ReadJsonValue (System.Runtime.Serialization.XmlReaderDelegator jsonReader, System.Runtime.Serialization.Json.XmlObjectSerializerReadContextComplexJson context) [0x00007] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.Json.DataContractJsonSerializer.ReadJsonValue (System.Runtime.Serialization.DataContract contract, System.Runtime.Serialization.XmlReaderDelegator reader, System.Runtime.Serialization.Json.XmlObjectSerializerReadContextComplexJson context) [0x00006] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.Json.XmlObjectSerializerReadContextComplexJson.ReadDataContractValue (System.Runtime.Serialization.DataContract dataContract, System.Runtime.Serialization.XmlReaderDelegator reader) [0x00000] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.XmlObjectSerializerReadContext.InternalDeserialize (System.Runtime.Serialization.XmlReaderDelegator reader, System.String name, System.String ns, System.Type declaredType, System.Runtime.Serialization.DataContract&amp; dataContract) [0x00264] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.XmlObjectSerializerReadContext.InternalDeserialize (System.Runtime.Serialization.XmlReaderDelegator xmlReader, System.Type declaredType, System.Runtime.Serialization.DataContract dataContract, System.String name, System.String ns) [0x0000b] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.XmlObjectSerializerReadContextComplex.InternalDeserialize (System.Runtime.Serialization.XmlReaderDelegator xmlReader, System.Type declaredType, System.Runtime.Serialization.DataContract dataContract, System.String name, System.String ns) [0x00010] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.Json.DataContractJsonSerializer.InternalReadObject (System.Runtime.Serialization.XmlReaderDelegator xmlReader, System.Boolean verifyObjectName) [0x000c5] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.XmlObjectSerializer.InternalReadObject (System.Runtime.Serialization.XmlReaderDelegator reader, System.Boolean verifyObjectName, System.Runtime.Serialization.DataContractResolver dataContractResolver) [0x00000] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.XmlObjectSerializer.ReadObjectHandleExceptions (System.Runtime.Serialization.XmlReaderDelegator reader, System.Boolean verifyObjectName, System.Runtime.Serialization.DataContractResolver dataContractResolver) [0x00072] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.XmlObjectSerializer.ReadObjectHandleExceptions (System.Runtime.Serialization.XmlReaderDelegator reader, System.Boolean verifyObjectName) [0x00000] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.Json.DataContractJsonSerializer.ReadObject (System.Xml.XmlDictionaryReader reader) [0x0000d] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at System.Runtime.Serialization.Json.DataContractJsonSerializer.ReadObject (System.IO.Stream stream) [0x00017] In &lt;8a29d8bc61874dfaa700acd3dd58b577>:0
        '''   at Microsoft.VisualBasic.Serialization.JSON.JsonContract.LoadObject (System.String json, System.Type type, System.Boolean simpleDict) [0x0003e] In &lt;fb45d3e2e23e49ecae88d0914baf5cb7>:0
        '''   at Microsoft.VisualBasic.Serialization.JSON.JsonContract.LoadObject[T] (System.String json, System.Boolean simpleDict) [0x00000] In &lt;fb45d3e2e23e49ecae88d0914baf5cb7>:0
        '''   at Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Designer..cctor () [0x0040f] In &lt;fe90788c758b4b75b8659df0623dea4b>:0
        '''    --- End of inner exception stack trace ---
        '''   at json.Program.Convert (System.String In, System.String nodesTable, System.String kegKCF, System.Boolean degreeSize, System.Boolean compress, System.String style, System.Boolean nodeID, System.String[] maps) [0x00058] In &lt;f0aa82bd17214027b520f8953095c372>:0
        '''   at Biodeep.KEGG.Network.Common.KEGG.BuildNetworkJSON (System.Collections.Generic.IEnumerable`1[T] compounds, System.String repo, System.String out, System.String[] mapIDs) [0x00145] In &lt;598a64b776e440d8b3f5e763e2b2cdd0>:0
        '''   at cytonetwork.Program.BiodeepKEGGNetwork (Microsoft.VisualBasic.CommandLine.CommandLine args) [0x000c1] In &lt;2ebe3e57fb4d4cafb0d4cd22a0a764c2>:0
        '''   at (wrapper managed-to-native) System.Reflection.MonoMethod:InternalInvoke (System.Reflection.MonoMethod,object,object[],System.Exception&amp;)
        '''   at System.Reflection.MonoMethod.Invoke (System.Object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) [0x00032] In &lt;902ab9e386384bec9c07fa19aa938869>:0
        '''    --- End of inner exception stack trace ---
        '''   at System.Reflection.MonoMethod.Invoke (System.Object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) [0x00048] In &lt;902ab9e386384bec9c07fa19aa938869>:0
        '''   at System.Reflection.MethodBase.Invoke (System.Object obj, System.Object[] parameters) [0x00000] In &lt;902ab9e386384bec9c07fa19aa938869>:0
        '''   at Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints.APIEntryPoint.__directInvoke (System.Object[] callParameters, System.Object target, System.Boolean Throw) [0x0000c] In &lt;fb45d3e2e23e49ecae88d0914baf5cb7>:0
        '''    --- End of inner exception stack trace ---
        ''' </remarks>
        Sub New()
            Try
                AvailableInterpolates = ParseAvailableInterpolates()
            Catch ex As Exception
                AvailableInterpolates = New Dictionary(Of Color, Color())

                Call App.LogException(ex)
            End Try

            Try
                ColorBrewer = ParseColorBrewer()
            Catch ex As Exception
                ColorBrewer = New Dictionary(Of String, ColorBrewer)

                Call App.LogException(ex)
                Call "ColorBrewer module is not available on Linux server".Warning
            End Try
        End Sub

        Private Function ParseColorBrewer() As Dictionary(Of String, ColorBrewer)
            Dim colorBrewerJSON$ = My.Resources.colorbrewer.GetString(Encodings.UTF8)
            Dim ns As String() = r _
                .Matches(colorBrewerJSON, """\d+""", RegexOptions.Singleline) _
                .ToArray(Function(m)
                             Return m.Trim(""""c)
                         End Function)
            Dim sb As New StringBuilder(colorBrewerJSON)

            For Each n As String In ns.Distinct
                Call sb.Replace($"""{n}""", $"""c{n}""")
            Next

            Return sb.ToString.LoadJSON(Of Dictionary(Of String, ColorBrewer))
        End Function

        Private Function ParseAvailableInterpolates() As Dictionary(Of Color, Color())
            Dim colors As Dictionary(Of String, String()) = My.Resources _
                    .designer_colors _
                    .GetString(Encodings.UTF8) _
                    .LoadJSON(Of Dictionary(Of String, String()))
            Dim valids As New Dictionary(Of Color, Color())

            For Each x As KeyValuePair(Of String, String()) In colors
                valids(ColorTranslator.FromHtml(x.Key)) = x.Value _
                    .Select(AddressOf ColorTranslator.FromHtml) _
                    .ToArray
            Next

            Return valids
        End Function

        ''' <summary>
        ''' <see cref="ColorMap"/> pattern names
        ''' </summary>
        ReadOnly allColorMapNames$() = {
            ColorMap.PatternAutumn,
            ColorMap.PatternCool,
            ColorMap.PatternGray,
            ColorMap.PatternHot,
            ColorMap.PatternJet,
            ColorMap.PatternSpring,
            ColorMap.PatternSummer,
            ColorMap.PatternWinter
        }.Select(AddressOf LCase) _
         .ToArray

        ''' <summary>
        ''' 20 Google material design colors
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property MaterialPalette As Color() = {
            Color.Red, Color.Green, Color.CadetBlue, Color.Pink, Color.Black, Color.Purple, Color.DarkViolet,
            Color.Indigo, Color.Blue, Color.LightBlue, Color.Cyan, Color.Teal,
            Color.LightGreen, Color.Lime, Color.Yellow, Color.Orchid,
            Color.Orange, Color.DarkOrange, Color.Brown, Color.Gray
        }

        Public ReadOnly Property Category31 As Color() = {
            "#BCBD22", "#4AD693", "#1F77B4", "#CDE64C", "#9AC901", "#56028E", "#A2D1CF", "#C8B631",
            "#6DBCEC", "#C24642", "#5F71BC", "#798DCD", "#292E76", "#FF7256", "#91CF4F", "#FE5050",
            "#FEBF00", "#FD7E00", "#07A7E4", "#51AC81", "#FE00FF", "#FDE688", "#7E00FD", "#CD00FF",
            "#988ED5", "#027093", "#73945A", "#8C564B", "#9467BD", "#D62829", "#2CA02C"
        }.AsColor()

        Public ReadOnly Property Typhoon As Color() = {
            "#FFFFFF", "#AAAAD4", "#5F59A0", "#3B277F", "#31277F",
            "#355C83", "#539144", "#72AC3E", "#8CB73A", "#BACB2D",
            "#FAEB3A", "#E4A726", "#CE5C18", "#C42917"
        }.AsColor

        Const rgbPattern$ = "rgb\(\d+\s*(,\s*\d+\s*)+\)"
        Const rgbListPattern$ = rgbPattern & "(\s*,\s*" & rgbPattern & ")+"

        <Extension>
        Private Function IsColorNameList(exp$) As Boolean
            ' 因为function和rgb表达式都存在括号
            ' 所以在这里需要先判断是否为颜色表达式的列表
            If exp.IsPattern(rgbListPattern, RegexICSng) Then
                ' 颜色列表
                Return True
            End If

            Static dotnetColorNames As Index(Of String) = GDIColors.AllDotNetColorNames _
                .Select(AddressOf Strings.LCase) _
                .Indexing

            If Not exp.IsPattern(DesignerExpression.FunctionPattern) AndAlso InStr(exp, ",") > 0 Then
                If exp.IsPattern(rgbPattern) Then
                    ' 单个rgb表达式的情况，肯定不是颜色列表
                    Return False
                Else
                    Return True
                End If
            ElseIf Strings.LCase(exp) Like dotnetColorNames Then
                ' is a single color name
                ' gray may be is not a single color while this
                ' function is apply for test color name list
                If exp.TextEquals(ScalerPalette.Gray.Description) Then
                    Return False
                Else
                    Return True
                End If
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' 在这个函数里，需要保证颜色的顺序和表达式之中所输入的顺序一致
        ''' </summary>
        ''' <param name="expr"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 这个函数不支持rgb表达式与颜色名称，html颜色表达式混合
        ''' </remarks>
        <Extension>
        Private Function SplitColorList(expr As String) As String()
            If expr.IsPattern(rgbListPattern, RegexICSng) Then
                ' 因为不支持混合，所以只能够出现rgb表达式列表
                ' 在这里如果符合字符串模式的话，就直接使用正则
                ' 进行列表元素的匹配操作了
                Return expr _
                    .Matches(rgbPattern, RegexICSng) _
                    .ToArray
            Else
                ' 颜色名称和html颜色代码之间可以相互混合
                ' 但是不允许出现rgb表达式
                Return expr.StringSplit(",\s*")
            End If
        End Function

        ''' <summary>
        ''' a unify method for get color maps.
        ''' 
        ''' (对于无效的键名称，默认是返回<see cref="Office2016"/>，请注意，
        ''' 如果是所有的.net的颜色的话，这里面还会包含有白色，所以还需要手工
        ''' 去除掉白色)
        ''' </summary>
        ''' <param name="exp$">
        ''' <see cref="DesignerExpression"/>.
        ''' (假若这里所输入的是一组颜色值，则必须是htmlcolor或者颜色名称，RGB表达式将不会被允许)
        ''' </param>
        ''' <returns></returns>
        Public Function GetColors(exp As String) As Color()
            If exp.StringEmpty Then
                Return {}
            ElseIf exp.IsColorNameList Then
                Return Designer _
                    .SplitColorList(exp) _
                    .Select(AddressOf TranslateColor) _
                    .ToArray
            Else
                With New DesignerExpression(exp)
                    Return .Modify(Designer.getColorsInternal(.Term))
                End With
            End If
        End Function

        ReadOnly colorRegistry As New Dictionary(Of String, Color())

        ''' <summary>
        ''' register a custom color palette
        ''' </summary>
        ''' <param name="colorName"></param>
        ''' <param name="colors"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Register(colorName As String, ParamArray colors As Color())
            colorRegistry(colorName) = colors
        End Sub

        ''' <summary>
        ''' a unify method for get color maps
        ''' </summary>
        ''' <param name="term$"></param>
        ''' <returns></returns>
        Private Function getColorsInternal(term$) As Color()
            If Array.IndexOf(allColorMapNames, term.ToLower) > -1 Then
                Return New ColorMap(20, 255).ColorSequence(term)
            End If

            Dim key As NamedValue(Of String) = Drawing2D.Colors.ColorBrewer.ParseName(term)

            If ColorBrewer.ContainsKey(key.Name) Then
                Return ColorBrewer(key.Name).GetColors(key.Value)
            ElseIf key.Name = "" AndAlso ColorBrewer.ContainsKey(key.Value) Then
                Return ColorBrewer(key.Value).GetColors(Nothing)
            End If

            Select Case Strings.LCase(term).Trim
                Case "material" : Return MaterialPalette
                Case "typhoon" : Return Typhoon
                Case "console.colors", "console" : Return ConsoleColors
                Case "tsf" : Return CustomDesigns.TSF
                Case "halloween" : Return CustomDesigns.Halloween
                Case "unicorn" : Return CustomDesigns.Unicorn
                Case "vibrant" : Return CustomDesigns.Vibrant
                Case "rainbow" : Return CustomDesigns.Rainbow
                Case "fleximaging" : Return CustomDesigns.FlexImaging
                Case "paper" : Return CustomDesigns.Paper
                Case "dotnet.colors" : Return AllDotNetPrefixColors
                Case "scibasic.chart()" : Return ChartColors
                Case "scibasic.category31()" : Return Category31
                Case "clusters" : Return CustomDesigns.ClusterColour
                Case "blackgreenred" : Return BlackGreenRed

                Case "red_channel" : Return {Color.FromArgb(0, 0, 0), Color.FromArgb(128, 0, 0), Color.FromArgb(255, 0, 0)}
                Case "green_channel" : Return {Color.FromArgb(0, 0, 0), Color.FromArgb(0, 128, 0), Color.FromArgb(0, 255, 0)}
                Case "blue_channel" : Return {Color.FromArgb(0, 0, 0), Color.FromArgb(0, 0, 128), Color.FromArgb(0, 0, 255)}

                    ' d3.js colors
                Case "d3.scale.category10()" : Return d3js.category10
                Case "d3.scale.category20()" : Return d3js.category20
                Case "d3.scale.category20b()" : Return d3js.category20b
                Case "d3.scale.category20c()" : Return d3js.category20c

                    ' viridis
                Case "viridis" : Return Viridis.viridis.ToArray
                Case "viridis:magma", "magma" : Return Viridis.magma.ToArray
                Case "viridis:inferno", "inferno" : Return Viridis.inferno.ToArray
                Case "viridis:plasma", "plasma" : Return Viridis.plasma.ToArray
                Case "viridis:cividis", "cividis" : Return Viridis.cividis.ToArray
                Case "viridis:mako", "mako" : Return Viridis.mako.ToArray
                Case "viridis:rocket", "rocket" : Return Viridis.rocket.ToArray
                Case "viridis:turbo", "turbo" : Return Viridis.turbo.ToArray

                Case Else

                    If OfficeColorThemes.Themes.ContainsKey(term) Then
                        Return OfficeColorThemes.GetAccentColors(term)
                    ElseIf colorRegistry.ContainsKey(term) Then
                        Return colorRegistry(term)
                    Else
                        Call $"unknown color set name: '{term}', returns the paper schema by default.".Warning

                        ' returns the default color set
                        Return CustomDesigns.Paper
                    End If
            End Select
        End Function

        ''' <summary>
        ''' 这个函数是获取得到一个连续的颜色谱
        ''' </summary>
        ''' <param name="term$"></param>
        ''' <param name="n">negative or zero value means no interoplation, 
        ''' just returns the raw color list which is mapping by the 
        ''' <paramref name="term"/>
        ''' </param>
        ''' <param name="alpha%"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetColors(term$, Optional n% = 256, Optional alpha% = 255) As Color()
            Return CubicSpline(GetColors(term), n, alpha)
        End Function

        ''' <summary>
        ''' ``New <see cref="SolidBrush"/>(<see cref="GetColors(String, Integer, Integer)"/>)``
        ''' </summary>
        ''' <param name="term$"></param>
        ''' <param name="n%"></param>
        ''' <param name="alpha%"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetBrushes(term$, Optional n% = 256, Optional alpha% = 255) As SolidBrush()
            Return GetColors(term, n, alpha).Select(Function(c) New SolidBrush(c)).ToArray
        End Function

        ''' <summary>
        ''' 相对于<see cref="GetColors"/>函数而言，这个函数是返回非连续的颜色谱，假若数量不足，会重新使用开头的起始颜色连续填充
        ''' </summary>
        ''' <param name="colors$"></param>
        ''' <param name="n%"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function FromNames(colors$(), n%) As Color()
            Return colors.Select(AddressOf ToColor).internalFills(n)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function internalFills(colors As IEnumerable(Of Color), n%) As Color()
            Return New LoopArray(Of Color)(colors).Take(n).ToArray
        End Function

        ''' <summary>
        ''' <see cref="FromSchema"/>和<see cref="FromNames"/>适用于函数绘图之类需要区分数据系列的颜色谱的生成
        ''' </summary>
        ''' <param name="term$"></param>
        ''' <param name="n%"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FromSchema(term$, n%) As Color()
            Return GetColors(term).internalFills(n)
        End Function

        ''' <summary>
        ''' **<see cref="ColorCube.GetColorSequence"/>**
        ''' 
        ''' Some useful color tables for images and tools to handle them.
        ''' Several color scales useful for image plots: a pleasing rainbow 
        ''' style color table patterned after that used in Matlab by Tim 
        ''' Hoar and also some simple color interpolation schemes between 
        ''' two or more colors. There is also a function that converts 
        ''' between colors and a real valued vector.
        ''' </summary>
        ''' <param name="col">
        ''' A list of colors (names or hex values) to interpolate.
        ''' </param>
        ''' <param name="n">
        ''' Number of color levels. The setting n=64 is the 
        ''' orignal definition.
        ''' </param>
        ''' <param name="alpha">
        ''' The transparency of the color – 255 is opaque and 0 is transparent. 
        ''' This is useful for overlays of color and still being able to view 
        ''' the graphics that is covered.
        ''' </param>
        ''' <returns>
        ''' A vector giving the colors in a hexadecimal format, two extra 
        ''' hex digits are added for the alpha channel.
        ''' </returns>
        Public Function Colors(col As Color(), Optional n% = 256, Optional alpha% = 255) As Color()
            Dim out As New List(Of Color)
            Dim steps! = n / col.Length
            Dim previous As Value(Of Color) = col.First

            For Each c As Color In col.Skip(1)
                out += ColorCube.GetColorSequence(
                    source:=previous,
                    target:=previous = c,
                    increment:=steps!,
                    alpha:=alpha%
                )
            Next

            Return out
        End Function

        ''' <summary>
        ''' 这个函数并不会计算alpha的值
        ''' </summary>
        ''' <param name="colors"></param>
        ''' <param name="n">所期望的颜色的数量</param>
        ''' <param name="interpolate">
        ''' set the interpolate parameter to value TRUE if apply the function 
        ''' for the scalar palette, otherwise keeps the default value FALSE
        ''' for deal with the category color palette.
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' if the <paramref name="n"/> value less than the 
        ''' collection size of the input <paramref name="colors"/>, 
        ''' then top n colors will be takes from the input 
        ''' color.
        ''' </remarks>
        <Extension>
        Public Function CubicSpline(colors As IEnumerable(Of Color),
                                    Optional n% = 256,
                                    Optional alpha% = 255,
                                    Optional interpolate As Boolean = False) As Color()

            Dim source As Color() = colors.SafeQuery.ToArray

            If source.Length = 1 Then
                Call $"multiple color value is required, but you just provides one color, color seqeucne will just contains one single color: {source(Scan0).ToString}".Warning

                Return source(Scan0) _
                    .Alpha(alpha) _
                    .Replicate(n) _
                    .ToArray
            ElseIf n <= 0 OrElse source.IsNullOrEmpty Then
                ' return raw color list if n is negative or zero
                Return source
            ElseIf n <= source.Length AndAlso Not interpolate Then
                Return source.Take(n).ToArray
            End If

            Dim x As New CubicSplineVector(source.Select(Function(c) CSng(c.R)))
            Dim y As New CubicSplineVector(source.Select(Function(c) CSng(c.G)))
            Dim z As New CubicSplineVector(source.Select(Function(c) CSng(c.B)))

            Call x.CalcSpline()
            Call y.CalcSpline()
            Call z.CalcSpline()

            Dim delta! = 1 / n
            Dim out As New List(Of Color)

            For f! = 0 To 1.0! Step delta!
                Dim r% = rangeConstraint(x.GetPoint(f))
                Dim g% = rangeConstraint(y.GetPoint(f))
                Dim b% = rangeConstraint(z.GetPoint(f))

                out += Color.FromArgb(alpha, r, g, b)
            Next

            Return out
        End Function

        ''' <summary>
        ''' Limit <see cref="CubicSpline"/> result in range [0, 255]
        ''' </summary>
        ''' <param name="x!"></param>
        ''' <returns></returns>
        Private Function rangeConstraint(x!) As Integer
            If x < 0! Then
                x = 0!
            ElseIf x > 255.0! Then
                x = 255.0!
            End If

            Return x
        End Function
    End Module
End Namespace
