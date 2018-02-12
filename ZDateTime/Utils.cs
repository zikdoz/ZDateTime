using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ZDateTime
{
	public static class Utils
	{
		#region [ EXTENSIONS ]

		public static DateTime setYear( this DateTime date_time, int? new_year = null ) =>
			date_time.AddYears( new_year.GetValueOrDefault( DateTime.Now.Year ) - date_time.Year );

		public static DateTime setDayOfYear( this DateTime date_time, int? new_day_of_year = null ) =>
			date_time.AddDays( new_day_of_year.GetValueOrDefault( DateTime.Now.DayOfYear ) - 1 );

		#endregion

		#region [ CONVERTER ]

		public static ZData encodeDateTime( DateTime? date = null )
		{
			if ( !date.HasValue )
				date = DateTime.Now;

			return new ZData( date.Value, encodeDate( date ), encodeTime( date ) );
		}

		public static string encodeDate( DateTime? date = null )
		{
			if ( !date.HasValue )
				date = DateTime.Now;

			var doty = date.Value.DayOfYear;

			return date.Value.Year.ToString().Substring( 2, 2 ) // last 2 digits of year
					+ ( ( doty >= 365 ) ? '+' : ( char ) ( 65 + ( doty - 1 ) / 14 ) ) // letter of month
					+ ( ( doty - 1 ) % 14 + 1 ).ToString( "D2" ); // 2 digits of day
		}

		public static string encodeTime( DateTime? date = null )
		{
			if ( !date.HasValue )
				date = DateTime.Now;

			var ms = ( ( int ) ( ( date.Value - new DateTime( date.Value.Year, date.Value.Month, date.Value.Day ) ).TotalMilliseconds / 86.4 ) ).ToString( "D6" );

			return ms.Substring( 0, 3 ) + ":" + ms.Substring( 3 );
		}

		public static DateTime decodeDateTime( string encoded_date )
		{
			if ( string.IsNullOrWhiteSpace( encoded_date ) )
				throw new ArgumentNullException( nameof(encoded_date), ".//FATAL:\tNo data found.\n\t\tEncoded date appears to be null or empty." );

			var splitted = Regex.Split( encoded_date, "(\\d\\d[a-zA-Z+]\\d\\d)|(\\d\\d\\d:\\d\\d\\d)" ).Where( elem => !string.IsNullOrWhiteSpace( elem ) ).ToArray(); // split and throw out empty ones
			string decoded_date = null, decoded_time = null;

			// get matching parts
			for ( int i = 0, end = splitted.Length;
				( ( decoded_date == null ) || ( decoded_time == null ) )
				&& ( i < end );
				++i )
			{
				if ( ( decoded_date == null ) && ( Regex.IsMatch( splitted[ i ], "(\\d\\d[a-zA-Z+]\\d\\d)" ) ) )
					decoded_date = splitted[ i ].ToUpper();

				if ( ( decoded_time == null ) && ( Regex.IsMatch( splitted[ i ], "(\\d\\d\\d:\\d\\d\\d)" ) ) )
					decoded_time = splitted[ i ];
			}

			if ( ( decoded_time == null ) && ( decoded_date == null ) )
				throw new FormatException( ".//FATAL:\tNo data matched.\n\t\tEncoded date appears to be in wrong format." );

			// decode
			var result = new DateTime();

			result = result.setYear( ( decoded_date != null )
				? ( 2000 + int.Parse( decoded_date.Substring( 0, 2 ) ) )
				: new int?() );
			result = result.setDayOfYear( ( decoded_date != null )
				? ( ( ( decoded_date[ 2 ] == '+' ) ? 26 : decoded_date[ 2 ] - 65 ) * 14 + int.Parse( decoded_date.Substring( 3 ) ) )
				: new int?() );

			if ( decoded_time != null )
				result = result.Add( TimeSpan.FromSeconds( double.Parse( decoded_time.Remove( 3, 1 ) ) * 0.0864 ) );

			return result;
		}

		#endregion

		#region [ DATETIME ]

		public static DateTime getDateFromDay( int year, int day_of_year ) =>
			getDateFromYear( year ).AddDays( day_of_year - 1 );

		public static DateTime getDateFromYear( int year ) =>
			new DateTime( year, 1, 1 );

		#endregion
	}

	public struct ZData
	{
		public DateTime _source_time;
		public string _encoded_date, _encoded_time;

		public ZData( DateTime source_time, string encoded_date, string encoded_time )
		{
			_source_time = source_time;
			_encoded_date = encoded_date;
			_encoded_time = encoded_time;
		}

		public override string ToString() =>
			$"{_encoded_date} {_encoded_time}";
	}
}