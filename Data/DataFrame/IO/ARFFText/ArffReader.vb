Imports System.IO

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

        Public Function LoadDataFrame(arff As Stream) As FeatureVector

        End Function
    End Module
End Namespace