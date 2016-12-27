# Schema
_namespace: [Microsoft.VisualBasic.Serialization.JSON](./index.md)_

Here is a basic example of a JSON Schema:
 
 ```json
 {
 "title": "Example Schema",
 "type": "object",
 "properties": {
 "firstName": {
 "type": "string"
 },
 "lastName": {
 "type": "string"
 },
 "age": {
 "description": "Age in years",
 "type": "integer",
 "minimum": 0
 }
 },
 "required": ["firstName", "lastName"]
 }
 ```




