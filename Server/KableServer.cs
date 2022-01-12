using System;
using System.Net;
using System.Net.Sockets;

using KableNet.Common;

namespace KableNet.Server
{
    /// <summary>
    /// Server Listener for KableNet
    /// </summary>
    public class KableServer
    {
        /// <summary>
        /// Initializes a KableNet Server on the specified port bound to "0.0.0.0"
        /// and starts listening.
        /// </summary>
        /// <param name="port"></param>
        public KableServer( int port )
        {
            _socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
            _socket.Bind( new IPEndPoint( IPAddress.Any, port ) );
            _socket.NoDelay = true;
            _socket.Listen( 10 );
        }

        public void StartListening( )
        {
            StartTCPAccept( );
        }

        private void OnTCPAcceptCallback( IAsyncResult ar )
        {
            try
            {
                Socket sock = _socket.EndAccept( ar );
                if ( sock != null )
                {
                    KableConnection conn = new KableConnection( sock );

                    NewConnectionEvent?.Invoke( conn );
                }
                // If its null, just continue the loop I guess?
                // Im not sure what would cause that situation, so ill deal
                // with it when/if it happens.

                StartTCPAccept( );
            }
            catch ( SocketException ex )
            {
                NewConnectionErroredEvent?.Invoke( $"[SocketException]New Connection Error'd!\n{ex.ToString( )}" );
            }
            catch ( Exception ex )
            {
                NewConnectionErroredEvent?.Invoke( $"[Exception]New Connection Error'd!\n{ex.ToString( )}" );
            }
        }

        private void StartTCPAccept( )
        {
            _socket.BeginAccept( new AsyncCallback( OnTCPAcceptCallback ), _socket );
        }

        public delegate void NewConnection( KableConnection connection );
        public event NewConnection NewConnectionEvent;

        public delegate void NewConnectionSocketError( string errorMessage );
        public event NewConnectionSocketError NewConnectionErroredEvent;

        private Socket _socket;
    }
}
