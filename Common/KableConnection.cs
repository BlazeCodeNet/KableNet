using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace KableNet.Common
{
    /// <summary>
    /// Represents a Kable-Networking connection. Both Client AND Server side will have this sort
    /// of instance for communicaiton.
    /// </summary>
    public class KableConnection
    {
        public int maxProcessIterations = 3;

        /// <summary>
        /// Only call this if you are ClientSided. ServerSided already
        /// handles this via the client's connection.
        /// </summary>
        public void Connect( )
        {
            try
            {
                tcpConnection.BeginConnect( new IPEndPoint( address, port ), ConnectCallback, null );
            }
            catch ( SocketException ex )
            {
                ConnectErroredEvent?.Invoke( ex, this );
                connected = false;
            }
            catch ( Exception ex )
            {
                ConnectionErroredEvent?.Invoke( ex, this );
                connected = false;
            }
        }

        /// <summary>
        /// Enables background thread processing of the packets
        /// WARNING: Does NOT work with Unity3D Engine
        /// </summary>
        public void EnableBackgroundProcessing( )
        {
            backgroundProcessing = true;
        }

        /// <summary>
        /// Starts the Async Callback for reading TCP data from the socket
        /// </summary>
        private void BeginRecieve( )
        {
            try
            {
                tcpConnection.BeginReceive( tcpBuffer, 0, tcpBuffer.Length, SocketFlags.None, new AsyncCallback( OnTCPRecvCallback ), null );
            }
            catch ( SocketException ex )
            {
                ConnectErroredEvent?.Invoke( ex, this );
                connected = false;
            }
            catch ( Exception ex )
            {
                ConnectionErroredEvent?.Invoke( ex, this );
                connected = false;
            }
        }

        /// <summary>
        /// Used ClientSide for completing the Async connection to the TCP server
        /// </summary>
        /// <param name="AR"></param>
        private void ConnectCallback( IAsyncResult AR )
        {
            try
            {
                tcpConnection.EndConnect( AR );
                connected = true;
                ConnectedEvent?.Invoke( this );
            }
            catch ( SocketException ex )
            {
                ConnectErroredEvent?.Invoke( ex, this );
                connected = false;
            }
            catch ( Exception ex )
            {
                ConnectionErroredEvent?.Invoke( ex, this );
                connected = false;
            }

            if ( connected )
            {
                BeginRecieve( );
            }
        }

        public async Task SendPacketTCPAsync( KablePacket packet )
        {
            await SendPacketTCPAsync( packet.GetRaw( ) );
        }

        /// <summary>
        /// Sends a TCP buffer through the KableConnection to the recieving end.
        /// </summary>
        /// <param name="packetBuffer">Bytes to send</param>
        /// <returns></returns>
        public async Task SendPacketTCPAsync( List<byte> packetBuffer )
        {
            try
            {
                if ( tcpConnection != null && connected )
                {
                    List<byte> sendBuffer = new List<byte>( );

                    // Get the amount of bytes as a UInt and convert it to bytes.
                    // This goes as a suffix to our actual payload btyes to tell the
                    // recieving end how many bytes we're sending.
                    sendBuffer.AddRange( BitConverter.GetBytes( (uint)packetBuffer.Count ) );
                    sendBuffer.AddRange( packetBuffer );

                    byte[ ] sendBufferArray = sendBuffer.ToArray( );

                    // Make sure we account for differences in LittleEdian!
                    if ( !BitConverter.IsLittleEndian )
                    {
                        Array.Reverse( sendBufferArray );
                    }

                    tcpConnection.Send( sendBufferArray );
                }
            }
            catch ( SocketException ex )
            {
                ConnectErroredEvent?.Invoke( ex, this );
                connected = false;
            }
            catch ( Exception ex )
            {
                ConnectionErroredEvent?.Invoke( ex, this );
                connected = false;
            }
        }

        /// <summary>
        /// Callback for the Async TCP reading
        /// </summary>
        /// <param name="ar"></param>
        private void OnTCPRecvCallback( IAsyncResult ar )
        {
            try
            {
                if ( tcpConnection != null )
                {
                    int bytesRead = tcpConnection.EndReceive( ar );

                    Console.Write( $"READ { bytesRead } Bytes" );

                    // Make sure we account for differences in LittleEdian!
                    if ( !BitConverter.IsLittleEndian )
                    {
                        Array.Reverse( tcpBuffer );
                    }

                    // Check that we actually read something, otherwise error
                    if ( bytesRead > 0 )
                    {
                        lock ( packetBuffer )
                        {
                            // Get only the read bytes; ignore the excess data
                            // I dont know if this is needed, but ill remove later if its not.
                            packetBuffer.AddRange( new List<byte>( tcpBuffer ).GetRange( 0, bytesRead ) );
                        }
                    }
                    else
                    {
                        // We didnt read any data. Assume the connection was terminated and
                        // throw a error for it.
                        ConnectionErroredEvent?.Invoke( new Exception( "[KableConnection_Error]Connection was lost: Read zero bytes!" ), this );
                        connected = false;
                    }
                }
            }
            catch ( SocketException ex )
            {
                ConnectErroredEvent?.Invoke( ex, this );
                connected = false;
            }
            catch ( Exception ex )
            {
                ConnectionErroredEvent?.Invoke( ex, this );
                connected = false;
            }

            if ( connected )
            {
                if ( backgroundProcessing )
                {
                    ProcessBuffer( );
                }

                BeginRecieve( );
            }
        }

        /// <summary>
        /// Process's the entire network buffer(to an extent) and
        /// triggeres events for the processed packets.
        /// 
        /// You MUST call this in order to recieve
        /// packet data events!
        /// </summary>
        public void ProcessBuffer( )
        {
            bool again = true;
            int againCount = 0;

            // Dont rerun this loop more than maxProcessIterations times.
            while ( again && againCount < maxProcessIterations )
            {
                againCount++;
                if ( packetBuffer.Count > 0 )
                {
                    if ( pendingPacket == null )
                    {
                        if ( packetBuffer.Count >= SizeHelper.Normal )
                        {
                            int newPayloadSize = -1;

                            lock ( packetBuffer )
                            {
                                newPayloadSize = BitConverter.ToInt32( packetBuffer.ToArray( ), 0 );
                            }

                            pendingPacket = new PendingPacket
                            {
                                payloadSize = newPayloadSize,
                            };
                            // Change tmpBuffer to the suffix of data after our "Payload Size" marker
                            lock ( packetBuffer )
                            {
                                packetBuffer = packetBuffer.GetRange( SizeHelper.Normal, packetBuffer.Count - SizeHelper.Normal );
                            }
                        }
                        else
                        {
                            // If this executes then we have no pending packet
                            // AND the read size is too small to tell us the
                            // size of a new pending packet. Continue on next
                            // iteration and break the loop so we can check again next iteration.
                            break;
                        }
                    }

                    if ( packetBuffer.Count >= pendingPacket.payloadSize )
                    {
                        // Add current buffer's data to the pendingPacket to fill it up more
                        lock ( packetBuffer )
                        {
                            pendingPacket.currentPayload.AddRange( packetBuffer.GetRange( 0, pendingPacket.payloadSize ) );
                        }
                        // Check if the pendingPacket is full...
                        if ( pendingPacket.currentPayload.Count >= pendingPacket.payloadSize )
                        {
                            // its full! Raise the event and then if we have enough data, repeat this loop.
                            try
                            {
                                PacketReadyEvent?.Invoke( new KablePacket( pendingPacket.currentPayload ), this );
                            }
                            catch ( Exception ex )
                            {
                                // Crash from a subscriber to the event.
                                // Not sure how to handle these, so for now just ignore it and continue?
                                // Will try to figure out a better solution later, of course
                                ConnectionErroredEvent?.Invoke( ex, this );
                            }

                            lock ( packetBuffer )
                            {
                                packetBuffer = packetBuffer.GetRange( pendingPacket.payloadSize, packetBuffer.Count - pendingPacket.payloadSize );
                            }

                            if ( packetBuffer.Count >= SizeHelper.Normal )
                            {
                                again = true;
                            }
                            pendingPacket = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ClientSide way to get a KableConnection instance.
        /// Make sure to call Connect()!
        /// </summary>
        /// <param name="address">Address to connect to</param>
        /// <param name="port">Port to connect to</param>
        public KableConnection( IPAddress address, int port )
        {
            this.address = address;
            this.port = port;
            tcpConnection = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
            tcpConnection.NoDelay = true;
            tcpBuffer = new byte[ SizeHelper.Buffer ];
            udpConnection = null;
            connected = false;
        }
        /// <summary>
        /// ServerSide way to get a KableConnection instance.
        /// </summary>
        /// <param name="activeTCPSocket">The raw TCP Socket.</param>
        internal KableConnection( Socket activeTCPSocket )
        {
            tcpConnection = activeTCPSocket;

            activeTCPSocket.NoDelay = true;

            connected = true;

            address = null;
            connected = true;
            tcpBuffer = new byte[ SizeHelper.Buffer ];

            BeginRecieve( );
        }

        public bool connected { get; private set; } = false;
        public Socket? tcpConnection { get; private set; }
        public Socket? udpConnection { get; private set; }
        public IPAddress address { get; private set; }
        public int port { get; private set; }

        public bool backgroundProcessing { get; private set; } = false;

        private byte[ ] tcpBuffer = null;
        private List<byte> packetBuffer = new List<byte>( );
        private PendingPacket? pendingPacket;

        public delegate void KableConnectErrored( SocketException exception, KableConnection source );
        public event KableConnectErrored ConnectErroredEvent;

        public delegate void KableConnectionErrored( Exception ex, KableConnection source );
        public event KableConnectionErrored ConnectionErroredEvent;

        public delegate void KablePacketReady( KablePacket packet, KableConnection source );
        public event KablePacketReady PacketReadyEvent;

        public delegate void KableConnected( KableConnection source );
        public event KableConnected ConnectedEvent;

        /// <summary>
        /// Used for simple data storage about the current "pending packet" while 
        /// we wait for it to fill.
        /// </summary>
        private class PendingPacket
        {
            public int payloadSize { get; set; }
            public List<byte> currentPayload { get; } = new List<byte>( );
        }
    }
}
