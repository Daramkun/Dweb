using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.Dweb
{
	public sealed class DwebRequest : IDisposable
	{
		DwebProtocol protocol;
		Stream protocolStream;

		Version version;
		Uri uri;
		Dictionary<string, object> header;
		Stream body;

		public Version HttpVersion
		{
			get
			{
				if ( version == null )
				{

				}

				return version;
			}
		}

		public Uri Uri
		{
			get
			{
				if ( version == null )
				{
					var temp = HttpVersion;
				}

				if ( uri == null )
				{

				}

				return uri;
			}
		}

		public IReadOnlyDictionary<string, object> Header
		{
			get
			{
				if ( uri == null )
				{
					var temp = Uri;
				}

				if ( header == null )
				{
					header = new Dictionary<string, object> ();

				}
				return header;
			}
		}
		public Stream Body
		{
			get
			{
				if ( header == null )
				{
					var temp = Header;
				}

				if ( body == null )
				{
					body = new MemoryStream ();

				}
				return body;
			}
		}

		internal DwebRequest ( DwebProtocol protocol )
		{
			this.protocol = protocol;
			protocolStream = protocol.Stream;
		}

		public void Dispose ()
		{
			header = null;
			if ( body != null )
			{
				body.Dispose ();
				body = null;
			}
			protocol.request = null;
		}
	}
}
