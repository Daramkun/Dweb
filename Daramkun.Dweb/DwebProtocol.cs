using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.Dweb
{
	public sealed class DwebProtocol
	{
		Stream stream, returnStream;

		internal DwebRequest request;

		public DwebListener Listener { get; private set; }
		internal Stream Stream { get { return stream; } }

		public DwebProtocol ( DwebListener listener, Socket connectedSocket )
		{
			Listener = listener;

			stream = new NetworkStream ( connectedSocket );
			if ( listener.Certificate != null )
			{
				stream = new SslStream ( stream );
				( stream as SslStream ).AuthenticateAsServer ( listener.Certificate );
			}

			returnStream = new MemoryStream ();
		}

		public DwebRequest ReceiveRequest ()
		{
			if ( request != null ) throw new InvalidOperationException ();
			return request = new DwebRequest ( this );
		}
	}
}
