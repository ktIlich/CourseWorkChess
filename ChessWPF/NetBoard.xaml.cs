using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChessWPF.ServiceChess;
using ChessLib;
using System.Windows.Threading;
using System.Threading;
using System.Text.RegularExpressions;

namespace ChessWPF {
  /// <summary>
  /// Логика взаимодействия для NetBoard.xaml
  /// </summary>
  public partial class NetBoard : UserControl, IServiceChessCallback {

    Random rnd = new Random( );

    ServiceChessClient client;
    Board board;
    Chess chess;
    ToolBar tool;
    MoveViewer viewer;
    DispatcherTimer onlineTimer;
    DispatcherTimer connectTimer;
    DispatcherTimer sessionTimer;

    ConnectForm login;
    ClientChoise clientChoise;
    ChatControl chat_ctrl;

    int ID { get; set; }
    int Game_ID { get; set; }
    string Color { get; set; } = "White";
    public string UserName { get; set; }
    bool isFindGame { get; set; } = false;
    bool isConnected { get; set; } = false;



    public NetBoard( ) {
      InitializeComponent( );
      NetInit( );
    }


    private void InitClientChoise( ) {
      if ( isConnected ) {
        clientChoise.btn_disconnect.Click -= DisconnectUserBtn;
        clientChoise.btn_disconnect.Click += DisconnectUserBtn;
        clientChoise.btn_join.Click -= JoinGameBtn;
        clientChoise.btn_join.Click += JoinGameBtn;
        clientChoise.btn_create.Click -= CreateGameBtn;
        clientChoise.btn_create.Click += CreateGameBtn;

        try {
          int[ ] arr_online = client.GetOnlineCount( );
          clientChoise.online_display.Content = $"Всего онлайн : {arr_online[ 0 ]} / созданно лобби : {arr_online[ 1 ]} / игроков в очереди : {arr_online[ 2 ]}";
          onlineTimer = new DispatcherTimer( TimeSpan.FromSeconds( 5 ) ,
            DispatcherPriority.Normal , new EventHandler( onlineTimer_Tick ) ,
            Dispatcher );
          onlineTimer.Start( );
        }
        catch {
          try {
            DisconnectUser( );
          }
          catch {
            MessageBox.Show( $"Ошибка подключения!\nПопробуйте позже" , "Ошибка" , MessageBoxButton.OK , MessageBoxImage.Error );
            client = null;
            isConnected = false;

            dock_content.Children.Clear( );
            dock_content.Children.Add( login );
          }
        }

      }
    }

    private void NetInit( ) {
      if ( !isConnected ) {
        login = new ConnectForm( );
        login.btn_connect.Click -= ConnectUserBtn;
        login.btn_connect.Click += ConnectUserBtn;

        dock_content.Children.Clear( );
        dock_content.Children.Add( login );
      }
    }

    private void InitNetBoard( ) {
      if ( isConnected && isFindGame ) {
        chess = new Chess( );
        board = new Board( );
        board.GSChess = chess;
        tool = new ToolBar( board );
        board.WithBot = false;
        board.WithOnline = true;
        board.SetFigureChess( );
        viewer = new MoveViewer( );
        viewer.Board = board;
        viewer.Chess = chess;
        chess.MoveStack.Clear( );
        chess.FenStack.Clear( );
        board.NewMoveNet -= NewMoveNet;
        board.NewMoveNet += NewMoveNet;

        tool.Undo.IsEnabled = false;
        tool.Redo.IsEnabled = false;

        chat_ctrl = new ChatControl( );
        chat_ctrl.tbInput.KeyDown -= chat_send_enter;
        chat_ctrl.tbInput.KeyDown += chat_send_enter;
        chat_ctrl.btn_send.Click -= chat_send_btn;
        chat_ctrl.btn_send.Click += chat_send_btn;
        chat_ctrl.btn_disconnect.Click -= DisconnectUserBtn;
        chat_ctrl.btn_disconnect.Click += DisconnectUserBtn;

        dock_content.Children.Clear( );
        dock_content.Children.Add( tool );
        dock_content.Children.Add( chat_ctrl );
        dock_content.Children.Add( viewer );
        dock_content.Children.Add( board );
        DockPanel.SetDock( tool , Dock.Top );
        DockPanel.SetDock( chat_ctrl , Dock.Right );
        DockPanel.SetDock( viewer , Dock.Left );

        double temp = dock_content.ActualWidth / 3;
        tool.Undo.Margin = new Thickness( temp , 0 , 0 , 0 );
        board.MinWidth = 350;
        board.MaxWidth = 550;

        sessionTimer = new DispatcherTimer( TimeSpan.FromSeconds( 1 ) ,
          DispatcherPriority.Normal , new EventHandler( sessionTimer_tick ) ,
          Dispatcher );
        sessionTimer.Start( );

        InfoGame( );

        if ( Color == "Black" ) {
          board.IsEnabled = false;
        }
      }
    }


