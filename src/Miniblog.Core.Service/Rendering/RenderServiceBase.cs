using System.Text;
using System.Threading.Tasks;
using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Service.Rendering
{
	public abstract class RenderServiceBase : IRenderService {
		public virtual string GetGravatar(IComment comment)
		{
			using (var md5 = System.Security.Cryptography.MD5.Create())
			{
				byte[] inputBytes = Encoding.UTF8.GetBytes(comment.Email.Trim().ToLowerInvariant());
				byte[] hashBytes = md5.ComputeHash(inputBytes);

				// Convert the byte array to hexadecimal string
				var sb = new StringBuilder();
				for (int i = 0; i < hashBytes.Length; i++)
				{
					sb.Append(hashBytes[i].ToString("X2"));
				}

				return $"https://www.gravatar.com/avatar/{sb.ToString().ToLowerInvariant()}?s=60&d=blank";
			}
		}

		public virtual string RenderContent(IComment comment)
		{
			return comment.Content;
		}

		public abstract string RenderContent(IPost post);		
	}
}