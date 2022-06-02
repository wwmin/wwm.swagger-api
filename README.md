## 🦄 wwm.swagger-api

> wwm.swagger-api is a powerful swagger json document to api js, supports .NET 5 .

    English |  中文

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

> way 2: Integrate into front-end package.json

Put the downloaded file in the front-end project root directory and configure 'wwm.swagger-api.json'

Added under scripts in package.json
```
"scripts": {
	"api": "wwm.swagger-api"
},
```

Or use a relative path

```
"scripts": {
	"api": ".swaggerwwm.swagger-api"
},
```

Then execute the 'npm run api'
## pack release
dotnet pack

## 🗄 License

[MIT](https://opensource.org/licenses/MIT)

Copyright (c) 2021-present, wwmin