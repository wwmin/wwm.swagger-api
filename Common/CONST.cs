using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wwm.swagger_api;


/// <summary>
/// 常量
/// </summary>
public static class CONST
{
    /// <summary>
    /// 脚本类型
    /// </summary>
    public static class ScriptType
    {
        /// <summary>
        /// JavaScript
        /// </summary>
        public static string JavaScript = "JavaScript";
        /// <summary>
        /// TypeScript
        /// </summary>
        public static string TypeScript = "TypeScript";
    }
}

/// <summary>
/// 脚本类型
/// </summary>
public enum TypeScriptEnum
{
    TypeScript,
    JavaScript
}
