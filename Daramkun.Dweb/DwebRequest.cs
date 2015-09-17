using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Daramkun.Dweb
{
	public sealed class DwebRequest : IDisposable
	{
		public DwebProtocol Protocol { get; private set; }
		Stream protocolStream;
		_HeaderParser protocolParser;

		DwebRequestMethod? method;
		Uri uri;
		Version version;
		Dictionary<string, object> field;
		bool bodyIsAlreadyCopied = false;
		Dictionary<string, object> body;

		public DwebRequestMethod Method
		{
			get
			{
				if ( method == null )
					method = ( DwebRequestMethod ) Enum.Parse ( typeof ( DwebRequestMethod ), protocolParser.ReadToSpace () );
				return method.Value;
			}
		}

		public Uri Uri
		{
			get
			{
				if ( method == null ) { var temp = Method; }
				if ( uri == null )
					uri = new Uri ( protocolParser.ReadToSpace (), UriKind.Relative );
				return uri;
			}
		}

		public Version HttpVersion
		{
			get
			{
				if ( uri == null ) { var temp = Uri; }
				if ( version == null )
					version = new Version ( protocolParser.ReadToSpace ().Substring ( 5 ) );
				return version;
			}
		}

		public IReadOnlyDictionary<string, object> Field
		{
			get
			{
				if ( version == null ) { var temp = HttpVersion; }
				if ( field == null )
				{
					protocolParser.SkipToNextLine ();
					field = new Dictionary<string, object> ();
					string key;
					while ( ( key = protocolParser.ReadToColon () ) != null )
						if ( !field.ContainsKey ( key ) )
						{
							object data = protocolParser.ReadToNextLine ().Trim ();
							switch ( key )
							{
								case DwebHeaderField.ContentLength: data = long.Parse ( data as string ); break;
								case DwebHeaderField.ContentType: data = new ContentType ( data as string ); break;
								case DwebHeaderField.Date: data = DateTime.Parse ( data as string ); break;
							}
                            field.Add ( key, data );
						}
				}
				return field;
			}
		}

		static readonly ContentType urlEncoded = new ContentType ( "application/x-www-form-urlencoded" );

		public IReadOnlyDictionary<string, object> Body
		{
			get
			{
				if ( field == null ) { var temp = Field; }
				if ( body == null )
				{
					if ( bodyIsAlreadyCopied ) return null;

					body = new Dictionary<string, object> ();
					if ( field.ContainsKey ( DwebHeaderField.ContentLength ) )
					{
						long contentLength = ( long ) field [ DwebHeaderField.ContentLength ];
						if ( contentLength > 0 )
						{
							if ( field [ DwebHeaderField.ContentType ] == urlEncoded )
							{
								MemoryStream memoryStream = new MemoryStream ();
								int length = 0;
								byte [] dataBuffer = new byte [ 4096 ];
								while ( length < contentLength )
								{
									int len = protocolStream.Read ( dataBuffer, 0, dataBuffer.Length );
									length += len;
									memoryStream.Write ( dataBuffer, 0, len );
								}

								string postString = HttpUtility.UrlDecode ( memoryStream.ToArray (), 0, ( int ) memoryStream.Length, Encoding.UTF8 );
								string [] tt = postString.Split ( '&' );
								tt = tt [ 1 ].Split ( '&' );
								foreach ( string s in tt )
								{
									string [] temp2 = s.Split ( '=' );
									body.Add ( temp2 [ 0 ], ( temp2.Length == 2 ) ? HttpUtility.UrlDecode ( temp2 [ 1 ] ) : null );
								}
							}
							else
							{
								ContentType contentType = field [ DwebHeaderField.ContentType ] as ContentType;
								ReadMultipartPOSTData ( ( long ) field [ DwebHeaderField.ContentLength ], contentType.Boundary );
                            }
						}
					}
				}
				return body;
			}
		}

		private void ReadMultipartPOSTData ( long contentLength, string boundary )
		{
			bool partSeparatorMode = true, firstLooping = true;
			Stream tempStream = null;
			Dictionary<string, object> fields = new Dictionary<string, object> ();
			byte [] multipartData = new byte [ 4 + boundary.Length ];
			Array.Copy ( new byte [] { ( byte ) '\r', ( byte ) '\n', ( byte ) '-', ( byte ) '-' }, multipartData, 4 );
			Array.Copy ( Encoding.UTF8.GetBytes ( boundary ), 0, multipartData, 4, boundary.Length );
			for ( int i = 0; i < 2 + boundary.Length; ++i )
				protocolStream.ReadByte ();

			byte [] separatorReaded = new byte [ 2 ];
			while ( true )
			{
				if ( partSeparatorMode )
				{
					if ( !firstLooping )
					{
						string filename = _Utility.ReadFilename ( fields [ DwebHeaderField.ContentDisposition ] as string );
						body.Add ( _Utility.ReadName ( fields [ DwebHeaderField.ContentDisposition ] as string ),
							filename ?? Encoding.UTF8.GetString ( ( tempStream as MemoryStream ).ToArray () ) );
					}

					protocolStream.Read ( separatorReaded, 0, 2 );
					if ( Encoding.UTF8.GetString ( separatorReaded ) == "--" )
					{
						if ( tempStream != null ) tempStream.Dispose ();
						return;
					}
					else
					{
						string key;
						fields.Clear ();
						while ( ( key = protocolParser.ReadToColon () ) != null )
							fields.Add ( key, protocolParser.ReadToNextLine ().Trim () );
						string filename = _Utility.ReadFilename ( fields [ DwebHeaderField.ContentDisposition ] as string );
						if ( tempStream != null ) tempStream.Dispose ();
						tempStream = ( filename == null ) ?
							new MemoryStream () as Stream :
							new FileStream (
								Path.Combine ( Protocol.Listener.TemporaryDirectory,
								Convert.ToBase64String ( Encoding.UTF8.GetBytes ( filename ) ) + Path.GetExtension ( filename )
							), FileMode.Create ) as Stream;
						partSeparatorMode = false;
					}
				}
				else
				{
					byte b;
					int multipartHeaderIndex = 0;
					Queue<byte> queue = new Queue<byte> ();
					while ( true )
					{
						b = ( byte ) protocolStream.ReadByte ();
						if ( b == multipartData [ multipartHeaderIndex ] )
						{
							queue.Enqueue ( b );
							++multipartHeaderIndex;
							if ( multipartHeaderIndex == multipartData.Length )
							{
								partSeparatorMode = true;
								queue.Clear ();
								break;
							}
						}
						else
						{
							while ( queue.Count != 0 ) tempStream.WriteByte ( queue.Dequeue () );
							if ( b == multipartData [ 0 ] ) { queue.Enqueue ( b ); multipartHeaderIndex = 1; }
							else { tempStream.WriteByte ( b ); multipartHeaderIndex = 0; }
						}
					}
				}
				firstLooping = false;
			}
		}
		
		internal DwebRequest ( DwebProtocol protocol )
		{
			Protocol = protocol;
			protocolStream = protocol.Stream;
			protocolParser = new _HeaderParser ( protocolStream );
		}

		public void Dispose ()
		{
			field = null;
			body = null;
			Protocol.request = null;
		}

		public async void LoadHeaderAsync () { await Task.Run ( () => { var temp = Field; } ); }

		public void BodyCopyTo ( Stream stream )
		{
			byte [] dataBuffer = new byte [ 1024 ];
			if ( field.ContainsKey ( DwebHeaderField.ContentLength ) )
			{
				long contentLength = ( long ) field [ DwebHeaderField.ContentLength ];
				long length = 0;
				while ( length != contentLength )
				{
					int len = protocolStream.Read ( dataBuffer, 0, 1024 );
					length += len;
					stream.Write ( dataBuffer, 0, len );
				}
			}

			bodyIsAlreadyCopied = true;
		}

		public async void BodyCopyToAsync ( Stream stream ) { await Task.Run ( () => BodyCopyTo ( stream ) ); }
	}
}
