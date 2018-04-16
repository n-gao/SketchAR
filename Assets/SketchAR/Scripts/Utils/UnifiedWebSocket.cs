using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
#if WINDOWS_UWP
using System.Collections.Concurrent;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Buffer = Windows.Storage.Streams.Buffer;
using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;
using System.Runtime.InteropServices.WindowsRuntime;
#else
using WebSocketSharp;
using WebSocketSharp.Server;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;
#endif

/// <summary>
/// This class offers a wrapper around the WebSocketSharp and Windows UWP implementation of the websocket protocol. The first one is
/// used when running in editor and the second one is used when running on HoloLens.
/// </summary>
public class UnifiedWebSocket
{
    private string _serverUri = "";

    /// <summary>
    /// This event will be called everytime as message has been received.
    /// </summary>
    public event MessageListener OnMessageReceived;
    /// <summary>
    /// This event will be called everytime an error occurs.
    /// </summary>
    public event ErrorListener OnErrorOccured;
    /// <summary>
    /// This event will be called when the connection has been closed.,
    /// </summary>
    public event ConnectionCloseLinstener OnClosed;
    /// <summary>
    /// This event will be called when a connection has been established.
    /// </summary>
    public event Action OnConnected;

    public delegate void MessageListener(string message);

    public delegate void ErrorListener(string message);

    public delegate void ConnectionCloseLinstener(ushort code, string reason);

    private bool _connected = false;

    /// <summary>
    /// Connects to the given uri. The uri should start with ws:// or sws:// and should contains the targeted port.
    /// </summary>
    /// <param name="uri">Target uri to which a connection should be established</param>
    public void ConnectTo(string uri)
    {
        if (string.IsNullOrEmpty(uri))
        {
            throw new ArgumentException("[WebSocket]Address is invalid.\n" + uri);
        }
        _serverUri = uri;
#if WINDOWS_UWP
        InitUwp();
#else
        InitWinRT();
#endif
    }

    /// <summary>
    /// Sends a message over the established connection.
    /// </summary>
    /// <param name="message">Message to be sent.</param>
    public void SendMessage(object message)
    {
#if WINDOWS_UWP
        _toSend.Add(message);
#else
        _toSend.Enqueue(message);
#endif
    }

    /// <summary>
    /// If a websocket connection is open it will be closed using this method.
    /// </summary>
    public void Close()
    {
        if (_webSocket != null)
        {
#if WINDOWS_UWP
            try
            {
                _webSocket.Close(1000, "Closed due to user request.");
                _webSocket.Dispose();
                _webSocket = null;
            }
            catch (Exception e)
            {
                Debug.LogError("Error while disconnecting.\n" + e.Message);
            }
#else
            _webSocket.Close();
#endif
        }
    }

    /// <summary>
    /// Indicates whether the websocket is currently connected.
    /// </summary>
    public bool Connected
    {
        get { return _connected; }
    }

    // Windows UWP implementation
#if WINDOWS_UWP
    private MessageWebSocket _webSocket;

    private DataWriter _writer;
    private Task _writeTask;
    private CancellationTokenSource _writerCancellationToken;
    private readonly BlockingCollection<object> _toSend = new BlockingCollection<object>();
        
    private StreamSocketListener _listener = new StreamSocketListener();

    private async void InitUwp()
    {
        _webSocket = new MessageWebSocket();
        _webSocket.Control.MessageType = SocketMessageType.Utf8;
        _webSocket.MessageReceived += WebSock_MessageReceived;
        _webSocket.Closed += WebSock_Closed;
        Uri uri = new Uri(_serverUri);
        try
        {
            await _webSocket.ConnectAsync(uri);
            _connected = true;
            OnConnected?.Invoke();
            WebSock_InitSendLoop();
        }
        catch (Exception)
        {
            _connected = false;
        }
    }

    private void WebSock_InitSendLoop()
    {
        _writer = new DataWriter(_webSocket.OutputStream);
        _writerCancellationToken = new CancellationTokenSource();

        _writeTask = Task.Factory.StartNew(async () =>
        {
            while (_connected)
            {
                try
                {
                    string message = _toSend.Take().ToString();
                    try
                    {
                        _writer.WriteString(message);
                        await _writer.StoreAsync();
                    }
                    catch (Exception)
                    {
                        _toSend.Add(message);
                    }
                }
                catch (Exception)
                {
                    //Ignored 
                }
            }
        }, _writerCancellationToken.Token, TaskCreationOptions.LongRunning | TaskCreationOptions.RunContinuationsAsynchronously, TaskScheduler.Default);
    }

    private void WebSock_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
    {
        DataReader messageReader = args.GetDataReader();
        messageReader.UnicodeEncoding = UnicodeEncoding.Utf8;
        string messageString = messageReader.ReadString(messageReader.UnconsumedBufferLength);
        OnMessageReceived?.Invoke(messageString);
        messageReader.Dispose();
    }

    //The Closed event handler
    private void WebSock_Closed(IWebSocket sender, WebSocketClosedEventArgs args)
    {
        OnClosed?.Invoke(args.Code, args.Reason);
        _writerCancellationToken.Cancel();
        _connected = false;
    }
// WebsocketSharp Implementation
#else
    private readonly BlockingQueue<object> _toSend = new BlockingQueue<object>();
    private WebSocket _webSocket;
    private Thread _sendThread;


    void InitWinRT()
    {
        _webSocket = new WebSocket(_serverUri);
        _webSocket.OnMessage += WebSocketOnMessage;
        _webSocket.OnClose += WebSocketOnClose;
        _webSocket.OnOpen += WebSocketOnOpen;
        _webSocket.OnError += WebSocketOnError;
        _webSocket.ConnectAsync();
    }

    private void WebSocketOnError(object sender, ErrorEventArgs e)
    {
        if (OnErrorOccured != null)
        {
            OnErrorOccured(e.Message);
        }
    }

    private void WebSocketOnOpen(object sender, EventArgs e)
    {
        _connected = true;
        if (OnConnected != null)
        {
            OnConnected.Invoke();
        }
        _sendThread = new Thread(() =>
        {
            while (_connected)
            {
                WebSocketSendMessage(_toSend.Dequeue());
            }
        });
        _sendThread.Start();
    }

    private void WebSocketOnClose(object sender, CloseEventArgs e)
    {
        _connected = false;
        if (OnClosed != null)
        {
            OnClosed(e.Code, e.Reason);
        }
        _toSend.Enqueue(null);
    }

    private void WebSocketOnMessage(object sender, MessageEventArgs e)
    {
        if (OnMessageReceived != null)
        {
            OnMessageReceived(e.Data);
        }
    }

    void WebSocketSendMessage(object message)
    {
        if (!_webSocket.IsAlive) return;
        _webSocket.Send(message.ToString());
        //        Debug.Log("SendMessage\n"+message.ToString());
    }
#endif
}