using System;
using System.Collections.Generic;
using System.Text;

using KableNet.Math;

namespace KableNet.Common
{

    /// <summary>
    /// The packet data class for KableNet
    /// Used for both reading AND writing a packet
    /// </summary>
    public class KablePacket
    {
        public KablePacket( )
        {
            rawBytes = new List<byte>( );
        }
        public KablePacket( List<byte> bytes )
        {
            rawBytes = bytes;
        }

        public void ResetReadPosition( )
        {
            readPosition = 0;
        }

        public List<byte> GetRaw( )
        {
            return new List<byte>( rawBytes );
        }
        public List<byte> GetUnreadBytes( )
        {
            return rawBytes.GetRange( readPosition, rawBytes.Count - readPosition );
        }
        public int GetReadPosition( )
        {
            return readPosition;
        }

        public void Write( byte[ ] data )
        {
            rawBytes.AddRange( data );
        }
        public void Write( List<byte> data )
        {
            rawBytes.AddRange( data );
        }
        public void Write( short data )
        {
            Write( BitConverter.GetBytes( data ) );
        }
        public void Write( int data )
        {
            Write( BitConverter.GetBytes( data ) );
        }
        public void Write( float data )
        {
            Write( BitConverter.GetBytes( data ) );
        }
        public void Write( ulong data )
        {
            Write( BitConverter.GetBytes( data ) );
        }
        public void Write( bool data )
        {
            Write( BitConverter.GetBytes( data ) );
        }
        public void Write( string data )
        {
            List<byte> tmpBuff = new List<byte>( );
            byte[ ] tmpStringBytes = Encoding.Unicode.GetBytes( data );
            int sizeMarker = tmpStringBytes.Length;

            tmpBuff.AddRange( BitConverter.GetBytes( sizeMarker ) );
            tmpBuff.AddRange( tmpStringBytes );

            Write( tmpBuff );
        }
        public void Write( NetId netId )
        {
            Write( netId.GetRaw( ) );
        }
        public void Write( Identifier identifier )
        {
            if ( identifier is null )
            {
                throw new Exception( "[KablePacket]Write(Identifier) was given NULL!" );
            }
            Write( identifier.path );
            Write( identifier.value );
        }
        public void Write( Vec3f data )
        {
            if ( data is null )
            {
                throw new Exception( "[KablePacket]Write(Vec3f) was given NULL!" );
            }
            Write( data.x );
            Write( data.y );
            Write( data.z );
        }

        public List<byte> ReadBytes( int length )
        {
            return rawBytes.GetRange( readPosition, length );
        }
        public string ReadString( )
        {
            int sizeMarker = BitConverter.ToInt32( rawBytes.GetRange( readPosition, SizeHelper.Normal ).ToArray( ), 0 );
            readPosition += SizeHelper.Normal;

            string ret = Encoding.Unicode.GetString( rawBytes.GetRange( readPosition, sizeMarker ).ToArray( ) );
            readPosition += sizeMarker;
            return ret;
        }
        public Vec3f ReadVec3f( )
        {
            float x = ReadFloat( );
            float y = ReadFloat( );
            float z = ReadFloat( );

            return new Vec3f( x, y, z );
        }
        public bool ReadBool( )
        {
            bool ret = BitConverter.ToBoolean( rawBytes.ToArray( ), readPosition );
            readPosition += SizeHelper.Bool;
            return ret;
        }
        public float ReadFloat( )
        {
            float ret = BitConverter.ToSingle( rawBytes.ToArray( ), readPosition );
            readPosition += SizeHelper.Normal;
            return ret;
        }
        public int ReadInt( )
        {
            int ret = BitConverter.ToInt32( rawBytes.ToArray( ), readPosition );
            readPosition += SizeHelper.Normal;
            return ret;
        }
        public short ReadShort( )
        {
            short ret = BitConverter.ToInt16( rawBytes.ToArray( ), readPosition );
            readPosition += SizeHelper.Small;
            return ret;
        }
        public ulong ReadULong( )
        {
            ulong ret = BitConverter.ToUInt64( rawBytes.ToArray( ), readPosition );
            readPosition += SizeHelper.Large;
            return ret;
        }
        public Identifier ReadIdentifier( )
        {
            string retPath = ReadString( );
            string retValue = ReadString( );

            return new Identifier( retPath, retValue );
        }
        public NetId? ReadNetId( )
        {
            NetId ret;
            if ( NetId.TryParse( ReadString( ), out ret ) )
            {
                return ret;
            }

            return null;
        }

        public int Count
        {
            get
            {
                return rawBytes.Count;
            }
        }

        int readPosition = 0;
        List<byte> rawBytes;
    }
}
