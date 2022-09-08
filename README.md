## 🦄 wwm.swagger-api

> wwm.swagger-api is a powerful swagger json document to api js, use .NET 6 .

<span>English</span> |  <a href="README.zh-CN.md">中文</a>

## feature
- [√] Export the get parameter
- [√] Export the post parameter
- [√] Custom export file location
- [√] By controller name to generate the file
- [√] The file name and interface initials are lowercase
- [√] Read the local interface file
- [√] Read the network interface file
- [√] Support query parameters in post request
- [√] Support delete request
- [√] Support put request
- [√] Support request with variable parameter in path
- [√] Method name for subsequent operations of the custom interface
- [√] Support excluded path name string or regular expression (exclude after match)
- [√] Support to generate TypeScript files

## 📚 Documentation
How to use: 

> Option 1: Use alone

Download [wwm.swagger-api release](https://github.com/wwmin/wwm.swagger-api/releases)

Then modify the 'wwm.swagger-api.json' configuration key, and then run cmd to execute the 'wwm.swagger-api.exe' program

- wwm.swagger-api.json config file
```json
{
  "ScriptType": "ts", // js or ts
  "OutPath": "./api/", // The output path, which can be relative and absolute, is not nullable
  "JsonUrl": "http://localhost:5000/swagger/Default/swagger.json", // The path to read the swagger.json, or local swagger.json file, and cannot be empty
  "FileHeadText": "/**\n * from wwm.swagger-api tool generated\n */", // Custom file header information, nullable
  "FuncTailParameter": "loading: boolean = true", // Function tail parameter, nullable
  "ApiFolderName": "api", // API folder name, its nullable, default: api
  "ApiInterfaceFolderName": "apiInterface", // API interface folder name, nullable, default: apiInterface
  "ImportHttp": "import http from \"../index\";", // Import the http module, which can be nullable, default:import http from \"../index\";
  "IndentSpaceNum": 2, // Indents the number of spaces, nullable, default:2
  "RemoveUnifyWrapObjectName": "Data" // Remove the global wrapper return field, and if it is empty, use the full field type to return
}
```


> way 2: Integrate into front-end package.json

Put the downloaded file in the front-end project root directory and configure 'wwm.swagger-api.json'(same with option 1)

Added under scripts in package.json
```
"scripts": {
	"api": "wwm.swagger-api"
},
```

Or use a relative path

```
"scripts": {
	"api": ".\\swagger\\wwm.swagger-api"
},
```

Then execute the `npm run api`
## pack release
dotnet pack

## 🗄 License

[MIT](https://opensource.org/licenses/MIT)

Copyright (c) 2021-present, wwmin
