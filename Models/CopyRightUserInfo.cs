using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace swagger2js_cli.Models
{
    /// <summary>
    /// 授权信息
    /// </summary>
    public class CopyRightUserInfo
    {
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public DateTime CreateTime { get; set; }
        public string FileRemark { get; set; }
    }
}
