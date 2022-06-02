namespace wwm.swaggerApi.Models;

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