    private void ConnectUser( ) {
      if ( !isConnected ) {
        client = new ServiceChessClient( new System.ServiceModel.InstanceContext( this ) );
        string str = login.login_input.Text;
        string pattern = @"[A-Za-z0-9_]{3,20}";

        #region Check login
        if ( String.IsNullOrWhiteSpace( str ) ) {
          UserName = "player_" + rnd.Next( 13245 , 14898787 );
          MessageBox.Show( $"Не корректный никнейм! \nВам был сгенерирован следующий никнейм: \n{ UserName }" , "Предупреждение" , MessageBoxButton.OK , MessageBoxImage.Exclamation );
        }
        else if ( Regex.IsMatch( str , pattern , RegexOptions.None ) ) {
          UserName = str;
        }
        else if ( str.Length > 20 ) {
          UserName = str.Substring( 0 , 20 );
          MessageBox.Show( "Ваш ник был укорочен до 20 символов" , "Предупреждение" , MessageBoxButton.OK , MessageBoxImage.Exclamation );
        }
        else {
          UserName = "player_" + rnd.Next( 13245 , 14898787 );
          MessageBox.Show( $"Не корректный никнейм! \nВам был сгенерирован следующий никнейм: \n{ UserName }" , "Предупреждение" , MessageBoxButton.OK , MessageBoxImage.Exclamation );
        }
        #endregion


        try {
          ID = client.Connect( UserName );
          isConnected = true;
          MessageBox.Show( $"Вы подключились к серверу!" , "Успех" , MessageBoxButton.OK , MessageBoxImage.Information );

          dock_content.Children.Clear( );
          clientChoise = new ClientChoise( );
          dock_content.Children.Add( clientChoise );

          InitClientChoise( );
        }
        catch {
          try {
            DisconnectUser( );
          }
          catch {
            MessageBox.Show( $"Ошибка подключения!\nПопробуйте позже" , "Ошибка" , MessageBoxButton.OK , MessageBoxImage.Error );
            client = null;
            isConnected = false;

            dock_content.Children.Clear( );
            dock_content.Children.Add( login );
          }
        }
      }
    }

    private void JoinGame( ) {
      try {
        client.ConnectSession( ID );
        ProgressBar progress = new ProgressBar( );
        progress.IsIndeterminate = true;
        progress.Height = 20;
        progress.Foreground = new SolidColorBrush( ( Color )ColorConverter.ConvertFromString( "#769656" ) );
        progress.Name = "prg";
        clientChoise.title.Content = "Вы в очереди. Ожидайте подключения к доступной игре.";
        clientChoise.row_btn.Visibility = Visibility.Collapsed;
        clientChoise.row_color.Visibility = Visibility.Collapsed;
        clientChoise.container.Children.Add( progress );

        connectTimer = new DispatcherTimer( TimeSpan.FromSeconds( 0.3 ) ,
         DispatcherPriority.Normal , new EventHandler( connectTimer_tick ) ,
         Dispatcher );
        connectTimer.Start( );
      }
      catch {
        try {
          DisconnectUser( );
        }
        catch {
          MessageBox.Show( $"Ошибка подключения!\nПопробуйте позже" , "Ошибка" , MessageBoxButton.OK , MessageBoxImage.Error );
          client = null;
          isConnected = false;

          dock_content.Children.Clear( );
          dock_content.Children.Add( login );
        }
      }
    }

