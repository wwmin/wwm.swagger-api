## 🦄 wwm.swagger-api
> wwm.swagger-api 是一个有用的 swagger json 文档 转api.js/api.ts dotnet cli 工具, 支持 .NET 5 及以上.

<a href="README.md">English</a> |  <span>中文</span>

## 功能
- [√] 导出get参数
- [√] 导出post参数
- [√] 自定义导出位置
- [√] 按照controller name分文件
- [√] 文件名及接口名首字母小写
- [√] 读取本地接口文件
- [√] 读取网络接口文件
- [√] 支持post请求中带有query参数
- [√] 支持delete请求
- [√] 支持put请求
- [√] 支持path中带有变量参数请求
- [√] 自定义接口后续操作的方法名
- [√] 支持排除的路径名字符串或正则表达式(匹配后排除)
- [√] 支持生成TypeScript文件

## 📚 文档说明
使用方式: 

 > 方式1: 单独使用

下载[wwm.swagger-api release](https://github.com/wwmin/wwm.swagger-api/releases)

然后修改`wwm.swagger-api.json` 配置项, 然后运行 cmd 执行 `wwm.swagger-api.exe` 程序

> 方式2: 集成到前端package.json

将下载的文件放到前端项目根目录, 配置好`wwm.swagger-api.json`

在package.json的scripts下添加
```
"scripts": {
	"api": "wwm.swagger-api"
},
```

或者使用相对路径

```
"scripts": {
	"api": ".\\swagger\\wwm.swagger-api"
},
```

然后执行 `npm run api`


## 打包
dotnet pack

## 🗄 许可证

[MIT](LICENSE)