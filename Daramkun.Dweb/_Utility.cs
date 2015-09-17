using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Daramkun.Dweb
{
	static class _Utility
	{
		public static string ReadName ( string disposition )
		{
			Regex regex = new Regex ( "name=\"(.*)\"" );
			Match match = regex.Match ( disposition );
			return match.Groups [ 1 ].Value;
		}

		public static string ReadFilename ( string disposition )
		{
			Regex regex = new Regex ( "filename=\"(.*)\"" );
			Match match = regex.Match ( disposition );
			if ( match == null || match.Groups.Count == 1 ) return null;
			return match.Groups [ 1 ].Value;
		}

		/*public static string GetFilename ( string baseDirectory, HttpUrl url, int startIndex )
		{
			StringBuilder filename = new StringBuilder ();
			filename.Append ( baseDirectory );
			if ( filename [ filename.Length - 1 ] == '\\' )
				filename.Remove ( filename.Length - 1, 1 );

			for ( int i = startIndex; i < url.Path.Count; ++i )
				filename.AppendFormat ( "\\{0}", url.Path [ i ] );

			return filename.ToString ();
		}*/
	}
}