    private void CreateGame( ) {
      if ( isConnected ) {
        try {
          Game_ID = client.CreateSession( ID );
          client.SetColor( Game_ID , Color );
          ProgressBar progress = new ProgressBar( );
          progress.IsIndeterminate = true;
          progress.Height = 20;
          progress.Foreground = new SolidColorBrush( ( Color )ColorConverter.ConvertFromString( "#769656" ) );
          progress.Name = "prg";
          clientChoise.title.Content = "Лобби созданно. Ожидайте подключения оппонента.";
          clientChoise.row_btn.Visibility = Visibility.Collapsed;
          clientChoise.row_color.Visibility = Visibility.Collapsed;
          clientChoise.container.Children.Add( progress );

          connectTimer = new DispatcherTimer( TimeSpan.FromSeconds( 0.2 ) ,
          DispatcherPriority.Normal , new EventHandler( connectTimer_tick ) ,
           Dispatcher );
          connectTimer.Start( );
        }
        catch {
          try {
            DisconnectUser( );
          }
          catch {
            MessageBox.Show( $"Ошибка подключения!\nПопробуйте позже" , "Ошибка" , MessageBoxButton.OK , MessageBoxImage.Error );
            client = null;
            isConnected = false;

            dock_content.Children.Clear( );
            dock_content.Children.Add( login );
          }
        }
      }
    }

    private void DisconnectUser( ) {
      if ( isConnected ) {
        try {
          client.Disconnect( ID );
          if ( isFindGame ) {
            client.DeleteFromQueue( ID );
          }
          MessageBox.Show( $"Вы отключились от сервера!" , "Успех" , MessageBoxButton.OK , MessageBoxImage.Information );
          client = null;
          isConnected = false;
          isFindGame = false;
        }
        catch {
          try {
            DisconnectUser( );
          }
          catch {
            MessageBox.Show( $"Ошибка подключения!\nПопробуйте позже" , "Ошибка" , MessageBoxButton.OK , MessageBoxImage.Error );
            client = null;
            isConnected = false;

            dock_content.Children.Clear( );
            dock_content.Children.Add( login );
          }
        }
      }
    }



    public void MsgCallback( string msg ) {
      TextBlock block = new TextBlock( );
      block.Text = msg;
      block.TextWrapping = TextWrapping.Wrap;
      chat_ctrl.chat_display.Items.Add( block );
      chat_ctrl.chat_display.ScrollIntoView( chat_ctrl.chat_display.Items[ chat_ctrl.chat_display.Items.Count - 1 ] );
    }

    public void MoveCallBack( string msg ) {
      board.DoMoveNet( msg );
      board.SetFigureChess( );
      if ( chess.IsCheckmate( ) || chess.IsStalemate( ) ) {
        NetBoardResult( chess.IsCheckmate( ), chess.IsStalemate( ) );
      }
    }

    private void NetBoardResult( bool mate , bool state ) {
      dock_content.Children.Clear( );
      clientChoise = new ClientChoise( );
      dock_content.Children.Add( clientChoise );

      InitClientChoise( );
      if ( mate ) {
        string win_color = board.GSChess.GetCurrentColor( ) == "Black" ? "Победили черные" : "Победили белые";
        clientChoise.title.Content = $"Мат! {win_color}";
      }
      if ( state ) {
        clientChoise.title.Content = $"Ничья! Победителей нет.";
      }
    }

    private void InfoGame( ) {
      if ( isConnected && isFindGame ) {
        try {
          string msg = client.GetGameInfo( Game_ID );
          if ( msg != "" ) {
            MsgCallback( msg );
          }
        }
        catch {
          try {
            DisconnectUser( );
          }
          catch {
            MessageBox.Show( $"Ошибка подключения!\nПопробуйте позже" , "Ошибка" , MessageBoxButton.OK , MessageBoxImage.Error );
            client = null;
            isConnected = false;

            dock_content.Children.Clear( );
            dock_content.Children.Add( login );
          }
        }
      }
    }



