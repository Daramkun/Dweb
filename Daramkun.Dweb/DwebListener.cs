using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.Dweb
{
	public class DwebListener : IDisposable
	{
		Socket listenSocket;
		protected Socket ListenSocket { get { return listenSocket; } }
		
		public string TemporaryDirectory { get; set; }
		public X509Certificate2 Certificate { get; set; }
		public Dictionary<string, ContentType> MimeTypes { get; private set; }

		public DwebListener ( IPEndPoint endPoint )
		{
			listenSocket = new Socket ( endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp );
			listenSocket.Bind ( endPoint );

			TemporaryDirectory = Environment.GetFolderPath ( Environment.SpecialFolder.InternetCache );

			MimeTypes = new Dictionary<string, ContentType>
			{
				{ "htm", new ContentType ( "text/html" ) },
                { "html", new ContentType ( "text/html" ) },
				{ "js", new ContentType ( "text/javascript" ) },
				{ "css", new ContentType ( "text/css" ) },
			};
		}

		~DwebListener () { Dispose ( false ); }

		protected virtual void Dispose ( bool disposing )
		{
			if ( disposing )
			{
				if ( listenSocket != null )
				{
					listenSocket.Dispose ();
					listenSocket = null;
				}
				else throw new ObjectDisposedException ( nameof ( DwebListener ) );
			}
		}

		public void Dispose ()
		{
			Dispose ( true );
			GC.SuppressFinalize ( this );
		}

		public void Listen ( int backlog = 5 )
		{
			listenSocket.Listen ( backlog );
		}
	}
}
