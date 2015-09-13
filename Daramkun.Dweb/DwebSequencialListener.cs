using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.Dweb
{
	public class DwebSequencialListener : DwebListener
	{
		public DwebSequencialListener ( IPEndPoint endPoint )
			: base ( endPoint )
		{

		}

		public DwebProtocol GetConnected ()
		{
			Socket socket = ListenSocket.Accept ();
			return new DwebProtocol ( this, socket );
		}

		public async Task<DwebProtocol> GetConnectedAsync ()
		{
			Func<DwebProtocol> run = GetConnected;
			return await Task.Run ( run );
		}
	}
}