    private void NewMoveNet( object sender , NewMoveEventArgs e ) {
      if ( board.GSChess.GetAllMoves( ).Count > 0 && client != null ) {
        try {
          client.SendMove( board.GSChess.MoveStack.CurrentMove , ID , Game_ID );
        }
        catch {
          try {
            DisconnectUser( );
          }
          catch {
            MessageBox.Show( $"Ошибка подключения!\nПопробуйте позже" , "Ошибка" , MessageBoxButton.OK , MessageBoxImage.Error );
            client = null;
            isConnected = false;

            dock_content.Children.Clear( );
            dock_content.Children.Add( login );
          }
        }
      }
    }




    private void ConnectUserBtn( object sender , RoutedEventArgs e ) {
      ConnectUser( );
      login.login_input.Text = string.Empty;
    }

    private void CreateGameBtn( object sender , RoutedEventArgs e ) {
      if ( isConnected ) {
        clientChoise.title.Content = "Выберите цвет";
        clientChoise.row_btn.Visibility = Visibility.Collapsed;
        clientChoise.row_color.Visibility = Visibility.Visible;
        clientChoise.btn_prev.Click -= PrevToChoice;
        clientChoise.btn_prev.Click += PrevToChoice;
        clientChoise.btn_commit.Click -= CommitGameBtn;
        clientChoise.btn_commit.Click += CommitGameBtn;
      }
    }

    private void CommitGameBtn( object sender , RoutedEventArgs e ) {
      Color = clientChoise.btn_white.IsChecked == true ?
        "White" :
        "Black";
      CreateGame( );
    }

    private void PrevToChoice( object sender , RoutedEventArgs e ) {
      clientChoise.title.Content = "Создать игру или подключиться?";
      clientChoise.row_btn.Visibility = Visibility.Visible;
      clientChoise.row_color.Visibility = Visibility.Collapsed;
    }

    private void JoinGameBtn( object sender , RoutedEventArgs e ) {
      JoinGame( );
    }

    private void chat_send_btn( object sender , RoutedEventArgs e ) {
      if ( isConnected && isFindGame ) {
        if ( client != null ) {
          try {
            client.SendMesg( chat_ctrl.tbInput.Text , ID , Game_ID );
            chat_ctrl.tbInput.Text = string.Empty;
          }
          catch {
            try {
              DisconnectUser( );
            }
            catch {
              MessageBox.Show( $"Ошибка подключения!\nПопробуйте позже" , "Ошибка" , MessageBoxButton.OK , MessageBoxImage.Error );
              client = null;
              isConnected = false;

              dock_content.Children.Clear( );
              dock_content.Children.Add( login );
            }
          }
        }
      }
    }

    private void chat_send_enter( object sender , KeyEventArgs e ) {
      if ( isConnected && isFindGame ) {
        if ( e.Key == Key.Enter ) {
          if ( client != null ) {
            try {
              client.SendMesg( chat_ctrl.tbInput.Text , ID , Game_ID );
              chat_ctrl.tbInput.Text = string.Empty;
            }
            catch {
              try {
                DisconnectUser( );
              }
              catch {
                MessageBox.Show( $"Ошибка подключения!\nПопробуйте позже" , "Ошибка" , MessageBoxButton.OK , MessageBoxImage.Error );
                client = null;
                isConnected = false;

                dock_content.Children.Clear( );
                dock_content.Children.Add( login );
              }
            }
          }
        }
      }
    }

    private void DisconnectUserBtn( object sender , RoutedEventArgs e ) {
      if ( isConnected ) {
        DisconnectUser( );
        dock_content.Children.Clear( );
        dock_content.Children.Add( login );
        if ( onlineTimer != null ) {
          onlineTimer.Stop( );
        }
        if ( connectTimer != null ) {
          connectTimer.Stop( );
        }
        if ( sessionTimer != null ) {
          sessionTimer.Stop( );
        }
      }
    }





