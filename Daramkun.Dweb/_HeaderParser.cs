using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.Dweb
{
	class _HeaderParser
	{
		Stream stream;
		byte [] buffer = new byte [ 1 ];
		byte [] buffer2 = new byte [ 4096 ];
		
		public _HeaderParser ( Stream stream ) { this.stream = stream; }

		public string ReadToSpace ()
		{
			StringBuilder builder = new StringBuilder ();
			while ( stream.Read ( buffer, 0, 1 ) == 1 && buffer [ 0 ] != ' ' )
				builder.Append ( ( char ) buffer [ 0 ] );
			return builder.ToString ();
		}

		public string ReadToColon ()
		{
			StringBuilder builder = new StringBuilder ();
			while ( stream.Read ( buffer, 0, 1 ) == 1 && buffer [ 0 ] != ':' )
			{
				if ( buffer [ 0 ] == '\r' ) { stream.Read ( buffer, 0, 1 ); return null; }
				builder.Append ( ( char ) buffer [ 0 ] );
			}
			return builder.ToString ();
		}

		public string ReadToNextLine ()
		{
			StringBuilder builder = new StringBuilder ();
			bool isStr = false;
			while ( stream.Read ( buffer, 0, 1 ) == 1 && buffer [ 0 ] == ' ' ) ;
			if ( buffer [ 0 ] != ' ' ) builder.Append ( ( char ) buffer [ 0 ] );
			if ( buffer [ 0 ] == '"' ) isStr = true;
			while ( stream.Read ( buffer, 0, 1 ) == 1 && ( buffer [ 0 ] != '\r' || isStr ) )
			{
				builder.Append ( ( char ) buffer [ 0 ] );
				if ( buffer [ 0 ] == '"' ) isStr = !isStr;
			}
			stream.ReadByte ();
			return builder.ToString ();
		}

		public void SkipToNextLine ()
		{
			do stream.Read ( buffer, 0, 1 ); while ( buffer [ 0 ] != '\r' );
			stream.Read ( buffer, 0, 1 );
		}
	}
}
