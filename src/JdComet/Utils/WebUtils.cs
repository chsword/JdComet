using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Linq;
namespace JdComet
{
    /// <summary>
    /// 网络工具类。
    /// </summary>
    sealed class WebUtils
    {
        /// <summary>
        /// 组装普通文本请求参数。
        /// </summary>
        /// <param name="parameters">Key-Value形式请求参数字典</param>
        /// <returns>URL编码后的请求数据</returns>
        public static string BuildQuery(IDictionary<string, string> parameters)
        {
            var postData = new StringBuilder();
            bool hasParam = false;

            IEnumerator<KeyValuePair<string, string>> dem = parameters.GetEnumerator();
            while (dem.MoveNext())
            {
                string name = dem.Current.Key;
                string value = dem.Current.Value;
                // 忽略参数名或参数值为空的参数
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value)) continue;
                if (hasParam)
                {
                    postData.Append("&");
                }
                postData.Append(name).Append("=").Append(HttpUtility.UrlEncode(value, Encoding.UTF8));
                hasParam = true;
            }

            return postData.ToString();
        }
    }
}