    private void connectTimer_tick( object sender , EventArgs e ) {
      if ( isConnected ) {
        try {
          int temp = client.StartGame( ID );
          if ( temp != -1 ) {
            Game_ID = temp;
            connectTimer.Stop( );
            onlineTimer.Stop( );
            isFindGame = true;
            Color = client.GetColor( ID , Game_ID );
            InitNetBoard( );
          }
        }
        catch {
          try {
            DisconnectUser( );
          }
          catch {
            MessageBox.Show( $"Ошибка подключения!\nПопробуйте позже" , "Ошибка" , MessageBoxButton.OK , MessageBoxImage.Error );
            client = null;
            isConnected = false;

            dock_content.Children.Clear( );
            dock_content.Children.Add( login );
          }
        }
      }
    }

    private void sessionTimer_tick( object sender , EventArgs e ) {
      if ( chess.GetAllMoves( ).Count > 0 ) {
        GameTimer gameTimer;
        gameTimer = board.GameTimer;
        tool.labelWhitePlayTime.Content = GameTimer.GetHumanElapse( gameTimer.WhitePlayTime );
        tool.labelBlackPlayTime.Content = GameTimer.GetHumanElapse( gameTimer.BlackPlayTime );


        if ( isConnected && isFindGame ) {
          if ( board.GSChess.GetCurrentMoveNumber( ) == 2 ) {
            try {
              client.DeleteFromQueue( ID );
            }
            catch {
              try {
                DisconnectUser( );
              }
              catch {
                MessageBox.Show( $"Ошибка подключения!\nПопробуйте позже" , "Ошибка" , MessageBoxButton.OK , MessageBoxImage.Error );
                client = null;
                isConnected = false;

                dock_content.Children.Clear( );
                dock_content.Children.Add( login );
              }
            }
          }

          try {
            switch ( client.CheckState( Game_ID ) ) {
              case 1:
                MsgCallback( $"Белый игрок отключился." );
                board.IsEnabled = false;
                chat_ctrl.btn_send.IsEnabled = false;
                sessionTimer.Stop( );
                break;
              case -1:
                MsgCallback( $"Черный игрок отключился." );
                board.IsEnabled = false;
                chat_ctrl.btn_send.IsEnabled = false;
                sessionTimer.Stop( );
                break;
              case 0: break;
              case 10: break;
            }
          }
          catch {
            try {
              DisconnectUser( );
            }
            catch {
              MessageBox.Show( $"Ошибка подключения!\nПопробуйте позже" , "Ошибка" , MessageBoxButton.OK , MessageBoxImage.Error );
              client = null;
              isConnected = false;

              dock_content.Children.Clear( );
              dock_content.Children.Add( login );
            }
          }

          if ( Color == board.GSChess.GetCurrentColor( ) ) {
            board.IsEnabled = true;
          }
          else if ( Color != board.GSChess.GetCurrentColor( ) ) {
            board.IsEnabled = false;
          }
        }
      }
    }

    private void onlineTimer_Tick( object sender , EventArgs e ) {
      if ( isConnected ) {
        try {
          int[ ] arr_online = client.GetOnlineCount( );
          clientChoise.online_display.Content = $"Всего онлайн : {arr_online[ 0 ]} / созданно лобби : {arr_online[ 1 ]} / игроков в очереди : {arr_online[ 2 ]}";
        }
        catch {
          try {
            DisconnectUser( );
          }
          catch {
            MessageBox.Show( $"Ошибка подключения!\nПопробуйте позже" , "Ошибка" , MessageBoxButton.OK , MessageBoxImage.Error );
            client = null;
            isConnected = false;

            dock_content.Children.Clear( );
            dock_content.Children.Add( login );
          }
        }
      }
    }


    private void UserControl_SizeChanged( object sender , SizeChangedEventArgs e ) {
      if ( isConnected && isFindGame ) {
        double temp = dock_content.ActualWidth / 3;
        tool.Undo.Margin = new Thickness( temp , 0 , 0 , 0 );
        viewer.Margin = new Thickness( 10 );
        chat_ctrl.Margin = new Thickness( 10 );
      }
    }
  }
}
