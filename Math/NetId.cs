using System;
using System.Linq;
using System.Text;

using KableNet.Common;

namespace KableNet.Math
{

    public class NetId
    {
        private NetId( string networkIdentifier )
        {
            rawNetId = networkIdentifier;
        }

        private string rawNetId = "__NULL__";

        public string GetRaw( )
        {
            return rawNetId;
        }

        public static NetId Empty
        {
            get { return new NetId( "__NULL__" ); }
        }

        public static bool operator ==( NetId primary, NetId secondary )
        {
            if ( primary is null && !( secondary is null ) )
            {
                return false;
            }
            else if ( secondary is null && !( primary is null ) )
            {
                return false;
            }
            else if ( primary is null )
            {
                // Both are null?
                return true;
            }
            else
            {
                return primary.Equals( secondary );
            }
        }
        public static bool operator !=( NetId primary, NetId secondary )
        {
            if ( primary is null && !( secondary is null ) )
            {
                return true;
            }
            else if ( secondary is null && !( primary is null ) )
            {
                return true;
            }
            else if ( primary is null )
            {
                // Both are null?
                return false;
            }
            else
            {
                return !primary.Equals( secondary );
            }
        }

        public override bool Equals( object? obj )
        {
            if ( obj is NetId )
            {
                NetId tarNetId = (NetId)obj;
                if ( tarNetId.rawNetId.Equals( rawNetId ) )
                {
                    return true;
                }
            }

            return false;
        }

        public static NetId Generate( )
        {
            return new NetId( GenerateIdent( KableHelper.rand ) );
        }

        public override string ToString( )
        {
            return $"<{rawNetId}>";
        }

        public static NetId Parse( string rawString )
        {
            if ( rawString.Length == validLength )
            {
                if ( rawString.All( x => validChars.Contains( x ) ) )
                {
                    return new NetId( rawString );
                }
            }
            return null;
        }
        public static bool TryParse( string rawString, out NetId reference )
        {
            reference = Parse( rawString );
            if ( reference != null )
            {
                return true;
            }
            return false;
        }

        private static string GenerateIdent( Random rand )
        {
            int length = validLength;
            StringBuilder strBuilder = new StringBuilder( );
            while ( length-- > 0 )
            {
                strBuilder.Append( validChars[ rand.Next( validChars.Length ) ] );
            }

            return strBuilder.ToString( );
        }

        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890$@#";
        const int validLength = 26;
    }
}