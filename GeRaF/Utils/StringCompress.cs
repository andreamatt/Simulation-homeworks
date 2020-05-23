using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Utils
{
	class StringCompress
	{
		public static string Compress(string s) {
			var bytes = Encoding.Unicode.GetBytes(s);
			using (var msi = new MemoryStream(bytes))
			using (var mso = new MemoryStream()) {
				using (var gs = new GZipStream(mso, CompressionMode.Compress)) {
					msi.CopyTo(gs);
				}
				return Convert.ToBase64String(mso.ToArray());
			}
		}
	}
}
