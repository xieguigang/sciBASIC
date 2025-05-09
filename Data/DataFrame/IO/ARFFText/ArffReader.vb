Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Data.Framework.IO.CSVFile
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Values
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace IO.ArffFile

    ''' <summary>
    ''' The **ARFF (Attribute-Relation File Format)** is a text-based file format designed for storing 
    ''' structured datasets, primarily used in machine learning and data mining tools like Weka. 
    ''' Here's a detailed breakdown of its structure, features, and applications:
    '''
    ''' ---
    ''' 
    ''' ### 1. **ARFF File Structure**
    ''' 
    ''' ARFF files consist of two main sections: the **header** (metadata) and the **data** (instances).
    ''' 
    ''' #### **Header Section**
    ''' 
    ''' - **`@relation`**: Declares the dataset name. Example:  
    ''' 
    '''   ```arff
    '''   @relation weather
    '''   ```
    '''   
    ''' - **`@attribute`**: Defines each attribute (column) with its name and data type. Supported types include:
    ''' 
    '''   - **Numeric**: `@attribute temperature numeric`  
    '''   - **Nominal/Categorical**: `@attribute outlook {sunny, overcast, rainy}`  
    '''   - **String**: `@attribute description string`  
    '''   - **Date**: `@attribute timestamp date "yyyy-MM-dd"`  
    '''   
    ''' - **Missing values** are represented by `?`.
    ''' 
    ''' #### **Data Section**
    ''' 
    ''' - Begins with `@data`, followed by rows of comma-separated values:  
    ''' 
    '''   ```arff
    '''   @data
    '''   sunny,85,85,FALSE,no
    '''   ?,78,90,?,yes  // Missing values
    '''   ```
    ''' 
    ''' ---
    ''' 
    ''' ### 2. **Key Features**
    ''' 
    ''' - **Structured Metadata**: Explicitly defines data types and categories, reducing ambiguity.
    ''' - **Human-Readable**: Easy to edit and interpret compared to binary formats.
    ''' - **Compatibility**: Native support in Weka and tools like Python's `liac-arff` library.
    ''' - **Flexibility**: Handles sparse data (e.g., `{1 26, 6 63}` for non-zero values).
    ''' 
    ''' ---
    ''' 
    ''' ### 3. **Use Cases**
    ''' 
    ''' - **Machine Learning**: Standard format for training models in Weka (e.g., decision trees, clustering).
    ''' - **Data Preprocessing**: Supports missing value imputation (`?`) and categorical encoding.
    ''' - **Research and Education**: Widely used in academic datasets (e.g., UCI repositories).
    ''' 
    ''' ---
    ''' 
    ''' ### 4. **Comparison with Other Formats**
    ''' 
    ''' | **Feature**        | **ARFF**                            | **CSV**                     |
    ''' |--------------------|-------------------------------------|-----------------------------|
    ''' | **Metadata**       | Explicit (types, categories)        | Implicit (no type info)     |
    ''' | **Readability**    | High (structured headers)           | Moderate (flat structure)   |
    ''' | **Missing Values** | Supported (`?`)                     | Often ad-hoc (e.g., blanks) |
    ''' | **Tools**          | Weka, Python (`liac-arff`, `scipy`) | Universal                   |
    ''' 
    ''' ---
    ''' 
    ''' ### 5. **Limitations**
    ''' 
    ''' - **Verbosity**: Header definitions can be lengthy for large datasets.
    ''' - **Limited Sparse Data Support**: Requires specific syntax for sparse entries.
    ''' - **Format Rigidity**: Sensitive to line breaks and spacing.
    ''' 
    ''' ---
    ''' 
    ''' ### 6. **Tools and Libraries**
    ''' 
    ''' - **Weka**: Native support for ARFF; includes visualization and preprocessing tools.
    ''' - **Python**:  
    '''   - `liac-arff`: Read/write ARFF files with Pandas integration.  
    '''   - `scipy.io.arff`: Basic ARFF parsing.
    ''' - **Conversion Tools**: Convert CSV/XLS to ARFF using Weka CLI or GUI.
    ''' 
    ''' ---
    ''' 
    ''' ### Example ARFF File
    ''' 
    ''' ```arff
    ''' @relation iris
    ''' @attribute sepal_length numeric
    ''' @attribute sepal_width numeric
    ''' @attribute class {Iris-setosa, Iris-versicolor, Iris-virginica}
    ''' @data
    ''' 5.1,3.5,Iris-setosa
    ''' 4.9,3.0,Iris-setosa
    ''' 7.0,?,Iris-versicolor  // Missing value
    ''' ```
    ''' 
    ''' For more details, refer to Weka's documentation or explore sample datasets 
    ''' like `weather.arff`.
    ''' </summary>
    Public Module ArffReader

        Public Function LoadDataFrame(arff As Stream) As DataFrame
            Dim fields As New Dictionary(Of String, List(Of String))
            Dim attrs As New Dictionary(Of String, (Integer, String))
            Dim str As New StreamReader(arff)
            Dim line As Value(Of String) = ""
            Dim name As String = Nothing
            Dim desc As New StringBuilder

            Do While LCase(line = str.ReadLine) <> "@data"
                If line.ToLower.StartsWith("@relation") Then
                    name = line.GetTagValue(" ", trim:=True).Value
                    name = name.Trim(""""c)
                ElseIf line.ToLower.StartsWith("@attribute") Then
                    Dim attr As String = line.GetTagValue(" ", trim:=True).Value
                    Dim kv = Tokenizer.CharsParser(attr, delimiter:=" "c,).ToArray

                    Call fields.Add(kv(0), New List(Of String))
                    Call attrs.Add(kv(0), (attrs.Count, kv.Skip(1).JoinBy(" ")))
                ElseIf line.StartsWith("%"c) Then
                    Call desc.AppendLine(CStr(line).TrimStart("%").Trim(" "c))
                End If
            Loop

            Dim offsets = attrs.Keys.ToArray

            Do While (line = str.ReadLine) IsNot Nothing
                Dim r As String() = Tokenizer.CharsParser(line).ToArray

                For i As Integer = 0 To offsets.Length - 1
                    Call fields(offsets(i)).Add(r(i))
                Next
            Loop

            Dim fieldData As New Dictionary(Of String, FeatureVector)

            For Each field In fields
                Call fieldData.Add(field.Key, ParseFeature(field.Key, attrs(field.Key).Item2, field.Value))
            Next

            Return New DataFrame With {
                .description = desc.ToString,
                .name = name,
                .features = fieldData
            }
        End Function

        Private Function ParseFeature(name$, type$, data As List(Of String)) As FeatureVector
            Select Case LCase(type)
                Case "real", "numeric", "double"
                    Return New FeatureVector(name, data.AsDouble)
                Case "bool", "boolean", "logical"
                    Return New FeatureVector(name, data.AsBoolean)
                Case "int", "integer", "int32"
                    Return New FeatureVector(name, data.AsInteger)
                Case "float"
                    Return New FeatureVector(name, data.AsSingle)
                Case "long", "int64"
                    Return New FeatureVector(name, data.AsLong)
                Case Else
                    Return New FeatureVector(name, data)
            End Select
        End Function

        Public Function GetCommentText(arff As Stream) As String
            Dim str As New StreamReader(arff)
            Dim line As Value(Of String) = ""
            Dim comment As New StringBuilder

            Do While (line = str.ReadLine) IsNot Nothing
                If line.StartsWith("%"c) Then
                    Call comment.AppendLine(CStr(line).TrimStart("%").Trim(" "c))
                ElseIf line.ToLower = "@data" Then
                    Exit Do
                End If
            Loop

            Return comment.ToString
        End Function
    End Module
End Namespace